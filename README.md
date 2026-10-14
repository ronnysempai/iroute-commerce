# iRoute — Prueba Técnica (Back .NET + Front Angular)

Aplicación para la gestión de comercio: carga masiva desde un archivo
`commerce_DDMMYYYY.csv`, previsualización antes de cargar, procesamiento por
fecha (`pc_processdate`) y consulta de los registros rechazados (cuarentena)
con su motivo. Incluye login opcional con JWT.

## Funcionalidad

1. **Carga de CSV** — se elige un archivo `commerce_DDMMYYYY.csv`, se
   **previsualiza** su contenido y luego se cargan los registros en la tabla
   `commerce`. Si el archivo está vacío se devuelve `400 Bad Request`.
2. **Procesar por fecha** — se valida una fecha `pc_processdate`. Los registros
   que no cumplen las reglas se **mueven a `commerce_quarantine`** guardando el
   `motivo`. Se devuelve la cantidad de registros enviados a cuarentena.
3. **Cuarentena** — lista de los registros rechazados con su `motivo`.
4. **Login (opcional)** — autenticación usuario/contraseña que emite un JWT.

### Reglas de validación (procesamiento)

- `pc_nomcomred` no debe estar vacío.
- `pc_numdoc` no debe estar vacío, ni contener letras ni caracteres especiales
  (solo dígitos).

El `motivo` se asigna con prioridad: nombre vacío → número vacío → número con
letras → número con caracteres especiales.

## Arquitectura

Clean Architecture (dependencias hacia adentro):

```
IRouteCommerce.Api            (Web API, controladores, JWT, Swagger, CORS)
   ├── IRouteCommerce.Application   (casos de uso, DTOs, interfaces/puertos)
   │      └── IRouteCommerce.Domain      (entidades)
   └── IRouteCommerce.Infrastructure  (EF Core, stored procedures, CSV, auth)
web/                          (Frontend Angular 18)
tests/IRouteCommerce.Tests    (pruebas unitarias xUnit)
```

- **Acceso a datos**: EF Core + **stored procedures** (`sp_create_commerce`,
  `sp_process_commerce`, `sp_get_quarantine`). La carga masiva usa **JSON +
  `OPENJSON`**; las cantidades se devuelven por **parámetros OUTPUT**.
- **Frontend**: Angular 18 standalone, carga diferida, `HttpClient` con
  **interceptor** que adjunta el JWT y **guard** que protege las rutas.

## Tecnologías

- .NET 10 (ASP.NET Core Web API), EF Core 10, CsvHelper, Swashbuckle.
- Angular 18.2.
- SQL Server.

## Requisitos

- .NET SDK 10.
- Node.js 20+ y npm (para el frontend).
- SQL Server (en este proyecto se usó la instancia `.\SQLEXPRESS`).

## Puesta en marcha

### 1. Base de datos

Crea las tablas y procedimientos (idempotente):

```powershell
sqlcmd -S ".\SQLEXPRESS" -E -i "scripts\init-db.sql"
```

Opcionalmente crea un usuario de aplicación dedicado:

```sql
CREATE LOGIN commerce_user WITH PASSWORD = '<TU_PASSWORD>',
    CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF;
```

> Nota: para conectarse desde clientes externos (HeidiSQL, etc.) la instancia
> debe tener **TCP/IP habilitado**.

### 2. Configuración de credenciales (sin secretos en el repo)

Por seguridad, **no hay credenciales en el repositorio**. La configuración de
desarrollo se toma de `src/IRouteCommerce.Api/appsettings.Development.json`,
que está en `.gitignore`. Para crearla:

```powershell
Copy-Item src/IRouteCommerce.Api/appsettings.Development.json.example `
          src/IRouteCommerce.Api/appsettings.Development.json
```

Luego edita ese archivo con tu cadena de conexión y una clave JWT propia:

```jsonc
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=IRouteCommerce;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=True;",
"Key": "UNA-CLAVE-SECRETA-LARGA-DE-MAS-DE-32-CARACTERES"
```

Como alternativa puedes usar **user-secrets** o variables de entorno (por
ejemplo `ConnectionStrings__DefaultConnection` y `Jwt__Key`).

Por defecto `appsettings.json` usa autenticación de Windows
(`Trusted_Connection=True`), lo que permite ejecutar la app sin contraseña si tu
instancia lo permite.

### 3. Backend

Ejecuta el API:

```powershell
dotnet run --project src/IRouteCommerce.Api
```

- API: `http://localhost:5062`
- Swagger UI: `http://localhost:5062/swagger`

Al iniciar se crea el usuario administrador de demostración
(`admin` / `Admin123*`) si no existe.

### 4. Frontend

```powershell
cd web
npm install
npm start
```

- App: `http://localhost:4200` (login de demostración: `admin` / `Admin123*`).

> El frontend consume el API en `http://localhost:5062`. Si cambias el puerto
> del API, actualiza `web/src/environments/environment*.ts`.

## Endpoints

| Método | Ruta                        | Descripción                                  | Auth |
|--------|-----------------------------|----------------------------------------------|------|
| POST   | `/api/auth/login`           | Login; devuelve el token JWT                 | No   |
| POST   | `/api/commerce/upload`      | Carga un CSV (`multipart/form-data`, campo `file`) | Sí |
| POST   | `/api/commerce/process`     | Procesa una fecha `{ "pcProcessdate": "yyyy-MM-dd" }` | Sí |
| GET    | `/api/commerce/quarantine`  | Lista los registros en cuarentena            | Sí   |

## Formato del CSV

Encabezado esperado:

```
pc_id,pc_nomcomred,pc_numdoc,pc_tipdoc,pc_direccion,pc_departamento,pc_provincia,pc_distrito,pc_telefono,pc_email,pc_processdate
```

Hay un archivo de ejemplo en `scripts/commerce_13102026.csv`.

## Pruebas

```powershell
dotnet test
```

Incluye pruebas de las validaciones de `CommerceService` (nombre/carga/proceso)
y del mapeo del CSV (`CsvService`).

### Prueba E2E de navegador (Playwright)

Ejecuta el flujo completo (login → carga → proceso → cuarentena) en un
navegador real. Levanta automáticamente el API y el frontend si no están activos.

```powershell
cd web
npm run e2e:install   # solo la primera vez (descarga Chromium)
npm run e2e
```

La prueba usa ids de comercio únicos por ejecución (basados en el timestamp) y
una fecha de proceso fija (`2030-06-15`) para no interferir con datos reales.
Al terminar, elimina de la base de datos los registros que creó, por lo que no
deja datos de prueba en el entorno. La limpieza obtiene la conexión desde
`appsettings` del API (o `appsettings.Development.json`) y usa `sqlcmd`.

## Estructura

```
scripts/init-db.sql             Esquema y stored procedures
src/IRouteCommerce.Domain       Entidades
src/IRouteCommerce.Application  Casos de uso, DTOs, interfaces
src/IRouteCommerce.Infrastructure  EF Core, repositorios, CSV, JWT
src/IRouteCommerce.Api          Controladores, middleware, configuración
tests/IRouteCommerce.Tests      Pruebas unitarias
web/                            Frontend Angular 18
```
