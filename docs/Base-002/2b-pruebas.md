# BASE-002 — Diseño de pruebas

**Ruta guardada:** `docs/base-002/2B-Pruebas.md`

## Objetivo

Definir las pruebas de aceptación para Repository Pattern y Unit of Work sobre EF Core y SQLite, verificando predicados, orden, includes, paginación, operaciones de escritura, persistencia y cancelación sin introducir entidades ni lógica de negocio en Domain.

## Rol

Pruebas (paso 2B).

## Archivos realmente consultados

- `docs/expedientes/BASE-002.md`.
- `.github/copilot-instructions.md`.
- `CursoNETIA.slnx`.
- `src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs`.
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`.
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`.
- `tests/AulaPedidos.ArchitectureTests/AulaPedidos.ArchitectureTests.csproj`.
- `tests/AulaPedidos.ArchitectureTests/ArchitectureTests.cs`.
- `tests/AulaPedidos.Api.IntegrationTests/AulaPedidos.Api.IntegrationTests.csproj`.

No se consultó el informe de Arquitectura 2A para formar esta propuesta. Tampoco se pudo leer `docs/paquetes/BASE-001-paquete-comun.md`, porque no está disponible en la ruta indicada.

## Estado observado

- La solución contiene actualmente cuatro proyectos productivos y dos proyectos de pruebas.
- `IRepository<T>` existe en Application, restringido a `class`, pero solo declara `AddAsync` y `RemoveAsync`.
- Infrastructure no tiene referencias de EF Core/SQLite en su proyecto mostrado y no hay implementación de persistencia observable en los archivos consultados.
- Las pruebas de arquitectura actuales describen el estado de BASE-001: esperan un contrato mínimo y prohíben paquetes de persistencia. Estas expectativas deberán actualizarse cuando BASE-002 se implemente.
- El proyecto de integración referencia Api, pero actualmente no declara `Microsoft.EntityFrameworkCore.Sqlite` ni una prueba de persistencia visible en los archivos consultados.

## Estrategia propuesta

Usar SQLite en memoria, no el proveedor InMemory de EF Core, para probar el comportamiento relacional más cercano al proveedor solicitado. Mantener abierta una única `SqliteConnection` durante cada prueba o fixture para evitar que la base desaparezca al cerrar el contexto. Crear un contexto aislado por prueba y sembrar datos deterministas.

La entidad de prueba debe ser técnica y local al proyecto de pruebas, por ejemplo `TestItem` y `TestChild`, sin añadir entidades de negocio a Domain. Para probar includes, `TestItem` debe tener una navegación a `TestChild` y el modelo de pruebas debe configurarla en el contexto usado por la prueba o mediante una entidad de soporte compatible con el diseño implementado.

La fixture debe:

1. Abrir la conexión SQLite en memoria.
2. Crear el esquema solo para la prueba o colección.
3. Insertar datos con valores conocidos y suficientes para distinguir filtro, orden y página.
4. Crear el repositorio y el Unit of Work con el mismo contexto.
5. Liberar contexto y conexión al terminar.

No deben verificarse resultados que no hayan sido ejecutados. Los casos siguientes son diseño de pruebas y quedan pendientes hasta que exista la implementación.

## Casos de aceptación

