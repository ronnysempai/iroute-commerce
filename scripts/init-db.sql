/* ============================================================================
   i-Route - Reto individual
   Base de datos: IRouteCommerce (SQL Server)
   Autor: Ronny
   Descripcion:
       Crea las tablas commerce y commerce_quarantine, la tabla de usuarios,
       y los stored procedures:
         - sp_create_commerce   : carga masiva del CSV (JSON -> OPENJSON)
         - sp_process_commerce  : valida y mueve registros invalidos a cuarentena
         - sp_get_quarantine    : lista los registros en cuarentena
       El script es idempotente (se puede ejecutar varias veces).
   ============================================================================ */

IF DB_ID('IRouteCommerce') IS NULL
    CREATE DATABASE IRouteCommerce;
GO

USE IRouteCommerce;
GO

/* ----------------------------------------------------------------------------
   Limpieza (orden inverso por dependencias)
   ---------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.sp_get_quarantine', 'P')   IS NOT NULL DROP PROCEDURE dbo.sp_get_quarantine;
IF OBJECT_ID('dbo.sp_process_commerce', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_process_commerce;
IF OBJECT_ID('dbo.sp_create_commerce', 'P')  IS NOT NULL DROP PROCEDURE dbo.sp_create_commerce;
IF OBJECT_ID('dbo.commerce_quarantine', 'U') IS NOT NULL DROP TABLE dbo.commerce_quarantine;
IF OBJECT_ID('dbo.commerce', 'U')            IS NOT NULL DROP TABLE dbo.commerce;
IF OBJECT_ID('dbo.app_users', 'U')           IS NOT NULL DROP TABLE dbo.app_users;
GO

/* ----------------------------------------------------------------------------
   Tabla commerce: misma estructura que commerce_DDMMYYYY.csv
   pc_id es la llave primaria (identificador del comercio en el archivo).
   Las columnas validables se dejan NULLables para poder almacenar primero
   los datos crudos y luego mover los invalidos a cuarentena.
   ---------------------------------------------------------------------------- */
CREATE TABLE dbo.commerce (
    pc_id            INT             NOT NULL,
    pc_nomcomred     NVARCHAR(200)   NULL,
    pc_numdoc        NVARCHAR(20)    NULL,
    pc_tipdoc        NVARCHAR(10)    NULL,
    pc_direccion     NVARCHAR(250)   NULL,
    pc_departamento  NVARCHAR(100)   NULL,
    pc_provincia     NVARCHAR(100)   NULL,
    pc_distrito      NVARCHAR(100)   NULL,
    pc_telefono      NVARCHAR(30)    NULL,
    pc_email         NVARCHAR(150)   NULL,
    pc_processdate   DATE            NOT NULL,
    CONSTRAINT PK_commerce PRIMARY KEY CLUSTERED (pc_id)
);
GO

CREATE NONCLUSTERED INDEX IX_commerce_pc_processdate
    ON dbo.commerce (pc_processdate);
GO

/* ----------------------------------------------------------------------------
   Tabla commerce_quarantine: estructura de commerce + columna "motivo".
   Se usa id IDENTITY como llave para conservar historial de cuarentenas.
   ---------------------------------------------------------------------------- */
CREATE TABLE dbo.commerce_quarantine (
    id               INT             IDENTITY(1,1) NOT NULL,
    pc_id            INT             NOT NULL,
    pc_nomcomred     NVARCHAR(200)   NULL,
    pc_numdoc        NVARCHAR(20)    NULL,
    pc_tipdoc        NVARCHAR(10)    NULL,
    pc_direccion     NVARCHAR(250)   NULL,
    pc_departamento  NVARCHAR(100)   NULL,
    pc_provincia     NVARCHAR(100)   NULL,
    pc_distrito      NVARCHAR(100)   NULL,
    pc_telefono      NVARCHAR(30)    NULL,
    pc_email         NVARCHAR(150)   NULL,
    pc_processdate   DATE            NOT NULL,
    motivo           NVARCHAR(300)   NOT NULL,
    fecha_cuarentena DATETIME2       NOT NULL CONSTRAINT DF_quarantine_fecha DEFAULT SYSDATETIME(),
    CONSTRAINT PK_commerce_quarantine PRIMARY KEY CLUSTERED (id)
);
GO

