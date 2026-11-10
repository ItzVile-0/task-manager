# Gestor de tareas

Sistema interno para registrar tareas, asignarlas a los colaboradores y hacer seguimiento a su estado. Desarrollado como prueba técnica Fullstack Semisenior.

Tecnologías:

- Backend: .NET 10 Web API con EF Core 10
- Frontend: Angular 22 con Angular Material
- Base de datos: SQL Server (lo probé con SQL Server 2025 Express)
- Pruebas: xUnit en el backend y Vitest en el frontend

## Estructura

```
task-manager/
├── database/script.sql          base de datos, tablas, índices, datos de prueba y consultas JSON
├── backend/
│   ├── TaskManager.Api/         la API
│   └── TaskManager.Api.Tests/   pruebas del servicio de tareas
└── frontend/                    aplicación Angular
```

La API está organizada en Controllers, Services, Repositories, DTOs, Entities, Data (el DbContext) y Exceptions. En el frontend, `core` tiene los modelos, los servicios HTTP y el interceptor de errores, y `features` las pantallas de tareas y usuarios.

## Requisitos

- .NET SDK 10
- Node.js 22 o superior y Angular CLI 22 (`npm install -g @angular/cli@22`)
- SQL Server y SSMS (o cualquier cliente para ejecutar el script)

## Cómo ejecutarlo

### 1. Base de datos

Abrir `database/script.sql` en SSMS y ejecutarlo completo. Crea la base `TaskManagerDb`, las tablas `Users` y `Tasks`, carga algunos datos de prueba y al final ejecuta las consultas de ejemplo con las funciones JSON.

Ojo: el script borra y vuelve a crear las tablas. Sirve también para volver a los datos iniciales.

### 2. API

La cadena de conexión está en `backend/TaskManager.Api/appsettings.json`:

```
Server=localhost,1433;Database=TaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True
```

Usa autenticación de Windows y se conecta por TCP al puerto 1433. En SQL Server Express el protocolo TCP/IP viene deshabilitado, así que hay que activarlo en SQL Server Configuration Manager (Protocolos de SQLEXPRESS, TCP/IP, Habilitar; en IPAll poner 1433 como Puerto TCP) y reiniciar el servicio. Otra opción es dejar el servidor como `localhost\SQLEXPRESS` y tener iniciado el servicio SQL Server Browser.

Si se usa un usuario de SQL Server en vez de autenticación de Windows, se cambia `Trusted_Connection=True` por `User Id=...;Password=...`.

```
cd backend/TaskManager.Api
dotnet run
```

La API queda en `http://localhost:5080`. En `TaskManager.Api.http` están las peticiones que usé para probarla, tanto los casos que funcionan como los que deben dar error. Se pueden ejecutar con la extensión REST Client de VS Code, o desde Visual Studio o Rider.

### 3. Frontend

```
cd frontend
npm install
ng serve
```

Se abre en `http://localhost:4200`. Necesita la API corriendo; la dirección de la API está en `src/app/core/api.ts`.

### 4. Pruebas

```
cd backend
dotnet test
```

```
cd frontend
ng test --watch=false
```

## Endpoints

| Método | Ruta | Qué hace | Respuestas |
|---|---|---|---|
| GET | `/api/users` | Lista los usuarios | 200 |
| POST | `/api/users` | Crea un usuario | 201, 400, 409 si el correo ya existe |
| GET | `/api/tasks?userId=&status=&priority=` | Lista las tareas, los filtros son opcionales | 200, 400 |
| GET | `/api/tasks/{id}` | Trae una tarea | 200, 404 |
| POST | `/api/tasks` | Crea una tarea, siempre empieza en Pending | 201, 400, 422 si el usuario no existe |
| PUT | `/api/tasks/{id}/status` | Cambia el estado | 200, 400, 404, 422 si va de Pending a Done |

Las tareas salen ordenadas de la más reciente a la más antigua. Los errores se devuelven en formato ProblemDetails: los de validación vienen en `errors` y los de reglas de negocio en `detail`.

Ejemplo para crear una tarea:

```json
{
  "title": "Revisar licencias de Office",
  "description": "Validar cuantas licencias quedan libres",
  "userId": 3,
  "additionalInfo": {
    "priority": "Medium",
    "estimatedEndDate": "2026-11-30",
    "tags": ["licencias", "office"]
  }
}
```

## Reglas de negocio

- El título es obligatorio.
- Toda tarea debe tener un usuario asignado, y ese usuario tiene que existir.
- Una tarea no puede pasar directamente de Pending a Done. Los demás cambios sí se permiten, por ejemplo de Done a Pending para reabrirla, porque la prueba solo restringe ese caso.
- No puede haber dos usuarios con el mismo correo.