| Requisito | Caso | Entrada | Resultado esperado | Comprobación |
|---|---|---|---|---|
| Contrato en Application | El repositorio sigue siendo independiente del almacenamiento | Inspección de ensamblado y firma de `IRepository<T>` | El contrato está en Application, usa `Expression<Func<T,bool>>`/LINQ y no referencia EF Core, SQLite ni `DbContext` | Pendiente: prueba de arquitectura y reflexión |
| Contrato en Application | Existe Unit of Work sin filtrar Infrastructure | Inspección de `IUnitOfWork` | Expone como mínimo `SaveChangesAsync(CancellationToken)` y no expone `DbContext` ni tipos EF | Pendiente: prueba de arquitectura y reflexión |
| Registro | Se resuelve el repositorio genérico desde DI | `ServiceProvider` con `AddInfrastructure` y configuración SQLite válida | Se resuelve `IRepository<TestItem>` y se obtiene una instancia scoped | Pendiente: prueba de integración de DI |
| Registro | Se resuelve Unit of Work desde DI | Mismo contenedor | Se resuelve `IUnitOfWork` y comparte el ámbito esperado con el contexto | Pendiente: prueba de integración de DI |
| Get con predicado | Filtrar elementos | `item => item.Category == "A"` sobre datos de categorías A y B | La lista contiene solo elementos de categoría A | Pendiente: prueba SQLite ejecutada |
| Get con predicado | Predicado composible | `item => item.Name.StartsWith("Al") && item.IsActive` | Se evalúan ambas condiciones y no aparecen elementos que incumplan una de ellas | Pendiente: prueba SQLite ejecutada |
| Get sin predicado | Obtener todos los elementos | `predicate: null` | Devuelve todos los elementos sembrados | Pendiente: prueba SQLite ejecutada, si el contrato permite nulos |
| Get ordenado | Orden ascendente | `orderBy: query => query.OrderBy(item => item.Name)` | La secuencia devuelta respeta el orden ascendente por nombre | Pendiente: prueba SQLite ejecutada |
| Get ordenado | Orden descendente | `orderBy: query => query.OrderByDescending(item => item.CreatedAt)` | La secuencia devuelta respeta el orden descendente | Pendiente: prueba SQLite ejecutada |
| Includes | Cargar navegación solicitada | `includeStrings: ["Child"]` con un `TestItem` relacionado | La navegación `Child` está cargada y sus datos son accesibles sin consulta posterior | Pendiente: prueba SQLite ejecutada; depende de una entidad de soporte para navegación |
| Includes | Ignorar includes no solicitados | Sin `includeStrings` | La prueba debe comprobar el comportamiento acordado por el Implementador; no debe asumir carga automática | Pendiente: fijar expectativa según el contrato final |
| Paginación | Primera página | `pageNumber = 1`, `pageSize = 2`, predicado nulo y orden por `Id` | Devuelve los dos primeros elementos ordenados | Pendiente: prueba SQLite ejecutada |
| Paginación | Página intermedia | `pageNumber = 2`, `pageSize = 2`, cinco elementos, orden por `Id` | Devuelve los elementos tercero y cuarto, sin duplicados ni desplazamiento incorrecto | Pendiente: prueba SQLite ejecutada |
| Paginación | Última página incompleta | `pageNumber = 3`, `pageSize = 2`, cinco elementos | Devuelve únicamente el quinto elemento | Pendiente: prueba SQLite ejecutada |
| Paginación | Predicado, orden y página combinados | Predicado `IsActive`, orden descendente por `Id`, página 2 de tamaño 2 | Primero filtra, después ordena y finalmente aplica `Skip`/`Take`; el resultado coincide con esa secuencia | Pendiente: prueba SQLite ejecutada |
| Validación | Número de página cero | `pageNumber = 0`, `pageSize = 2` | `ArgumentOutOfRangeException` o excepción documentada equivalente | Pendiente: prueba de contrato |
| Validación | Tamaño de página cero | `pageNumber = 1`, `pageSize = 0` | `ArgumentOutOfRangeException` o excepción documentada equivalente | Pendiente: prueba de contrato |
| Validación | Valores negativos | `pageNumber = -1` o `pageSize = -1` | Se rechaza la entrada antes de ejecutar la consulta | Pendiente: prueba de contrato |
| Paginación determinista | Consulta paginada sin orden | `orderBy: null` | Debe verificarse la decisión documentada: permitirla con orden no garantizado o rechazarla | Pendiente: decisión humana y prueba asociada |
| Add | Añadir entidad | Nuevo `TestItem` y `AddAsync` | La entidad queda agregada al contexto; tras `SaveChangesAsync`, existe en una nueva consulta | Pendiente: prueba SQLite ejecutada |
| Update | Actualizar entidad | Entidad existente con cambio de propiedad | Tras guardar y consultar un nuevo contexto, el valor actualizado persiste | Pendiente: incluir solo si el contrato final define actualización |
| Remove | Eliminar entidad | Entidad existente y `RemoveAsync` | Tras `SaveChangesAsync`, la entidad ya no aparece | Pendiente: prueba SQLite ejecutada |
| Unit of Work | Confirmar varios cambios juntos | Un alta y una eliminación antes de `SaveChangesAsync` | Devuelve el número de cambios esperado y ambos cambios quedan persistidos | Pendiente: prueba SQLite ejecutada |
| Unit of Work | No guardar implícitamente | Cambio en contexto sin llamar a `SaveChangesAsync` | Una nueva consulta no observa el cambio persistido | Pendiente: prueba SQLite ejecutada |
| Cancelación | Cancelar lectura | `CancellationToken` cancelado antes de `GetAsync`/`GetPagedAsync` | La operación observa el token y termina con `OperationCanceledException` o excepción compatible | Pendiente: prueba SQLite ejecutada; puede depender del proveedor y del punto de cancelación |
| Cancelación | Cancelar guardado | Token cancelado antes de `SaveChangesAsync` | La operación no se presenta como guardada y propaga la cancelación según el contrato | Pendiente: prueba SQLite ejecutada |
| Separación de capas | Application no incorpora EF Core | Revisión de referencias compiladas y paquetes restaurados | EF Core y SQLite aparecen solo en Infrastructure o pruebas autorizadas | Pendiente: prueba de arquitectura actualizada |
| Separación de capas | Domain no incorpora persistencia | Revisión de ensamblado y proyecto Domain | Domain continúa sin referencias ni tipos de persistencia | Pendiente: prueba de arquitectura |
| Solución | Compilación | `dotnet build CursoNETIA.slnx` | Compilación sin errores | Pendiente: no ejecutado en este análisis |
| Solución | Pruebas completas | `dotnet test CursoNETIA.slnx` | Todas las pruebas pasan | Pendiente: no ejecutado en este análisis |