/* ----------------------------------------------------------------------------
   Tabla de usuarios para el login (JWT)
   ---------------------------------------------------------------------------- */
CREATE TABLE dbo.app_users (
    id            INT             IDENTITY(1,1) NOT NULL,
    username      NVARCHAR(50)    NOT NULL,
    password_hash NVARCHAR(500)   NOT NULL,
    role          NVARCHAR(30)    NOT NULL CONSTRAINT DF_app_users_role DEFAULT 'User',
    created_at    DATETIME2       NOT NULL CONSTRAINT DF_app_users_created DEFAULT SYSDATETIME(),
    CONSTRAINT PK_app_users PRIMARY KEY CLUSTERED (id),
    CONSTRAINT UQ_app_users_username UNIQUE (username)
);
GO

/* ============================================================================
   sp_create_commerce
   Recibe las filas del CSV serializadas en JSON y las inserta en commerce.
   Se usa JSON + OPENJSON para permitir carga masiva en una sola llamada.
   ============================================================================ */
CREATE OR ALTER PROCEDURE dbo.sp_create_commerce
    @Json NVARCHAR(MAX),
    @RegistrosCargados INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.commerce
        (pc_id, pc_nomcomred, pc_numdoc, pc_tipdoc, pc_direccion,
         pc_departamento, pc_provincia, pc_distrito, pc_telefono, pc_email, pc_processdate)
    SELECT
        TRY_CONVERT(INT, j.pc_id),
        NULLIF(LTRIM(RTRIM(j.pc_nomcomred)), ''),
        NULLIF(LTRIM(RTRIM(j.pc_numdoc)), ''),
        NULLIF(LTRIM(RTRIM(j.pc_tipdoc)), ''),
        NULLIF(LTRIM(RTRIM(j.pc_direccion)), ''),
        NULLIF(LTRIM(RTRIM(j.pc_departamento)), ''),
        NULLIF(LTRIM(RTRIM(j.pc_provincia)), ''),
        NULLIF(LTRIM(RTRIM(j.pc_distrito)), ''),
        NULLIF(LTRIM(RTRIM(j.pc_telefono)), ''),
        NULLIF(LTRIM(RTRIM(j.pc_email)), ''),
        TRY_CONVERT(DATE, j.pc_processdate)
    FROM OPENJSON(@Json)
    WITH (
        pc_id           NVARCHAR(50)  '$.pc_id',
        pc_nomcomred    NVARCHAR(200) '$.pc_nomcomred',
        pc_numdoc       NVARCHAR(20)  '$.pc_numdoc',
        pc_tipdoc       NVARCHAR(10)  '$.pc_tipdoc',
        pc_direccion    NVARCHAR(250) '$.pc_direccion',
        pc_departamento NVARCHAR(100) '$.pc_departamento',
        pc_provincia    NVARCHAR(100) '$.pc_provincia',
        pc_distrito     NVARCHAR(100) '$.pc_distrito',
        pc_telefono     NVARCHAR(30)  '$.pc_telefono',
        pc_email        NVARCHAR(150) '$.pc_email',
        pc_processdate  NVARCHAR(30)  '$.pc_processdate'
    ) AS j;

    SET @RegistrosCargados = @@ROWCOUNT;
END;
GO

/* ============================================================================
   sp_process_commerce
   Valida los registros de una fecha de proceso (pc_processdate):
     1. pc_nomcomred no debe estar vacio.
     2. pc_numdoc no debe estar vacio, ni contener letras ni caracteres
        especiales (solo digitos).
   Los registros que incumplen se mueven (INSERT + DELETE) a commerce_quarantine
   guardando el motivo. Retorna la cantidad de registros movidos.
   ============================================================================ */
