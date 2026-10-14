import { expect, test } from '@playwright/test';
import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';
import { deleteTestData } from './support/db';

const USERNAME = 'admin';
const PASSWORD = 'Admin123*';

// Fecha fija en el futuro para no chocar con datos reales.
const PROCESS_DATE = '2030-06-15';
const FILE_NAME = 'commerce_15062030.csv';

// Ids unicos por ejecucion (base de segundos); se suman 1..7.
const BASE_ID = Math.floor(Date.now() / 1000);

function buildCsv(): string {
  const header =
    'pc_id,pc_nomcomred,pc_numdoc,pc_tipdoc,pc_direccion,pc_departamento,' +
    'pc_provincia,pc_distrito,pc_telefono,pc_email,pc_processdate';

  const row = (offset: number, nomcomred: string, numdoc: string): string =>
    `${BASE_ID + offset},${nomcomred},${numdoc},1,Calle ${offset},Lima,Lima,Lima,` +
    `90000000${offset},correo${offset}@example.com,${PROCESS_DATE}`;

  const rows = [
    row(1, 'Comercial Uno', '12345678'), // valido
    row(2, 'Bodega Dos', '87654321'), // valido
    row(3, '', '10203040'), // nomcomred vacio
    row(4, 'Tienda Cuatro', ''), // numdoc vacio
    row(5, 'Distribuidora Cinco', '12AB5678'), // numdoc con letras
    row(6, 'Mayorista Seis', '1234-5678'), // numdoc con caracteres especiales
    row(7, 'Comercial Siete', '55667788') // valido
  ];

  return [header, ...rows].join('\n') + '\n';
}

let csvPath: string;

test.beforeAll(() => {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'iroute-e2e-'));
  csvPath = path.join(dir, FILE_NAME);
  fs.writeFileSync(csvPath, buildCsv(), 'utf8');
});

// Limpieza: elimina de la BD los registros creados por esta ejecucion
// para no dejar datos de prueba en el entorno de desarrollo.
test.afterAll(() => {
  deleteTestData(BASE_ID + 1, BASE_ID + 7);
});

test('flujo completo: login -> carga -> proceso -> cuarentena', async ({ page }) => {
  // 1) Login
  await page.goto('/login');
  await page.fill('input[name="username"]', USERNAME);
  await page.fill('input[name="password"]', PASSWORD);
  await page.getByRole('button', { name: 'Ingresar' }).click();
  await expect(page).toHaveURL(/\/carga$/);

  // 2) Carga con vista previa
  const [chooser] = await Promise.all([
    page.waitForEvent('filechooser'),
    page.locator('.file-drop').click()
  ]);
  await chooser.setFiles(csvPath);

  await expect(page.locator('table thead th').first()).toHaveText('pc_id');
  await expect(page.locator('table tbody tr')).toHaveCount(7);

  await page.getByRole('button', { name: 'Cargar registros' }).click();
  await expect(page.locator('.success')).toContainText('Se cargaron 7 registros');

  // 3) Procesar por fecha
  await page.getByRole('link', { name: 'Procesar' }).click();
  await expect(page).toHaveURL(/\/proceso$/);
  await page.fill('input[type="date"]', PROCESS_DATE);
  await page.getByRole('button', { name: 'Procesar' }).click();
  await expect(page.locator('.result-count .count')).toHaveText('4');

  // 4) Cuarentena: verificar los 4 registros de esta ejecucion
  await page.getByRole('link', { name: 'Cuarentena' }).click();
  await expect(page).toHaveURL(/\/cuarentena$/);
  await expect(page.locator('table')).toBeVisible();

  const rows = page.locator('table tbody tr');
  const total = await rows.count();
  const mine: string[] = [];

  for (let i = 0; i < total; i++) {
    const row = rows.nth(i);
    const idText = (await row.locator('td').first().innerText()).trim();
    const id = Number(idText);
    if (id >= BASE_ID + 1 && id <= BASE_ID + 7) {
      mine.push(await row.innerText());
    }
  }

  expect(mine).toHaveLength(4);

  const text = mine.join(' ');
  expect(text).toContain('nomcomred');
  expect(text).toContain('vacio');
  expect(text).toContain('letras');
  expect(text).toContain('especiales');
});
