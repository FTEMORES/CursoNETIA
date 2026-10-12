# BASE-002: Repository Pattern y Unit of Work

## Objetivo
Añadir sobre BASE-001 un Repository genérico reutilizable y un Unit of Work.
Los contratos van en Application. Las implementaciones con EF Core y SQLite van en Infrastructure.
Los métodos Get se basan en predicados para poder componerse sobre `IQueryable`.

## Alcance
- Contratos en Application:
  - `IRepository<TEntity>`.
  - `IUnitOfWork`.
- Los Get reciben `Expression<Func<TEntity, bool>>` como predicado, con opciones de orden, includes y paginación.
- Get paginado (referencia aportada por el alumno, a adaptar en la implementación):
  ```csharp
  Task<List<T>> GetPagedAsync(
      int pageNumber,
      int pageSize,
      Expression<Func<T, bool>> predicate,
      Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
      CancellationToken cancellationToken = default,
      params string[] includeStrings);
  ```
  - Propuesta: `params` debe ir último, por lo que `CancellationToken` va antes. El Arquitecto debe confirmar el orden.
  - Validar `pageNumber >= 1` y `pageSize >= 1`.
  - Aplicar `Skip((pageNumber - 1) * pageSize).Take(pageSize)` tras el predicado y el orden.
  - Decisión pendiente: `orderBy` y `predicate` nulos (propuesta: opcionales) y si devolver también el total de elementos.
- Implementación en Infrastructure:
  - `DbContext` con EF Core y `Microsoft.EntityFrameworkCore.Sqlite`.
  - Repositorio genérico.
  - Unit of Work.
- Registro de DI desde Api.
- Cadena de conexión SQLite en configuración, sin secretos.

## Fuera de alcance
- Entidades de negocio y lógica de negocio (supuesto heredado de BASE-001, no verificado).
- Endpoints nuevos.
- MediatR o un mediador propio. Se mantiene Mediator de martinothamar, ya registrado en BASE-001.
- Specification pattern completo.

## Reglas de arquitectura
- Application no referencia EF Core ni expone `DbContext`.
- Domain no cambia de dependencias.
- Infrastructure referencia Application.
- Todas las operaciones de I/O usan async y `CancellationToken`.

## Decisiones pendientes (humanas)
1. Si se crea una base de entidad (por ejemplo, `Entity` con `Id`) o se usa solo `class`.
2. Si Get devuelve `IReadOnlyList<T>` o también `IQueryable<T>`.
3. Si hay transacciones explícitas en Unit of Work.
4. Si se usa `EnsureCreated` o migraciones.

## Aceptación
- `dotnet build CursoNETIA.slnx` sin errores.
- Pruebas de repositorio sobre SQLite en memoria: predicado, orden, paginación, includes, add/update/remove, SaveChanges y cancelación.

## Limitaciones
- No se pudo leer `docs/paquetes/BASE-001-paquete-comun.md` al redactar este alcance. Las referencias a BASE-001 son supuestos por verificar.