## Organización recomendada de las pruebas

La opción de menor impacto es ampliar `tests/AulaPedidos.Api.IntegrationTests`, porque ya referencia Api y por transitividad puede alcanzar Infrastructure. Para crear la conexión SQLite explícitamente, el proyecto de pruebas deberá declarar la dependencia de SQLite que utilice el fixture, o encapsular esa creación en una API de pruebas claramente autorizada.

Si el volumen de pruebas crece, crear un proyecto `tests/AulaPedidos.Infrastructure.Tests` sería más claro, pero exigiría actualizar `CursoNETIA.slnx` y las pruebas que actualmente afirman que existen exactamente dos proyectos de pruebas. No se recomienda introducir ese proyecto sin una decisión expresa.

Las pruebas de arquitectura deben mantenerse separadas de las pruebas funcionales de SQLite. Deben actualizarse para:

- aceptar `Microsoft.EntityFrameworkCore` y `Microsoft.EntityFrameworkCore.Sqlite` en Infrastructure;
- dejar de afirmar que Infrastructure solo contiene `DependencyInjection`;
- comprobar que los contratos siguen en Application;
- conservar la prohibición de EF Core/SQLite en Domain y Application;
- verificar que Api no contiene una implementación de repositorio.

## Supuestos

- La implementación final devolverá listas materializadas y no expondrá un `IQueryable` cuya vida dependa de un `DbContext` externo.
- `predicate` y `orderBy` podrían ser opcionales, tal como permite el expediente, pero esa decisión aún no está fijada.
- `params string[] includeStrings` será el último parámetro; el `CancellationToken` quedará antes.
- `GetPagedAsync` devolverá la página, no el total, salvo que se decida un resultado paginado distinto.
- El proyecto no tiene actualmente entidades de negocio; las entidades de prueba serán auxiliares y no se añadirán a Domain.
- No se exige migración ni `EnsureCreated` como comportamiento de producción en esta fase.

## Evidencia y comprobaciones

### Análisis del código realizado

- Se confirmó que `IRepository<T>` actual solo tiene `AddAsync` y `RemoveAsync`.
- Se confirmó que Infrastructure no declara actualmente paquetes de EF Core/SQLite en el archivo consultado.
- Se confirmó que los proyectos de pruebas existentes son ArchitectureTests y Api.IntegrationTests.
- Se confirmó que las pruebas actuales de arquitectura contienen expectativas específicas de BASE-001 que entrarían en conflicto con la implementación de BASE-002.

### Evidencia ejecutada por otros

No se ha aportado ninguna salida de `dotnet build`, `dotnet test` ni de pruebas de SQLite. Por tanto, no se marca ningún caso como ejecutado.

### Verificaciones pendientes

- Crear o actualizar las pruebas funcionales después de implementar los contratos.
- Confirmar las firmas finales y el comportamiento ante predicados/orden nulos.
- Confirmar cómo se modelará el include en las entidades auxiliares de prueba.
- Ejecutar `dotnet build CursoNETIA.slnx`.
- Ejecutar `dotnet test CursoNETIA.slnx`.
- Revisar cualquier fallo de las pruebas de arquitectura causado por sus expectativas de BASE-001.

## Siguiente paso

Revisar esta matriz con el diseño de contratos y decidir las cuestiones pendientes: actualización explícita, nulabilidad de predicado y orden, total de resultados, transacciones y ubicación definitiva de las pruebas. Después, el Implementador puede crear la implementación y las pruebas sin modificar Domain.

---

## Evaluación posterior (tras la implementación)

Esta sección se añade después de que el Implementador entregó `docs/informes/BASE-002/3-implementacion.md`. El análisis inicial de arriba se conserva sin cambios.

### Objetivo de esta evaluación

Contrastar los casos de aceptación diseñados en el análisis inicial contra el código y las pruebas realmente presentes en el árbol de trabajo, distinguiendo evidencia ejecutada de comprobaciones que quedaron pendientes.

