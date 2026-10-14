# TAREA-003 — Arquitectura: CRUD de Product

- ID: TAREA-003 (alias Producto-003) · Rol: Arquitecto · Carpeta: `docs/informes/TAREA-003/`
- Objetivo: CRUD de `Product` con `Id` (Guid), `Price`, `Description`, `AlternateDescription`.
- Issue: `FTEMORES/CursoNetIA` #2 — https://github.com/FTEMORES/CursoNETIA/issues/2

## Archivos consultados
`.github/copilot-instructions.md`, `docs/informes/README.md`, `indice.md`, `docs/github-pr.md`, `docs/multiagente-copilot.md`, `docs/expedientes/Producto-003.md` (antecedente), `src/AulaPedidos.Api/Program.cs`, `Application/Abstractions/Persistence/IRepository.cs` e `IUnitOfWork.cs`, `Application/DependencyInjection.cs`, `Infrastructure/DependencyInjection.cs`, `Infrastructure/Persistence/{AulaPedidosDbContext,EfRepository,EfUnitOfWork}.cs`.
No leídos: `.csproj`, tests, `docs/paquetes/BASE-001-paquete-comun.md`. No se ejecutó nada.

## Estructura actual (verificada)
- Domain: sin entidades en lo consultado (no se leyó su contenido).
- Application: `IRepository<T>` (AddAsync, RemoveAsync, GetAsync con predicado/orden/includes, GetPagedAsync), `IUnitOfWork.SaveChangesAsync`. `AddApplication` registra `ISender`/`IPublisher` sobre `IMediator`.
- Infrastructure: `AulaPedidosDbContext` vacío (sin `DbSet` ni mapeos), `EfRepository<T>` (consultas con tracking, **sin AsNoTracking**; contradice el expediente histórico), `EfUnitOfWork`; registro abierto de `IRepository<>` en `AddInfrastructure` (SQLite, cadena `DefaultConnection`).
- Api: `Program.cs` registra Mediator (scoped), OpenAPI y `/health`; no hay endpoints de producto.
- Sin repositorio con `GetById` ni `Update`: no hace falta ampliar el contrato (ver propuesta).

## Propuesta
**Domain** (`AulaPedidos.Domain`)
- `Product`: `Id` (Guid, asignado en el servidor), `Price` (decimal), `Description` (string), `AlternateDescription` (string?).
- Constructor/fábrica y método `Update(...)` que validan: `Description` no vacía, `Price` ≥ 0. Setters privados. Sin referencias a EF/HTTP.
- Excepción de dominio simple solo si no existe una; no introducir `Result`.

**Application** (`Products/`)
- Comandos: `CreateProductCommand`, `UpdateProductCommand`, `DeleteProductCommand`; consultas: `GetProductByIdQuery`, `GetProductsQuery`; `ProductDto`.
- Handlers `IRequestHandler` de martinothamar/Mediator; usan `IRepository<Product>` e `IUnitOfWork`.
- Obtener por Id: `GetAsync(p => p.Id == id)`. Actualizar: entidad rastreada → `Update(...)` → `SaveChangesAsync`. Eliminar: `RemoveAsync` + `SaveChangesAsync`. Listado: `orderBy` por `Description` y luego `Id` (orden estable).
- Inexistente: devolver `null`/`ProductDto?` y que Api traduzca a 404 (evita crear excepciones o Result sin necesidad).

**Infrastructure**
- `DbSet<Product> Products` y `ProductConfiguration : IEntityTypeConfiguration<Product>` (clave `Id`, `Price` con conversión compatible con SQLite). Aplicar con `ApplyConfigurationsFromAssembly`.
- Esquema: añadir migración o `EnsureCreated` según el mecanismo existente (**no determinado**: no se leyó). No borrar ni recrear bases compartidas.
- No añadir repositorio específico.

**Api**
- `ProductEndpoints` con `MapGroup("/products")`: POST (201 + Location), GET, GET `{id:guid}`, PUT `{id:guid}` (200/404), DELETE (204/404). Delegan en `ISender` y propagan `CancellationToken`. Request DTO propio, 400 por validación con ProblemDetails. Registrar en `Program.cs`.

## Dependencias propuestas
Api → Application, Infrastructure. Infrastructure → Application, Domain. Application → Domain. Domain → nada. Ningún paquete nuevo (Mediator y EF Core SQLite ya presentes; FluentValidation no necesario).

## Archivos por cambiar o crear (referencia)
Domain: `Products/Product.cs`. Application: `Products/*` (5 casos de uso + handlers + `ProductDto`). Infrastructure: `AulaPedidosDbContext.cs`, `Persistence/Configurations/ProductConfiguration.cs`, migración si aplica. Api: `Endpoints/ProductEndpoints.cs`, `Program.cs`. Tests: dominio, handlers, repositorio SQLite aislado, integración HTTP.

## Justificación
Reutiliza el contrato y la implementación genérica existentes; una entidad con comportamiento protege las reglas en Domain; `GetAsync` + tracking cubren GetById/Update sin ampliar la interfaz; 404 con `null` es lo mínimo.

## Riesgos
- Descripciones y precio: reglas sin decidir (ver supuestos).
- Decimal en SQLite: se almacena como TEXT; ordenar/comparar por precio en SQL puede ser incorrecto. No se propone filtrar por precio.
- Cambio del `DbContext` sin saber el mecanismo de esquema: riesgo de romper bases existentes.
- Divergencia con el expediente (Sku/Name; AsNoTracking): se declara; prevalece la petición actual y el código.

## Supuestos / decisiones humanas pendientes
`Description` obligatoria; `AlternateDescription` opcional; `Price` ≥ 0 (0 permitido); sin longitudes máximas; PUT completo; sin paginación. Son propuestas, no decisiones.

## Aceptación sugerida
- POST devuelve 201, Guid no vacío y Location; GET posterior devuelve los tres campos.
- GET lista vacía → 200 `[]`; con datos, orden estable.
- PUT conserva Id y persiste cambios; Id inexistente → 404.
- DELETE → 204 y GET posterior 404; inexistente → 404.
- Description vacía/espacios o precio negativo → 400 sin persistir.
- Pruebas de arquitectura: Domain sin EF/HTTP; Application sin Infrastructure.
- `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx` pasan (pendiente de ejecución).

## Nota de proceso
En un turno anterior, este rol creó el issue #2 y escribió `01-coordinacion.md`, fuera de su permiso (solo informe propio y comentario de su fase). Ambos se conservan; el Coordinador debe validarlos o reemplazarlos. `01-coordinacion.md` contiene una propuesta de campos y reglas que no es decisión humana.

## Publicación en GitHub
- Repositorio/issue: `FTEMORES/CursoNetIA` #2
- Marcador: `<!-- aulapedidos:TAREA-003:arquitectura -->`
- Comentario: id 6157652595 — https://github.com/FTEMORES/CursoNETIA/issues/2#issuecomment-6157652595
- Estado: publicado (la respuesta de la API confirmó id y URL; no se releyó el comentario). Se consultaron los comentarios antes de publicar y no había ninguno.