Estas reglas se validan en la API: el título y el usuario obligatorio en los DTOs, y el resto en `TaskService` y `UserService`. En el frontend la opción Done aparece deshabilitada para las tareas pendientes, pero eso es solo para ayudar al usuario; la validación que cuenta es la del backend.

## JSON en SQL Server

La columna `Tasks.AdditionalInfo` es `NVARCHAR(MAX)` y guarda la información adicional de la tarea: prioridad, fecha estimada y etiquetas.

- `ISJSON`: está en una restricción `CHECK`, así la base rechaza cualquier JSON mal formado.
- `JSON_VALUE`: se usa en la columna calculada `Priority`, que tiene un índice. Es la que usa la API para filtrar por prioridad.
- `JSON_QUERY`: en el script, para sacar el arreglo de etiquetas. Con `JSON_VALUE` daría NULL porque no es un valor simple.
- `OPENJSON`: en el script, para filtrar tareas por etiqueta y contar cuántas tareas tiene cada etiqueta.
- `JSON_MODIFY` (opcional): en el script, cambia la prioridad y agrega una etiqueta dentro de una transacción con rollback para no dañar los datos de prueba.

Por ejemplo, para traer las tareas que tienen la etiqueta "urgente":

```sql
SELECT t.Id, t.Title, t.Status
FROM dbo.Tasks t
WHERE EXISTS (SELECT 1 FROM OPENJSON(t.AdditionalInfo, '$.tags') WHERE value = N'urgente');
```

Las demás consultas de ejemplo están al final de `database/script.sql`.

## Decisiones técnicas

### Estructura del backend

Hice un solo proyecto de API con carpetas por capa en lugar de varios proyectos tipo Clean Architecture. Para dos entidades me pareció suficiente: el controller recibe la petición y responde, el servicio aplica las reglas y el repositorio consulta la base. Las capas se conectan por interfaces registradas en `Program.cs`, lo que además permitió probar el servicio sin base de datos.

### EF Core sin migraciones

Como la prueba pedía un script SQL, dejé el esquema definido ahí y EF Core solo lo mapea. Si además usaba migraciones, iba a tener dos definiciones del mismo esquema que podían terminar desincronizadas.

### Filtro por un valor del JSON

No se puede crear un índice directamente sobre un valor dentro de un JSON, pero sí sobre una columna calculada con `JSON_VALUE`. Por eso agregué la columna `Priority` con su índice; en el código C# el filtro queda como un `Where` normal.

### Manejo de errores

Los servicios lanzan excepciones propias (`NotFoundException`, `ConflictException`, `BusinessRuleException`) y un `GlobalExceptionHandler` las convierte en 404, 409 o 422. Así los controllers no tienen try/catch. Si ocurre un error inesperado se registra en el log y se responde 500 sin mostrar el detalle interno.

Para las reglas de negocio usé 422 y no 400: la petición está bien formada, pero no se puede cumplir. El 400 lo dejé para datos inválidos, como un título vacío.

### Otros detalles

- El estado se guarda como texto (`NVARCHAR(20)` con un `CHECK`), porque es más fácil de leer en las consultas y para tres valores fijos no justificaba una tabla aparte.
- Las fechas se guardan en UTC con `SYSUTCDATETIME()` y el frontend las muestra en la hora local.
- En el frontend usé Angular Material para la tabla, los formularios y las notificaciones, y signals para el estado de cada pantalla.
- Todas las respuestas con error pasan por un interceptor que muestra el mensaje que manda la API, así no se repite ese código en cada pantalla.
- Agregué una referencia directa a `System.Security.Cryptography.Pkcs` porque EF Core trae una versión anterior con vulnerabilidades reportadas por NuGet.

## Pendiente

Están todos los requisitos de la prueba. Esto es lo que dejé por fuera por tiempo:

- Paginación en el listado de tareas.
- Autenticación y permisos.
- Pruebas de integración automatizadas para la API. Por ahora esos casos los probé con el archivo `.http`.
- Editar y eliminar tareas o usuarios, que no estaban en el alcance.
- Filtrar por etiqueta desde la API. La consulta con `OPENJSON` está en el script, pero no la expuse como endpoint.
- Si dos personas crean un usuario con el mismo correo exactamente al mismo tiempo, la segunda recibe un 500 en vez de un 409 (la base no deja duplicarlo). Se corrige capturando el error 2627 de SQL Server.
- La dirección de la API está fija en el frontend; para publicarlo habría que moverla a la configuración de entornos.
- Un docker compose para levantar todo con un solo comando.
