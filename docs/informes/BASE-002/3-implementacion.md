# BASE-002 — Informe de implementación

## Objetivo

Registrar el estado real de lo implementado para BASE-002 (Repository Pattern y Unit of Work sobre EF Core/SQLite), tomando como contexto `docs/Base-002/3-sintesis.md`.

## Rol

Implementador.

## Archivos consultados

- `docs/Base-002/3-sintesis.md`
- `src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs`
- `src/AulaPedidos.Application/Abstractions/Persistence/IUnitOfWork.cs`
- `src/AulaPedidos.Application/Abstractions/IRepository.cs`
- `src/AulaPedidos.Application/DependencyInjection.cs`
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`
- `src/AulaPedidos.Infrastructure/Persistence/AulaPedidosDbContext.cs`
- `src/AulaPedidos.Infrastructure/Persistence/EfRepository.cs`
- `src/AulaPedidos.Infrastructure/Persistence/EfUnitOfWork.cs`
- `tests/AulaPedidos.Api.IntegrationTests/EfRepositoryTests.cs`
- `tests/AulaPedidos.ArchitectureTests/ArchitectureTests.cs`
- `tests/AulaPedidos.ArchitectureTests/CompositionTests.cs`
- `CursoNETIA.slnx`

Nota: este informe describe código ya presente en el árbol de trabajo (no fue escrito en esta sesión); no hay commits previos en el repositorio (`git status` muestra todo el contenido como no rastreado, `??`), por lo que no hay un diff anterior con el que comparar.

## Estado implementado

### Application

- `AulaPedidos.Application.Abstractions.Persistence.IRepository<T>` (`T : class`) con `AddAsync`, `RemoveAsync`, `GetAsync` y `GetPagedAsync`. Ambas consultas aceptan `Expression<Func<T, bool>>?` opcional, `Func<IQueryable<T>, IOrderedQueryable<T>>?` opcional para orden, `CancellationToken` y `params string[] includeStrings` como último parámetro. No se añadió `Update`.
- `AulaPedidos.Application.Abstractions.Persistence.IUnitOfWork` con `Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)`. No expone tipos de EF Core.
- Coexiste un contrato previo distinto, `AulaPedidos.Application.Abstractions.IRepository<T>` (restringido a `IAggregateRoot`, sin implementación, documentado como no implementado en esta base). No se tocó ni se unificó con el nuevo contrato porque la síntesis no lo pedía expresamente.

### Infrastructure

- `AulaPedidosDbContext : DbContext` técnico, sin `DbSet` de dominio (no hay entidades de Domain que mapear en esta base).
- `EfRepository<T>` implementa `IRepository<T>` usando `context.Set<T>()`, `Include(string)` por cadenas, `Where(predicate)`, `orderBy?.Invoke(query)`, y `Skip`/`Take` para paginación con validación de `pageNumber >= 1` y `pageSize >= 1` (`ArgumentOutOfRangeException` si no se cumple).
- `EfUnitOfWork` delega en `context.SaveChangesAsync(cancellationToken)`.
- `AddInfrastructure` registra `AulaPedidosDbContext` con `UseSqlite(connectionString)` leyendo `ConnectionStrings:DefaultConnection` de la configuración (lanza `InvalidOperationException` si falta), y registra `IRepository<>`/`EfRepository<>` e `IUnitOfWork`/`EfUnitOfWork` como *scoped*.

### Pruebas

- La solución (`CursoNETIA.slnx`) sólo incluye dos proyectos de pruebas: `AulaPedidos.ArchitectureTests` y `AulaPedidos.Api.IntegrationTests`, conforme a la recomendación de la síntesis de no crear un tercer proyecto. Existen además las carpetas `tests/AulaPedidos.Api.Tests`, `tests/AulaPedidos.Application.Tests` y `tests/AulaPedidos.Infrastructure.Tests` en disco, pero **no están referenciadas en el `.slnx`** y por tanto no se compilan ni se ejecutan con `dotnet test CursoNETIA.slnx`.
- `EfRepositoryTests` (en `AulaPedidos.Api.IntegrationTests`) usa SQLite en memoria con una conexión abierta por fixture y entidades técnicas de prueba (`TestItem`), cubriendo: predicado + orden + paginación con validación de límites, includes, alta/baja con persistencia tras `SaveChangesAsync`.
- `ArchitectureTests`/`CompositionTests` validan fronteras entre capas y composición de DI; no se detectaron pruebas rotas por la incorporación de EF Core/SQLite en Infrastructure.

## Comprobaciones ejecutadas

- `dotnet build CursoNETIA.slnx`: **correcto**, 0 advertencias, 0 errores.
- `dotnet test CursoNETIA.slnx`: **correcto**. Resultado real:
  - `AulaPedidos.ArchitectureTests.dll`: 21 superadas, 0 con error, 0 omitidas.
  - `AulaPedidos.Api.IntegrationTests.dll`: 14 superadas, 0 con error, 0 omitidas.
  - Total: 35 pruebas superadas.

## Supuestos

- Se asume que `tests/AulaPedidos.Api.Tests`, `tests/AulaPedidos.Application.Tests` y `tests/AulaPedidos.Infrastructure.Tests` son restos no incorporados a la solución actual y quedan fuera del alcance de esta verificación, ya que el `.slnx` sólo declara cuatro proyectos productivos y dos de pruebas.
- No se unificó `IRepository<T>` (Abstractions) con `IRepository<T>` (Abstractions.Persistence) por no haber una decisión humana expresa al respecto; ambos contratos conviven sin colisión de nombres completos (namespaces distintos).

## Pendiente / siguiente paso

- Decidir si los proyectos de pruebas no referenciados (`Api.Tests`, `Application.Tests`, `Infrastructure.Tests`) deben añadirse al `.slnx`, eliminarse o mantenerse fuera de la solución actual.
- Si se requiere `Update` en `IRepository<T>`, añadirlo junto con su prueba cuando exista un caso de uso que lo necesite (no se incluyó en este alcance).
- Revisar si procede inicializar el repositorio Git con un primer commit, dado que actualmente todo el árbol aparece como no rastreado.
