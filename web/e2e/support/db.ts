import { spawnSync } from 'child_process';
import * as fs from 'fs';
import * as path from 'path';

// Lee la cadena de conexion del API (primero Development, luego la base) para
// no tener credenciales de la base de datos escritas en la prueba.
const API_DIR = path.resolve(__dirname, '../../../src/IRouteCommerce.Api');

// Ruta conocida de sqlcmd (ODBC 170) usada como respaldo si no esta en el PATH.
const SQLCMD_FALLBACK =
  'C:\\Program Files\\Microsoft SQL Server\\Client SDK\\ODBC\\170\\Tools\\Binn\\sqlcmd.exe';

interface DbConfig {
  server: string;
  database: string;
  user?: string;
  password?: string;
}

function readConnectionString(): string {
  for (const file of ['appsettings.Development.json', 'appsettings.json']) {
    const full = path.join(API_DIR, file);
    if (!fs.existsSync(full)) {
      continue;
    }
    const json = JSON.parse(fs.readFileSync(full, 'utf8'));
    const cs: string | undefined = json?.ConnectionStrings?.DefaultConnection;
    if (cs && !/TU_|YOUR_/i.test(cs)) {
      return cs;
    }
  }
  throw new Error('No se encontro DefaultConnection en appsettings del API.');
}

function parseConnectionString(cs: string): DbConfig {
  const map = new Map<string, string>();
  for (const part of cs.split(';')) {
    const idx = part.indexOf('=');
    if (idx > 0) {
      map.set(part.slice(0, idx).trim().toLowerCase(), part.slice(idx + 1).trim());
    }
  }
  return {
    server: map.get('server') ?? map.get('data source') ?? '.\\SQLEXPRESS',
    database: map.get('database') ?? map.get('initial catalog') ?? 'IRouteCommerce',
    user: map.get('user id') ?? map.get('uid'),
    password: map.get('password') ?? map.get('pwd')
  };
}

function resolveSqlcmd(): string {
  const probe = spawnSync('sqlcmd', ['-?'], { stdio: 'ignore' });
  return probe.error ? SQLCMD_FALLBACK : 'sqlcmd';
}

/**
 * Elimina los registros creados por una ejecucion de la prueba E2E.
 * Borra de commerce y commerce_quarantine los pc_id del rango indicado.
 * No lanza excepcion si la limpieza falla: solo deja una advertencia.
 */
export function deleteTestData(fromPcId: number, toPcId: number): void {
  let args: string[];
  try {
    const cfg = parseConnectionString(readConnectionString());
    args = ['-S', cfg.server, '-d', cfg.database];
    if (cfg.user) {
      args.push('-U', cfg.user, '-P', cfg.password ?? '');
    } else {
      args.push('-E');
    }
  } catch (error) {
    console.warn('[e2e] No se pudo determinar la conexion a la BD:', (error as Error).message);
    return;
  }

  const query =
    `DELETE FROM dbo.commerce_quarantine WHERE pc_id BETWEEN ${fromPcId} AND ${toPcId}; ` +
    `DELETE FROM dbo.commerce WHERE pc_id BETWEEN ${fromPcId} AND ${toPcId};`;
  args.push('-Q', query);

  const result = spawnSync(resolveSqlcmd(), args, { encoding: 'utf8' });

  if (result.error) {
    console.warn('[e2e] No se pudo limpiar la BD (sqlcmd no disponible):', result.error.message);
    return;
  }

  if (result.status !== 0) {
    console.warn('[e2e] Limpieza de BD finalizo con advertencia:', (result.stderr || '').trim());
  }
}
