# BASE-002 — Síntesis de arquitectura y pruebas

## Objetivo
Repository y Unit of Work sobre BASE-001. Contratos en Application, implementación con EF Core y SQLite en Infrastructure. Los `Get` usan predicados reutilizables y permiten componer sobre `IQueryable`, con un `GetPagedAsync` paginado.

## Fuentes leídas
- `docs/informes/BASE-002/2A-Arquitectura.md` (completo).
- `docs/informes/BASE-002/2B-Pruebas.md` (completo).
- No localizado por ambos informes: `docs/paquetes/BASE-001-paquete-comun.md`.
- Ambos informes mencionan `docs/expedientes/BASE-002.md`; el expediente creado en esta conversación se llama `docs/expedientes/BASE-002-repository-unit-of-work.md`. Revisar que el nombre coincida.
- Ambos informes indican que fueron guardados en una ruta indicada por el usuario, distinta de la convención `docs/informes/BASE-002/`. Aquí se leyeron en la ruta convencional.
- No se ejecutaron comandos ni se leyó código adicional en esta síntesis.

## Acuerdos
- Contratos `IRepository<TEntity>` e `IUnitOfWork` en Application, sin EF Core.
- Implementación en Infrastructure con `AppDbContext`, `Repository<TEntity>` y `UnitOfWork`; EF Core Sqlite solo ahí.
- `GetAsync`, `FirstOrDefaultAsync`, `AnyAsync` y `CountAsync` con predicado opcional, además de `Query(predicate)` y `GetPagedAsync` con `List<TEntity>` y sin total.
- Validación de `pageNumber` y `pageSize` menores que 1 con `ArgumentOutOfRangeException`.
- `AddAsync`, `Update` y `Remove` no persisten hasta `SaveChangesAsync`.
- Propagación de `CancellationToken`.
- Repository y UnitOfWork comparten el `AppDbContext` por scope.
- Sin entidades de negocio: se usa una entidad de test.
- Estado actual: Infrastructure no tiene EF Core, `DbContext` ni repositorios; no existen contratos de persistencia.

## Diferencias y puntos a resolver
| Tema | 2A | 2B | Recomendación |
|---|---|---|---|
| Firma de `AddInfrastructure` | Conservar `IConfiguration` y leer `ConnectionStrings:Default` | Mantener `IConfiguration` o sobrecarga con string | Conservar `IConfiguration`; no hace falta sobrecarga |
| Máximo de `pageSize` | No fijarlo en el repositorio | Caso T13 pendiente de decisión | Sin máximo en el repositorio; T13 se descarta por ahora |
| `orderBy` | Opcional, documentado como no determinista sin orden | Exigir orden o aplicar uno por defecto | Opcional con documentación XML (más simple); decisión humana |
| `Query` y tracking | `AsNoTracking`, solo lectura | Decidir y probar | `AsNoTracking`, con prueba |
| Cadena de conexión ausente | `InvalidOperationException` | Excepción clara (T21) | Coinciden |
| Pruebas | Tests de Infrastructure, ArchitectureTests e IntegrationTests existentes | Dice que no existe proyecto de pruebas | 2A encontró `tests/AulaPedidos.ArchitectureTests` y confirmó `IntegrationTests` en la solución. Falta un proyecto para Infrastructure |

Diferencia de código analizado: 2B afirma que no hay proyecto de pruebas conocido, y 2A lista dos en la solución. No es una contradicción de diseño. Confirmar la estructura de `tests/` antes de implementar.

## Alcance recomendado (propuesta, no decisión humana)
1. Application: `Abstractions/Persistence/IRepository.cs` e `IUnitOfWork.cs`.
2. Infrastructure: paquete `Microsoft.EntityFrameworkCore.Sqlite` 10.x, `Persistence/AppDbContext.cs`, `Repository.cs`, `UnitOfWork.cs` y registro en `AddInfrastructure`.
3. Api: `ConnectionStrings:Default` en `appsettings.json` (SQLite local, sin secretos); `Program.cs` sin cambios; revisar `.gitignore` para `*.db`.
4. Pruebas: proyecto de tests de Infrastructure con SQLite en memoria (conexión abierta) y `TestEntity`; ajustar IntegrationTests para proveer la cadena de conexión; ampliar ArchitectureTests.
5. Fuera de alcance: entidades reales, `PagedResult`, máximo de `pageSize`, especificaciones, `Result`.

