import { defineConfig, devices } from '@playwright/test';

/**
 * Configuracion de Playwright para el flujo E2E completo.
 * Levanta automaticamente el API (.NET) y el frontend (Angular) si no estan activos.
 */
export default defineConfig({
  testDir: './e2e',
  timeout: 60_000,
  expect: { timeout: 10_000 },
  fullyParallel: false,
  workers: 1,
  retries: 1,
  reporter: [['list']],
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry'
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] }
    }
  ],
  webServer: [
    {
      command: 'dotnet run --project ../src/IRouteCommerce.Api --urls http://localhost:5062',
      url: 'http://localhost:5062/swagger/v1/swagger.json',
      reuseExistingServer: true,
      timeout: 180_000
    },
    {
      command: 'npm start -- --port 4200',
      url: 'http://localhost:4200',
      reuseExistingServer: true,
      timeout: 180_000
    }
  ]
});
