# BASE-002: Repository y Unit of Work

## Objetivo
Implementar el patrón Repository y Unit of Work sobre BASE-001. Los contratos van en Application. Las implementaciones con EF Core y SQLite van en Infrastructure. Los `Get` usan predicados reutilizables para poder componerlos sobre `IQueryable`.

## Estado
Propuesta pendiente de validación humana. Sin implementar.

## Alcance propuesto
- **Application**
  - `IRepository<TEntity>` con:
    - `GetAsync(Expression<Func<TEntity,bool>>? predicate, CancellationToken)`
    - `FirstOrDefaultAsync(predicate, ...)`
    - `AnyAsync(predicate, ...)`
    - `CountAsync(predicate, ...)`
    - `Query(predicate?)`, que devuelve `IQueryable<TEntity>`
    - `GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity,bool>>? predicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy, CancellationToken cancellationToken, params string[] includeStrings)`, que devuelve `List<TEntity>`
    - `AddAsync`, `Update` y `Remove`
  - `IUnitOfWork` con `SaveChangesAsync(CancellationToken)`.
- **Infrastructure**
  - `Repository<TEntity>`, `UnitOfWork` y `AppDbContext` con SQLite.
  - `AddInfrastructure(connectionString)` para el registro en DI.
  - Paquetes EF Core y Sqlite solo en Infrastructure.
- **Api**
  - Lee la cadena de conexión de la configuración y registra Infrastructure.
- Sin entidades ni lógica de negocio. Las pruebas usan una entidad de test.

## Casos de aceptación
- Filtrar con predicado devuelve solo las coincidencias.
- Sin predicado se devuelven todos los registros.
- `Query` permite componer filtros, orden y proyección antes de ejecutar.
- `GetPagedAsync` devuelve solo `pageSize` registros de la página `pageNumber`, aplicando predicado, orden e includes.
- `GetPagedAsync` rechaza `pageNumber` o `pageSize` menores que 1.
- Add, Update y Remove no persisten
- Se respeta la cancelación con `CancellationToken`.
- Domain y Application no referencian EF Core.

## Pendientes
- Comprobar el código actual de BASE-001 (contratos, paquetes, `DbContext`).
- Decidir si `GetPagedAsync` debe devolver también el total de registros (por ejemplo, un `PagedResult`), y si hay un máximo de `pageSize`.
- `orderBy` es obligatorio en la práctica para que la paginación sea estable; decidir si se exige.
- Decidir dónde se ubica la cadena de conexión.

## Flujo siguiente
1. Arquitecto: `docs/informes/BASE-002/2A-arquitectura.md`
2. Pruebas: `docs/informes/BASE-002/2B-pruebas.md`
3. Coordinador: `docs/informes/BASE-002/3-sintesis.md`
4. Implementador.
