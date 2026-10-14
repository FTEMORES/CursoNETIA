# BASE-002 — Informe de implementación

## Objetivo
Implementar Repository y Unit of Work sobre BASE-001: contratos en Application, implementación con EF Core y SQLite en Infrastructure, registro desde Api y pruebas proporcionales.

## Rol
Implementador.

## Fuentes y archivos consultados
- `docs/informes/BASE-002/3-sintesis.md` (síntesis de 2A y 2B).
- `CursoNETIA.slnx`.
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`, `DependencyInjection.cs`, `Abstractions/Persistence/IRepository.cs` (versión previa de BASE-001).
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`, `DependencyInjection.cs`.
- `src/AulaPedidos.Api/Program.cs`, `appsettings.json`, `AulaPedidos.Api.csproj`.
- `tests/AulaPedidos.ArchitectureTests/ArchitectureTests.cs`, `CompositionTests.cs`, csproj.
- `tests/AulaPedidos.Api.IntegrationTests/ApiTests.cs`, csproj.
- `.gitignore`.

## Cambios realizados

### Application
- `src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs` (reescrito): `GetAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `CountAsync` con predicado opcional; `Query(predicate)`; `GetPagedAsync(pageNumber, pageSize, predicate, orderBy, cancellationToken, params includeStrings)`; `AddAsync`, `Update`, `Remove`. Documentación XML que advierte que sin `orderBy` la paginación no es determinista.
- `src/AulaPedidos.Application/Abstractions/Persistence/IUnitOfWork.cs` (nuevo): `Task<int> SaveChangesAsync(CancellationToken)`.
- Sin dependencias de EF Core en Application.

### Infrastructure
- `Persistence/AppDbContext.cs` (nuevo): contexto EF Core sin `DbSet` de negocio.
- `Persistence/Repository.cs` (nuevo): implementación genérica. Lecturas y `Query` con `AsNoTracking`; `GetPagedAsync` valida `pageNumber` y `pageSize` menores que 1 con `ArgumentOutOfRangeException`, aplica predicado, `Include(string)`, orden y después `Skip`/`Take`, con cálculo de offset en `long` para evitar desbordamiento; `AddAsync`, `Update` y `Remove` validan null y no persisten; `CancellationToken` propagado en todo el I/O.
- `Persistence/UnitOfWork.cs` (nuevo): `SaveChangesAsync` sobre `AppDbContext`.
- `DependencyInjection.AddInfrastructure(IServiceCollection, IConfiguration)` conserva su firma: lee `ConnectionStrings:Default`, lanza `InvalidOperationException` si falta o está vacía, registra `AddDbContext<AppDbContext>` con `UseSqlite`, `IRepository<>` abierto y `IUnitOfWork` como scoped sobre el mismo contexto.
- `AulaPedidos.Infrastructure.csproj`: `Microsoft.EntityFrameworkCore.Sqlite` 10.0.12, solo en esta capa.

### Api
- `appsettings.json`: `ConnectionStrings:Default = "Data Source=aulapedidos.db"`.
- `Program.cs` sin cambios.
- `.gitignore`: añadidos `*.db`, `*.db-shm`, `*.db-wal`.

### Pruebas
- Nuevo proyecto `tests/AulaPedidos.Infrastructure.Tests` (añadido a `CursoNETIA.slnx` con `dotnet sln add`), con SQLite en memoria y conexión abierta durante la prueba.
  - `TestEntity.cs`: `TestEntity`, `TestChild` y `TestDbContext`, solo de pruebas.
  - `RepositoryTests.cs`: predicado y ausencia de predicado, lista vacía sin coincidencias, `FirstOrDefaultAsync`/`AnyAsync`/`CountAsync`, composición diferida de `Query` sin seguimiento, página intermedia, última página parcial y página fuera de rango, combinación de predicado + orden + include, validación de valores menores que 1, persistencia solo tras `SaveChangesAsync` comprobada con un segundo contexto, cancelación y argumentos nulos.
  - `InfrastructureCompositionTests.cs`: cadena de conexión ausente o en blanco, y repositorio, unidad de trabajo y `AppDbContext` compartidos por scope y distintos entre scopes.
- `tests/AulaPedidos.ArchitectureTests/ArchitectureTests.cs`: tres proyectos de pruebas en la solución; paquetes esperados de Infrastructure con EF Core Sqlite; Domain y Application sin referencias ni paquetes de EF Core o SQLite; persistencia permitida en Infrastructure y, de forma transitiva, en Api; contratos de persistencia independientes del almacenamiento; tipos declarados por capa actualizados; `AppDbContext` sin `DbSet`; configuración de Api que admite solo `ConnectionStrings:Default` con prefijo `Data Source=`.
- `tests/AulaPedidos.ArchitectureTests/CompositionTests.cs`: composición encadenable que registra persistencia scoped; configuración en memoria con la cadena de conexión.
- `tests/AulaPedidos.Api.IntegrationTests/ApiTests.cs`: la prueba de composición ahora espera `IRepository<>` e `IUnitOfWork` resueltos por scope, manteniendo `/health` como único endpoint.

## Decisiones aplicadas (de la síntesis)
- `orderBy` opcional, documentado como no determinista sin orden.
- `AsNoTracking` en `Query` y en las lecturas.
- Sin máximo de `pageSize` y sin total de registros.
- Sin entidades ni lógica de negocio en producción; la entidad de test vive solo en el proyecto de pruebas.
- Firma de `AddInfrastructure` conservada, sin sobrecarga con string.

## Comprobaciones ejecutadas
- `dotnet build CursoNETIA.slnx`: correcto, 0 advertencias, 0 errores.
- `dotnet test CursoNETIA.slnx`: 45 pruebas superadas, 0 fallos (11 en Api.IntegrationTests, 21 en ArchitectureTests, 13 en Infrastructure.Tests).

## Supuestos y notas
- `AddAsync` con un token ya cancelado no lanza `OperationCanceledException` porque EF Core solo espera a los generadores de valores; la prueba lo documenta y verifica la cancelación en lecturas y en `SaveChangesAsync`. Es una desviación respecto al caso de aceptación redactado en 2B.
- No se crean migraciones ni se ejecuta `EnsureCreated` en producción; la base de datos real queda fuera de alcance.
- No se tocaron `docs/expedientes/BASE-002.md` ni `docs/expedientes/BASE-002-repository-unit-of-work.md`; la duplicidad de nombres de expediente sigue pendiente de resolución humana.

## Pendientes
- Confirmar qué expediente de BASE-002 queda vigente.
- Decidir estrategia de creación del esquema (migraciones) en una tarea posterior.
- Revisar en Visual Studio: menú Compilar y Explorador de pruebas, o desde la raíz `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx`.

## Siguiente paso
Revisión humana del código y de las pruebas (pasos 5 y 6 del flujo de clase).

