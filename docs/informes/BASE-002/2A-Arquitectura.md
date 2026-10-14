# BASE-002 — Análisis 2A de arquitectura: Repository y Unit of Work

## Objetivo y rol
- **Rol:** Arquitecto (paso 2A). No se implementa código.
- **Objetivo:** diseñar Repository + Unit of Work sobre BASE-001: contratos en Application, implementación EF Core + SQLite en Infrastructure, composición en Api.

## Archivos realmente consultados
- `docs/expedientes/BASE-002.md`
- `CursoNETIA.slnx`
- `src/AulaPedidos.Api/Program.cs`, `AulaPedidos.Api.csproj`, `appsettings.json`
- `src/AulaPedidos.Application/DependencyInjection.cs`, `AulaPedidos.Application.csproj`
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`, `AulaPedidos.Infrastructure.csproj`
- `tests/AulaPedidos.ArchitectureTests/AulaPedidos.ArchitectureTests.csproj`

No localizados: `docs/paquetes/BASE-001-paquete-comun.md`. No se revisaron el proyecto Domain ni `tests/AulaPedidos.Api.IntegrationTests`. Se asume que Domain está vacío (sin entidades), como indica el alcance de BASE-001.

## Estructura actual
- Solución `CursoNETIA.slnx`: Api, Application, Domain, Infrastructure (src) y ArchitectureTests e IntegrationTests (tests).
- Referencias: Application → Domain; Infrastructure → Application; Api → Application + Infrastructure.
- Application: `AddApplication()` registra `ISender`/`IPublisher` sobre `IMediator`. Paquetes: `Mediator.Abstractions` 3.0.2, `Microsoft.Extensions.DependencyInjection.Abstractions`.
- Infrastructure: `AddInfrastructure(IServiceCollection, IConfiguration)` ya existe y está vacío. Paquetes: Configuration.Abstractions y DI.Abstractions 10.0.12. No hay EF Core.
- Api: `Program.cs` llama a `AddApplication`, `AddInfrastructure(builder.Configuration)`, `AddMediator` (Mediator.SourceGenerator) y health checks. `appsettings.json` no tiene `ConnectionStrings`.
- No existen repositorios, `DbContext`, `IUnitOfWork` ni entidades.

## Diferencia con el expediente
El expediente propone `AddInfrastructure(connectionString)`, pero la firma actual es `AddInfrastructure(IConfiguration)` y `Program.cs` ya la usa. **Propuesta:** conservar la firma actual y leer dentro `configuration.GetConnectionString("Default")`. Así no cambia `Program.cs` y se cierra el pendiente de ubicación de la cadena.

## Diseño propuesto

### Application (sin EF Core)
Carpeta `Abstractions/Persistence/`:
- `IRepository<TEntity> where TEntity : class` con:
  - `Task<List<TEntity>> GetAsync(Expression<Func<TEntity,bool>>? predicate = null, CancellationToken cancellationToken = default)`
  - `Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity,bool>>? predicate = null, CancellationToken cancellationToken = default)`
  - `Task<bool> AnyAsync(...)` y `Task<int> CountAsync(...)` con predicado opcional.
  - `IQueryable<TEntity> Query(Expression<Func<TEntity,bool>>? predicate = null)`
  - `Task<List<TEntity>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity,bool>>? predicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy, CancellationToken cancellationToken, params string[] includeStrings)`
  - `Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)`, `void Update(TEntity entity)`, `void Remove(TEntity entity)`
- `IUnitOfWork` con `Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)`.
- Nota: `params` obliga a que `cancellationToken` no sea opcional en `GetPagedAsync`, tal como está en el expediente.
- `IQueryable` y `Expression` pertenecen a `System.Linq`, no requieren paquetes. Los métodos async de EF (`ToListAsync`) solo se usan en Infrastructure.

### Infrastructure
- Paquetes (solo aquí): `Microsoft.EntityFrameworkCore.Sqlite` 10.x. `Microsoft.EntityFrameworkCore.Relational` llega de forma transitiva.
- `Persistence/AppDbContext : DbContext` (constructor con `DbContextOptions<AppDbContext>`, sin `DbSet` de negocio).
- `Persistence/Repository<TEntity>` con `AppDbContext` y `DbSet<TEntity>`.
  - `Query` devuelve `_set.AsNoTracking()` filtrado por el predicado. Debe documentarse que es solo lectura. Se propone `AsNoTracking` en lecturas, y las modificaciones se hacen con `Update`/`Remove` sobre entidades ya conocidas.
  - `GetPagedAsync`: valida `pageNumber >= 1` y `pageSize >= 1` (`ArgumentOutOfRangeException`), aplica predicado, includes (`Include(string)`), `orderBy`, y luego `Skip((pageNumber-1)*pageSize).Take(pageSize)`. Se usa `long` o `checked` en el cálculo del offset para evitar desbordamiento.
  - `AddAsync`, `Update`, `Remove` solo marcan el estado en el contexto. No llaman a `SaveChanges`.
  - Todos los métodos async pasan el `CancellationToken`.
- `Persistence/UnitOfWork : IUnitOfWork` delega en `AppDbContext.SaveChangesAsync`.
- `DependencyInjection.AddInfrastructure`:
  - Lee `ConnectionStrings:Default` y lanza `InvalidOperationException` si falta o está vacía.
  - `AddDbContext<AppDbContext>(o => o.UseSqlite(cs))`
  - `AddScoped(typeof(IRepository<>), typeof(Repository<>))`
  - `AddScoped<IUnitOfWork, UnitOfWork>()`
  - Requiere agregar `Microsoft.EntityFrameworkCore.Sqlite` (cuyas dependencias ya cubren `Microsoft.Extensions.*`).

### Api
- `appsettings.json`: añadir `"ConnectionStrings": { "Default": "Data Source=aulapedidos.db" }`. Sin secretos (SQLite local).
- `Program.cs` no cambia (ya llama a `AddInfrastructure(builder.Configuration)`).
- Añadir `*.db` a `.gitignore` si no está (no verificado).
- Las `IntegrationTests` deberán proveer la cadena de conexión (por ejemplo, SQLite en memoria o archivo temporal), porque ahora `AddInfrastructure` falla sin ella.

### Domain
Sin cambios. No referencia EF Core.

## Archivos por cambiar o crear
| Acción | Archivo |
|---|---|
| Crear | `src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs` |
| Crear | `src/AulaPedidos.Application/Abstractions/Persistence/IUnitOfWork.cs` |
| Crear | `src/AulaPedidos.Infrastructure/Persistence/AppDbContext.cs` |
| Crear | `src/AulaPedidos.Infrastructure/Persistence/Repository.cs` |
| Crear | `src/AulaPedidos.Infrastructure/Persistence/UnitOfWork.cs` |
| Modificar | `src/AulaPedidos.Infrastructure/DependencyInjection.cs` |
| Modificar | `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj` (paquete EF Core Sqlite) |
| Modificar | `src/AulaPedidos.Api/appsettings.json` |
| Pruebas | Nuevo proyecto o carpeta de tests de Infrastructure con entidad de prueba; ajustar IntegrationTests y ArchitectureTests |

## Decisiones sobre los pendientes del expediente
1. **Total de registros:** mantener `List<TEntity>` como pide el expediente. Quien necesite el total usa `CountAsync(predicado)`. Un `PagedResult` queda fuera hasta que un caso de uso lo exija.
2. **Máximo de `pageSize`:** no fijarlo en el repositorio (es política de negocio/API). Se aplicará en el caso de uso o endpoint cuando existan.
3. **`orderBy`:** opcional en el contrato, tal como indica el expediente. Documentarlo en XML: sin orden la paginación no es determinista. Alternativa más estricta (exigirlo) se descarta por ahora para no ampliar el alcance.
4. **Cadena de conexión:** `ConnectionStrings:Default` en `appsettings.json` de Api, leída desde `IConfiguration` en `AddInfrastructure`.
5. **`AddAsync`:** conservar `Task` por coherencia con EF Core, aunque en SQLite sea síncrono.

## Justificación
- Respeta las cuatro capas: Application define contratos sin EF; Infrastructure implementa; Api compone.
- `IQueryable` en el contrato es una concesión del expediente (permite componer filtros, orden y proyección). Su coste es que acopla el contrato a LINQ provider-dependiente. Se acepta porque se pide expresamente.
- No se introducen patrones nuevos: ni especificaciones, ni `Result`, ni repositorios por entidad.
- Se conserva la firma existente de `AddInfrastructure` para minimizar cambios.

## Aceptación sugerida
- Filtrar con predicado devuelve solo coincidencias; sin predicado devuelve todos.
- `Query` permite componer filtros, orden y proyección y no ejecuta hasta materializar.
- `GetPagedAsync` devuelve solo `pageSize` registros de la página pedida con predicado, orden e includes, y lanza `ArgumentOutOfRangeException` con `pageNumber` o `pageSize` < 1.
- `AddAsync`, `Update` y `Remove` no persisten hasta `SaveChangesAsync`.
- Un `CancellationToken` cancelado produce `OperationCanceledException`.
- Application y Domain no referencian EF Core (test de arquitectura sobre referencias de ensamblado).
- `AddInfrastructure` falla con mensaje claro si falta `ConnectionStrings:Default`, y resuelve `IRepository<T>` e `IUnitOfWork` en alcance scoped.
- `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx` pasan.

## Cómo verificar
1. `dotnet build CursoNETIA.slnx`.
2. Tests de Infrastructure con SQLite en memoria (conexión abierta) y una entidad de prueba.
3. ArchitectureTests: Domain y Application sin referencia a `Microsoft.EntityFrameworkCore*`.
4. `dotnet run --project src/AulaPedidos.Api` y comprobar `/health`.

## Supuestos y comprobaciones
- Domain vacío y `.gitignore` sin revisar. Versión de EF Core Sqlite a alinear con 10.0.x, igual que los demás paquetes.
- Comprobaciones ejecutadas: ninguna (análisis estático, sin comandos).

## Siguiente paso
Pruebas (2B) y luego Coordinador (3). Esta ruta se usa por indicación del usuario en lugar de `docs/informes/BASE-002/`.