## Casos de aceptación
Prioridad alta (2B: T01, T07, T12, T14–T16, T18):
- Predicado filtra; sin predicado devuelve todos; sin coincidencias devuelve lista vacía.
- `FirstOrDefaultAsync` devuelve la entidad o null; `AnyAsync` y `CountAsync` respetan el predicado.
- `Query` compone `Where`, `OrderBy` y `Select` sin ejecutar hasta materializar.
- `GetPagedAsync`: página intermedia, última página parcial, página fuera de rango vacía, combinación predicado + orden + includes, y validación de valores menores que 1.
- Add, Update y Remove solo persisten tras `SaveChangesAsync`, comprobado con un segundo contexto; `SaveChangesAsync` devuelve el número de filas.
- Token cancelado produce `OperationCanceledException` en lecturas, `AddAsync` y `SaveChangesAsync`.
- Argumentos nulos producen `ArgumentNullException`.
- Domain y Application sin referencia a EF Core; EF Core y Sqlite solo en Infrastructure.
- DI resuelve `IRepository<T>` e `IUnitOfWork` en scope con el mismo `AppDbContext`.
- Falta de `ConnectionStrings:Default` lanza excepción clara.
- `/health` responde 200 con Infrastructure registrada.
- Sin lógica de negocio: la entidad de test vive solo en pruebas.
- `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx` pasan.

## Pendientes
- Decisión humana: `orderBy` opcional o exigido, y `AsNoTracking` en `Query`.
- Confirmar que no se añade máximo de `pageSize` ni total de registros.
- Confirmar nombre del expediente y estructura de `tests/`.
- Revisar `.gitignore`, el proyecto Domain y la versión exacta de EF Core Sqlite.
- Ninguna comprobación ejecutada hasta ahora.

## Prompt para el Implementador
```
Implementa BASE-002 en AulaPedidos (.NET 10, CursoNETIA.slnx), sobre BASE-001. Trabaja en español, con nombres de código en inglés. No uses MediatR ni un mediador propio; no añadas entidades ni lógica de negocio.

1. Application: crea src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs e IUnitOfWork.cs, sin EF Core.
   IRepository<TEntity> where TEntity : class:
   - Task<List<TEntity>> GetAsync(Expression<Func<TEntity,bool>>? predicate = null, CancellationToken cancellationToken = default)
   - Task<TEntity?> FirstOrDefaultAsync(predicate = null, ct)
   - Task<bool> AnyAsync(predicate = null, ct); Task<int> CountAsync(predicate = null, ct)
   - IQueryable<TEntity> Query(Expression<Func<TEntity,bool>>? predicate = null)
   - Task<List<TEntity>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity,bool>>? predicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy, CancellationToken cancellationToken, params string[] includeStrings)
   - Task AddAsync(TEntity entity, CancellationToken ct = default); void Update(TEntity entity); void Remove(TEntity entity)
   IUnitOfWork: Task<int> SaveChangesAsync(CancellationToken cancellationToken = default).
   Documenta con XML que sin orderBy la paginación no es determinista.
2. Infrastructure: añade Microsoft.EntityFrameworkCore.Sqlite 10.x (solo aquí). Crea Persistence/AppDbContext (sin DbSet de negocio), Repository<TEntity> y UnitOfWork.
   - Query y lecturas usan AsNoTracking.
   - GetPagedAsync valida pageNumber >= 1 y pageSize >= 1 (ArgumentOutOfRangeException), aplica predicado, Include(string), orderBy y luego Skip/Take, con offset sin desbordamiento. Sin máximo de pageSize.
   - AddAsync, Update y Remove validan null (ArgumentNullException), no llaman a SaveChanges.
   - Propaga CancellationToken en todo el I/O.
   - AddInfrastructure(IServiceCollection, IConfiguration) mantiene su firma: lee ConnectionStrings:Default (InvalidOperationException si falta o está vacía), registra AddDbContext con UseSqlite, IRepository<> abierto y IUnitOfWork como scoped, compartiendo el mismo AppDbContext.
3. Api: añade ConnectionStrings:Default = "Data Source=aulapedidos.db" en appsettings.json. Program.cs sin cambios. Añade *.db a .gitignore si falta.
4. Pruebas: crea un proyecto de tests de Infrastructure con SQLite en memoria (conexión abierta) y una TestEntity solo de pruebas. Cubre los casos de aceptación de docs/informes/BASE-002/3-sintesis.md. Ajusta las IntegrationTests para proporcionar la cadena de conexión y añade al ArchitectureTests que Domain y Application no referencian EF Core.
5. Ejecuta dotnet build CursoNETIA.slnx y dotnet test CursoNETIA.slnx e informa de los resultados reales y de lo pendiente.
```

## Decisión humana
Todo lo anterior es propuesta de los informes 2A/2B. Queda pendiente tu confirmación, sobre todo en `orderBy` opcional, `AsNoTracking` en `Query` y ausencia de máximo de `pageSize`.