### Archivos consultados en esta evaluación

- `src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs`
- `src/AulaPedidos.Application/Abstractions/Persistence/IUnitOfWork.cs`
- `src/AulaPedidos.Infrastructure/Persistence/AulaPedidosDbContext.cs`
- `src/AulaPedidos.Infrastructure/Persistence/EfRepository.cs`
- `src/AulaPedidos.Infrastructure/Persistence/EfUnitOfWork.cs`
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`
- `tests/AulaPedidos.Api.IntegrationTests/EfRepositoryTests.cs`
- `tests/AulaPedidos.ArchitectureTests/ArchitectureTests.cs`
- `tests/AulaPedidos.ArchitectureTests/CompositionTests.cs`
- `CursoNETIA.slnx`
- `docs/informes/BASE-002/3-implementacion.md`

No se ejecutaron comandos nuevos para esta evaluación; se toman como evidencia ejecutada los resultados que el informe de implementación declara (`dotnet build` y `dotnet test`, 35 pruebas superadas: 21 en `AulaPedidos.ArchitectureTests` y 14 en `AulaPedidos.Api.IntegrationTests`). Esta cifra es consistente con el recuento de métodos `[Fact]`/`[Theory]` leídos en los archivos de prueba consultados.

### Resultado por caso diseñado

| Requisito | Caso diseñado | Estado frente a la implementación | Evidencia |
|---|---|---|---|
| Contrato en Application | Repositorio independiente del almacenamiento | Ejecutado y aprobado | `ArchitectureTests.PersistenceContractsAreStorageIndependent` (incluida en las 21 pruebas superadas) |
| Contrato en Application | `IUnitOfWork` sin filtrar Infrastructure | Ejecutado y aprobado | Mismo método, verifica `SaveChangesAsync` devuelve `Task<int>` |
| Registro | Resolver `IRepository<T>` desde DI | Ejecutado y aprobado | `CompositionTests.CompositionRegistersPersistenceContractsWithConfiguredStorage` |
| Registro | Resolver `IUnitOfWork` desde DI | Ejecutado y aprobado | Mismo método |
| Get con predicado | Filtrar por categoría A/B | No implementado tal como se diseñó | `TestItem` no tiene propiedad `Category`; el filtro probado usa `IsActive`, no es el mismo caso |
| Get con predicado | Predicado composible (`StartsWith` + booleano) | No cubierto | `EfRepositoryTests` solo usa un predicado simple (`item.IsActive`) |
| Get sin predicado | Obtener todos los elementos con datos sembrados | No cubierto directamente | Sólo existe `GetAsync()` sin predicado para comprobar una lista vacía tras eliminar; no hay caso con datos presentes y predicado nulo |
| Get ordenado | Orden ascendente | Ejecutado y aprobado | `GetPagedAsync_AppliesPredicateOrderAndPagination` usa `OrderBy(item => item.Name)` |
| Get ordenado | Orden descendente | No cubierto | Ninguna prueba usa `OrderByDescending` |
| Includes | Cargar navegación solicitada | Ejecutado y aprobado | `GetAsync_LoadsRequestedIncludeAndRemovePersistsAfterUnitOfWork` carga `Child` y verifica su valor |
| Includes | Ignorar includes no solicitados | No cubierto | No existe caso que compruebe `Child` sin pedir el include |
| Paginación | Primera página aislada | No cubierto como caso aislado | Sólo se prueba la página 2 combinada con predicado y orden |
| Paginación | Página intermedia aislada | No cubierto como caso aislado | Igual que arriba; el escenario combinado no equivale al caso de "página intermedia" sin filtro |
| Paginación | Última página incompleta | No cubierto | No hay escenario con una página parcial final |
| Paginación | Predicado + orden + página combinados | Ejecutado y aprobado | `GetPagedAsync_AppliesPredicateOrderAndPagination` (resultado esperado `["Gamma"]`) |
| Validación | `pageNumber = 0` | Ejecutado y aprobado | Mismo método, `Assert.ThrowsAsync<ArgumentOutOfRangeException>` sobre `GetPagedAsync(0, 2)` |
| Validación | `pageSize = 0` | No cubierto | No hay aserción específica para `pageSize = 0` (el código sí valida, `EfRepository.GetPagedAsync`, pero no hay prueba) |
| Validación | Valores negativos | No cubierto | No hay prueba con `pageNumber`/`pageSize` negativos |
| Paginación determinista | `orderBy: null` en `GetPagedAsync` | No cubierto; decisión no documentada explícitamente en prueba | El contrato permite `orderBy` nulo, pero no hay prueba ni comentario que fije el comportamiento esperado |
| Add | Alta persistida tras `SaveChangesAsync` | Cubierto de forma indirecta, no como caso aislado | `AddAsync` se usa como preparación en ambos tests de `EfRepositoryTests`, pero no hay un caso dedicado sólo a "alta y verificación en nuevo contexto" |
| Update | Actualizar entidad | No implementado (decisión coherente con el informe de implementación) | `IRepository<T>` no declara `Update`; documentado como decisión pendiente en `3-implementacion.md` |
| Remove | Eliminar entidad | Ejecutado y aprobado | `GetAsync_LoadsRequestedIncludeAndRemovePersistsAfterUnitOfWork` verifica lista vacía tras `RemoveAsync` + `SaveChangesAsync` |
| Unit of Work | Confirmar varios cambios juntos (alta + baja antes de guardar) | No cubierto como caso aislado | No hay prueba que verifique el valor devuelto por `SaveChangesAsync` tras combinar alta y baja en la misma transacción |
| Unit of Work | No guardar implícitamente | No cubierto | No hay prueba que compruebe que un cambio sin `SaveChangesAsync` no se observa en una nueva consulta |
| Cancelación | Cancelar lectura | Ejecutado y aprobado | `GetAsync_ObservesCancellation` |
| Cancelación | Cancelar guardado (`SaveChangesAsync`) | No cubierto | No existe prueba de cancelación sobre `EfUnitOfWork.SaveChangesAsync` |
| Separación de capas | Application sin EF Core | Ejecutado y aprobado | `ArchitectureTests.ProjectReferencesAndPackagesRespectLayerBoundaries` y `RestoredGraphAllowsPersistenceOnlyInInfrastructure` |
| Separación de capas | Domain sin persistencia | Ejecutado y aprobado | Mismos métodos |
| Solución | `dotnet build CursoNETIA.slnx` | Ejecutado y aprobado (según informe de implementación) | Reportado como "correcto, 0 advertencias, 0 errores" en `3-implementacion.md`; no reejecutado en esta evaluación |
| Solución | `dotnet test CursoNETIA.slnx` | Ejecutado y aprobado (según informe de implementación) | Reportado como 35/35 superadas en `3-implementacion.md`; no reejecutado en esta evaluación |

### Hallazgo adicional no previsto en el diseño inicial

El informe de implementación señala que `CursoNETIA.slnx` sólo referencia dos proyectos de pruebas (`ArchitectureTests` y `Api.IntegrationTests`), y que `tests/AulaPedidos.Api.Tests`, `tests/AulaPedidos.Application.Tests` y `tests/AulaPedidos.Infrastructure.Tests` existen en disco pero no se compilan ni ejecutan. Esto es coherente con lo observado en `ArchitectureTests.SolutionContainsExactlyFourProductionAndTwoTestProjectsOnNet10`, que fija expresamente "dos proyectos de pruebas" como parte del contrato de arquitectura. No se puede descartar que esos proyectos no referenciados contengan más casos de los aquí señalados como "no cubiertos"; esta evaluación no los analizó porque no participan en `dotnet test CursoNETIA.slnx`.

### Conclusión de la evaluación

- Los casos centrados en **separación de capas, contratos, DI, includes con navegación, validación del límite inferior de página, cancelación de lectura y eliminación con persistencia** están ejecutados y aprobados con evidencia real (35 pruebas superadas).
- Varios casos del diseño original quedan **sin prueba dedicada**, aunque el código de `EfRepository`/`EfUnitOfWork` pueda comportarse correctamente de forma incidental: predicados compuestos, orden descendente, páginas aisladas (primera/intermedia/última), `pageSize`/valores negativos, comportamiento sin `orderBy`, "no guardar implícitamente", cancelación de `SaveChangesAsync` y combinación de alta+baja en una sola unidad de trabajo.
- `Update` queda fuera de alcance de forma consistente entre diseño e implementación (decisión explícita, no un defecto).
- No se detectaron pruebas que afirmen cubrir un caso y en realidad no lo hagan; la discrepancia es por **ausencia de prueba**, no por prueba fallida o engañosa.

### Siguiente paso (evaluación posterior)

- Decidir si los casos marcados como "no cubiertos" deben añadirse como pruebas en `AulaPedidos.Api.IntegrationTests` antes de considerar cerrada la tarea, priorizando cancelación de `SaveChangesAsync` y "no guardar implícitamente" por ser los de mayor riesgo de regresión silenciosa.
- Aclarar el estado de los proyectos de pruebas no referenciados en el `.slnx` antes de asumir que no aportan cobertura adicional.