CREATE OR ALTER PROCEDURE dbo.sp_process_commerce
    @pc_processdate DATE,
    @RegistrosEnCuarentena INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @cantidad INT = 0;
    SET @RegistrosEnCuarentena = 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        CREATE TABLE #Invalidos (
            pc_id           INT,
            pc_nomcomred    NVARCHAR(200),
            pc_numdoc       NVARCHAR(20),
            pc_tipdoc       NVARCHAR(10),
            pc_direccion    NVARCHAR(250),
            pc_departamento NVARCHAR(100),
            pc_provincia    NVARCHAR(100),
            pc_distrito     NVARCHAR(100),
            pc_telefono     NVARCHAR(30),
            pc_email        NVARCHAR(150),
            pc_processdate  DATE,
            motivo          NVARCHAR(300)
        );

        INSERT INTO #Invalidos
            (pc_id, pc_nomcomred, pc_numdoc, pc_tipdoc, pc_direccion,
             pc_departamento, pc_provincia, pc_distrito, pc_telefono, pc_email,
             pc_processdate, motivo)
        SELECT
            c.pc_id, c.pc_nomcomred, c.pc_numdoc, c.pc_tipdoc, c.pc_direccion,
            c.pc_departamento, c.pc_provincia, c.pc_distrito, c.pc_telefono, c.pc_email,
            c.pc_processdate,
            CASE
                WHEN c.pc_nomcomred IS NULL OR LTRIM(RTRIM(c.pc_nomcomred)) = ''
                    THEN N'El nombre del comercio (nomcomred) se encuentra vacio'
                WHEN c.pc_numdoc IS NULL OR LTRIM(RTRIM(c.pc_numdoc)) = ''
                    THEN N'El número (numdoc) se encuentra vacio'
                WHEN c.pc_numdoc LIKE '%[A-Za-z]%'
                    THEN N'El número (numdoc) contiene letras'
                ELSE N'El número (numdoc) contiene caracteres especiales'
            END
        FROM dbo.commerce c
        WHERE c.pc_processdate = @pc_processdate
          AND (
                c.pc_nomcomred IS NULL OR LTRIM(RTRIM(c.pc_nomcomred)) = ''
             OR c.pc_numdoc IS NULL OR LTRIM(RTRIM(c.pc_numdoc)) = ''
             OR c.pc_numdoc LIKE '%[^0-9]%'
          );

        INSERT INTO dbo.commerce_quarantine
            (pc_id, pc_nomcomred, pc_numdoc, pc_tipdoc, pc_direccion,
             pc_departamento, pc_provincia, pc_distrito, pc_telefono, pc_email,
             pc_processdate, motivo)
        SELECT
            pc_id, pc_nomcomred, pc_numdoc, pc_tipdoc, pc_direccion,
            pc_departamento, pc_provincia, pc_distrito, pc_telefono, pc_email,
            pc_processdate, motivo
        FROM #Invalidos;

        SET @cantidad = @@ROWCOUNT;

        DELETE c
        FROM dbo.commerce c
        INNER JOIN #Invalidos i ON i.pc_id = c.pc_id;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SET @RegistrosEnCuarentena = @cantidad;
END;
GO

/* ============================================================================
   sp_get_quarantine
   Lista los registros que quedaron en cuarentena con su motivo.
   ============================================================================ */
CREATE OR ALTER PROCEDURE dbo.sp_get_quarantine
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        q.id,
        q.pc_id,
        q.pc_nomcomred,
        q.pc_numdoc,
        q.pc_tipdoc,
        q.pc_direccion,
        q.pc_departamento,
        q.pc_provincia,
        q.pc_distrito,
        q.pc_telefono,
        q.pc_email,
        q.pc_processdate,
        q.motivo,
        q.fecha_cuarentena
    FROM dbo.commerce_quarantine q
    ORDER BY q.id DESC;
END;
GO

PRINT 'IRouteCommerce inicializado correctamente.';
GO
