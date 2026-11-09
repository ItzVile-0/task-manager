-- Base de datos para el gestor de tareas

IF DB_ID('TaskManagerDb') IS NULL
    CREATE DATABASE TaskManagerDb;
GO

USE TaskManagerDb;
GO

IF OBJECT_ID('dbo.Tasks', 'U') IS NOT NULL DROP TABLE dbo.Tasks;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users (
    Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Name      NVARCHAR(100) NOT NULL,
    Email     NVARCHAR(150) NOT NULL,
    CreatedAt DATETIME2(0)  NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Name  CHECK (LEN(Name) > 0),
    CONSTRAINT CK_Users_Email CHECK (Email LIKE '%_@_%._%')
);
GO

CREATE TABLE dbo.Tasks (
    Id             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tasks PRIMARY KEY,
    Title          NVARCHAR(200)  NOT NULL,
    Description    NVARCHAR(1000) NULL,
    Status         NVARCHAR(20)   NOT NULL CONSTRAINT DF_Tasks_Status DEFAULT 'Pending',
    UserId         INT            NOT NULL,
    AdditionalInfo NVARCHAR(MAX)  NULL,
    CreatedAt      DATETIME2(0)   NOT NULL CONSTRAINT DF_Tasks_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt      DATETIME2(0)   NULL,
    -- la prioridad sale del json, la dejo como columna calculada para poder indexarla
    Priority AS CAST(JSON_VALUE(AdditionalInfo, '$.priority') AS NVARCHAR(10)),
    CONSTRAINT FK_Tasks_Users          FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
    CONSTRAINT CK_Tasks_Title          CHECK (LEN(Title) > 0),
    CONSTRAINT CK_Tasks_Status         CHECK (Status IN ('Pending', 'InProgress', 'Done')),
    CONSTRAINT CK_Tasks_AdditionalInfo CHECK (AdditionalInfo IS NULL OR ISJSON(AdditionalInfo) = 1)
);
GO

-- usuario + estado + fecha, que es la consulta principal (tambien cubre la FK)
CREATE INDEX IX_Tasks_UserId_Status_CreatedAt ON dbo.Tasks (UserId, Status, CreatedAt DESC);
CREATE INDEX IX_Tasks_Status_CreatedAt ON dbo.Tasks (Status, CreatedAt DESC);
CREATE INDEX IX_Tasks_Priority ON dbo.Tasks (Priority);
GO

-- datos de prueba
INSERT INTO dbo.Users (Name, Email) VALUES
    (N'Ana Gómez',    N'ana.gomez@empresa.com'),
    (N'Carlos Ruiz',  N'carlos.ruiz@empresa.com'),
    (N'Laura Torres', N'laura.torres@empresa.com');

INSERT INTO dbo.Tasks (Title, Description, Status, UserId, AdditionalInfo, CreatedAt) VALUES
    (N'Configurar servidor de pruebas', N'Preparar ambiente QA', 'Pending', 1,
     N'{"priority":"High","estimatedEndDate":"2026-11-20","tags":["infra","urgente"]}', DATEADD(DAY, -5, SYSUTCDATETIME())),
    (N'Actualizar manual de usuario', NULL, 'InProgress', 1,
     N'{"priority":"Low","estimatedEndDate":"2026-12-01","tags":["documentacion"]}', DATEADD(DAY, -4, SYSUTCDATETIME())),
    (N'Revisar backups semanales', N'Validar restauración', 'Done', 1,
     N'{"priority":"Medium","tags":["infra","seguridad"]}', DATEADD(DAY, -3, SYSUTCDATETIME())),
    (N'Diseñar reporte de ventas', NULL, 'Pending', 2,
     N'{"priority":"Medium","estimatedEndDate":"2026-11-25","tags":["reportes"]}', DATEADD(DAY, -2, SYSUTCDATETIME())),
    (N'Corregir error en login', N'Usuarios no pueden ingresar', 'InProgress', 2,
     N'{"priority":"High","tags":["bug","urgente"]}', DATEADD(DAY, -1, SYSUTCDATETIME())),
    (N'Inventario de equipos', NULL, 'Pending', 3, NULL, SYSUTCDATETIME());
GO


-- ===== Consultas =====

-- tareas de un usuario, filtro opcional por estado, mas recientes primero
DECLARE @UserId INT = 1;
DECLARE @Status NVARCHAR(20) = NULL; -- probar con 'Pending'

SELECT t.Id, t.Title, t.Status, t.CreatedAt, u.Name AS UserName
FROM dbo.Tasks t
INNER JOIN dbo.Users u ON u.Id = t.UserId
WHERE t.UserId = @UserId
  AND (@Status IS NULL OR t.Status = @Status)
ORDER BY t.CreatedAt DESC;
GO

-- ISJSON: el check no deja guardar json mal formado
BEGIN TRY
    INSERT INTO dbo.Tasks (Title, UserId, AdditionalInfo)
    VALUES (N'Json malo', 1, N'{prioridad: alta');
END TRY
BEGIN CATCH
    SELECT ERROR_MESSAGE() AS ErrorEsperado;
END CATCH;

SELECT Id, Title, ISJSON(AdditionalInfo) AS JsonValido FROM dbo.Tasks;
GO

-- JSON_VALUE: leer campos del json y filtrar por prioridad
SELECT Id, Title,
       JSON_VALUE(AdditionalInfo, '$.priority') AS Priority,
       JSON_VALUE(AdditionalInfo, '$.estimatedEndDate') AS EstimatedEndDate
FROM dbo.Tasks
WHERE JSON_VALUE(AdditionalInfo, '$.priority') = 'High';

-- lo mismo pero con la columna calculada (asi lo hace la api)
SELECT Id, Title, Priority FROM dbo.Tasks WHERE Priority = 'High';
GO

-- JSON_QUERY: para arreglos u objetos. JSON_VALUE aqui devuelve NULL
SELECT Id, Title,
       JSON_QUERY(AdditionalInfo, '$.tags') AS Tags,
       JSON_VALUE(AdditionalInfo, '$.tags') AS TagsConJsonValue
FROM dbo.Tasks
WHERE AdditionalInfo IS NOT NULL;
GO

-- OPENJSON: tareas que tienen una etiqueta
DECLARE @Tag NVARCHAR(50) = N'urgente';

SELECT t.Id, t.Title, t.Status
FROM dbo.Tasks t
WHERE EXISTS (SELECT 1 FROM OPENJSON(t.AdditionalInfo, '$.tags') WHERE value = @Tag);

-- cuantas tareas hay por etiqueta
SELECT tag.value AS Tag, COUNT(*) AS Total
FROM dbo.Tasks t
CROSS APPLY OPENJSON(t.AdditionalInfo, '$.tags') tag
GROUP BY tag.value
ORDER BY Total DESC;
GO

-- JSON_MODIFY (opcional). Va con rollback para no dañar los datos de prueba
BEGIN TRAN;

UPDATE dbo.Tasks
SET AdditionalInfo = JSON_MODIFY(AdditionalInfo, '$.priority', 'Low'),
    UpdatedAt = SYSUTCDATETIME()
WHERE Id = 1;

UPDATE dbo.Tasks
SET AdditionalInfo = JSON_MODIFY(AdditionalInfo, 'append $.tags', 'revisado')
WHERE Id = 1;

SELECT Id, AdditionalInfo, Priority FROM dbo.Tasks WHERE Id = 1;

ROLLBACK;
GO
