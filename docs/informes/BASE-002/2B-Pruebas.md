# BASE-002 — 2B Pruebas (diseño propuesto)

## Objetivo
Definir casos verificables para Repository y Unit of Work (EF Core + SQLite) sobre BASE-001, sin implementar código.

## Rol
Pruebas (2B). Análisis de requisitos y código; no se editó código ni se ejecutaron comandos.

## Archivos consultados
- docs/expedientes/BASE-002.md
- src/AulaPedidos.Api/Program.cs
- src/AulaPedidos.Infrastructure/DependencyInjection.cs
- src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj
- src/AulaPedidos.Application/AulaPedidos.Application.csproj
- src/AulaPedidos.Api/appsettings.json
- docs/informes/Base-001/2b-pruebas.md (solo formato)
- No localizado: docs/paquetes/BASE-001-paquete-comun.md.

## Estado del código actual (análisis, no ejecutado)
- Infrastructure solo referencia Application y paquetes de abstracciones (Configuration, DI). No hay EF Core, SQLite, DbContext ni repositorios.
- `AddInfrastructure(IServiceCollection, IConfiguration)` valida nulos y no registra nada. El expediente propone `AddInfrastructure(connectionString)`: discrepancia a decidir.
- Application referencia Domain, Mediator.Abstractions y DI Abstractions; sin EF Core (cumple el requisito).
- Api llama a `AddApplication`, `AddInfrastructure(builder.Configuration)` y `AddMediator`. `appsettings.json` no tiene `ConnectionStrings`.
- No existe proyecto de pruebas conocido: se requiere crear `tests/...` con una entidad de test (p. ej. `TestEntity`) y SQLite in-memory (`DataSource=:memory:` con conexión abierta) o archivo temporal.

## Casos propuestos
| ID | Requisito | Caso | Entrada | Resultado esperado | Defecto que detecta | Comprobación |
|---|---|---|---|---|---|---|
| T01 | Filtrar con predicado | GetAsync con predicado | 5 registros, predicado `x.Value > 3` (2 cumplen) | Devuelve solo los 2 | Predicado ignorado | Test de integración SQLite |
| T02 | Sin predicado | GetAsync(null) | 5 registros | Devuelve los 5 | Filtro implícito | Test de integración |
| T03 | Sin coincidencias | GetAsync con predicado sin match | Predicado imposible | Lista vacía, no null | NRE / null | Test |
| T04 | FirstOrDefault | Con y sin coincidencia | Predicado existente / inexistente | Entidad / null | Excepción en vacío | Test |
| T05 | Any y Count | Con predicado y sin él | 5 registros, predicado 2 coincidencias | Any true, Count 2; sin predicado Count 5; vacío Any false | Predicado no aplicado en Count/Any | Test |
| T06 | Query componible | `Query(p)` + `Where` + `OrderBy` + `Select` | Datos mixtos | Resultado correcto, traducido a SQL en una sola ejecución; no ejecuta al llamar `Query` | Materializar antes de tiempo | Test + verificar con log SQL o conteo |
| T07 | Paginación | GetPagedAsync(2, 3) | 10 registros ordenados por Id | Registros 4–6 exactamente | Offset/skip erróneo | Test |
| T08 | Paginación última página | Página parcial | 10 registros, página 4 tamaño 3 | 1 registro | Error de límites | Test |
| T09 | Página fuera de rango | Página 10 | 10 registros, tamaño 3 | Lista vacía | Excepción | Test |
| T10 | Paginación con predicado y orden | predicado + orderBy desc | Datos mixtos | Filtra primero, ordena y luego pagina | Orden de operaciones incorrecto | Test |
| T11 | Includes | `includeStrings` con navegación | Entidad de test con relación | Navegación cargada; con include inválido lanza excepción de EF | Includes ignorados | Test (requiere entidad relacionada de test) |
| T12 | Validación | pageNumber 0 o -1; pageSize 0 o -1 | Valores < 1 | `ArgumentOutOfRangeException` | Validación ausente | Test parametrizado |
| T13 | Validación pageSize máximo | pageSize sobre el máximo (si se decide) | p. ej. 1000 | Rechazo o recorte según decisión | Consultas sin límite | Pendiente de decisión |
| T14 | Add/Update/Remove no persisten | Add sin SaveChanges | Nuevo contexto tras Add | No aparece en una segunda lectura con otro contexto | Persistencia implícita | Test con dos contextos |
| T15 | Persistencia con UoW | SaveChangesAsync | Add + Save; Update + Save; Remove + Save | Cambios visibles en nuevo contexto; devuelve nº de filas | UoW no comparte contexto | Test; el repositorio y UoW deben compartir el `AppDbContext` por scope |
| T16 | Cancelación | Token ya cancelado | `GetAsync`, `GetPagedAsync`, `SaveChangesAsync`, `AddAsync` | `OperationCanceledException` | Token no propagado | Test |
| T17 | Argumentos nulos | Update(null), Remove(null), AddAsync(null) | null | `ArgumentNullException` | Fallo tardío | Test |
| T18 | Arquitectura | Domain y Application sin EF Core | Revisar csproj y `dotnet list package` | Sin EF Core/Sqlite en Domain ni Application | Fuga de dependencia | Pendiente; test de reflexión de ensamblados referenciados |
| T19 | Paquetes | EF Core y Sqlite solo en Infrastructure | Revisar csproj de Api y Application | Api no referencia EF directamente (salvo necesidad justificada) | Acoplamiento | Pendiente |
| T20 | DI | Resolver `IRepository<T>` e `IUnitOfWork` | Construir ServiceProvider con AddInfrastructure y cadena de prueba | Se resuelven; mismo `AppDbContext` dentro de un scope | Registro incorrecto/lifetime | Test |
| T21 | Configuración | Cadena de conexión ausente o vacía | Sin `ConnectionStrings:Default` | Excepción clara al registrar | Fallo oscuro en runtime | Test |
| T22 | Api | Arranque y `/health` | `WebApplicationFactory<Program>` | 200 OK con Infrastructure registrada | Regresión de BASE-001 | Test de integración |
| T23 | Sin lógica de negocio | No hay entidades de dominio reales | Revisar Domain | Entidad de test solo en proyecto de pruebas | Alcance excedido | Revisión de código |

## Decisiones pendientes que afectan a las pruebas
1. Firma de `AddInfrastructure`: mantener `IConfiguration` (actual) o `connectionString` (expediente). Recomendación: conservar `IConfiguration` y leer `ConnectionStrings:Default`, o añadir sobrecarga con string.
2. `GetPagedAsync` devuelve `List<TEntity>`, sin total. Si se necesita total, usar `CountAsync` aparte o `PagedResult` (cambia T07–T10).
3. Máximo de `pageSize` (T13) y si `orderBy` es obligatorio. Recomendación: exigir orden o aplicar orden por defecto documentado.
4. `Query` devuelve `IQueryable` con tracking o `AsNoTracking`: decidir y probar.
5. Ubicación de la cadena: `appsettings.json` con valor SQLite local; sin secretos.
6. Nombre y ubicación del proyecto de pruebas (no existe aún).

## Evidencia
- Ejecutada por otros: ninguna aportada.
- Ejecutada por mí: ninguna.
- Pendiente: `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx` tras implementar; todos los casos T01–T23.

## Siguiente paso
Coordinador: sintetizar con 2A (si existe) y resolver las decisiones pendientes. Implementador: crear proyecto de pruebas, entidad de test y casos T01–T23 priorizando T01, T07, T12, T14–T16, T18.

## Nota de ruta (análisis inicial)
Se redactó para `docs/Base-002/2B-Pruebas.md`; el archivo se encuentra ahora en la ruta canónica `docs/informes/BASE-002/2B-pruebas.md`, que es donde se guarda esta actualización.

---

# Evaluación posterior (tras la implementación)

El análisis inicial anterior se conserva sin cambios. Esta sección evalúa el código ya implementado y el informe `docs/informes/BASE-002/3-implementracion.md`.

## Archivos revisados en esta evaluación
- `docs/informes/BASE-002/3-implementracion.md`
- `src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs`
- `src/AulaPedidos.Infrastructure/Persistence/Repository.cs`, `AppDbContext.cs`
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`
- `tests/AulaPedidos.Infrastructure.Tests/RepositoryTests.cs`

## Evidencia ejecutada por otros
El informe del Implementador declara `dotnet build` correcto y `dotnet test` con 45 pruebas superadas (11 Api.IntegrationTests, 21 ArchitectureTests, 13 Infrastructure.Tests). No lo he ejecutado ni verificado: queda registrado como evidencia aportada por otro rol.

## Cobertura de los casos propuestos
| Caso | Estado | Nota |
|---|---|---|
| T01, T02, T03 | Cubierto | `GetAsyncFiltersWithThePredicateAndReturnsEverythingWithoutIt` |
| T04, T05 | Cubierto | `FirstOrDefaultAnyAndCountRespectThePredicate` |
| T06 | Parcial | Verifica composición y ausencia de seguimiento; no comprueba una única ejecución SQL |
| T07, T08, T09 | Cubierto | Página intermedia, última parcial y fuera de rango |
| T10 | Cubierto | Predicado + orden descendente + include |
| T11 | Débil | Pasa `nameof(TestEntity.Child)` pero solo asevera valores; no comprueba que la navegación quede cargada ni que un include inválido falle |
| T12 | Cubierto | `Theory` con 0, -1 y -3 |
| T13 | No aplica | Se decidió no limitar `pageSize` |
| T14, T15 | Cubierto | Comprobado con un segundo contexto |
| T16 | Cubierto con desviación documentada | `AddAsync` no observa la cancelación; explicado en el test y en el informe |
| T17 | Cubierto | Incluye constructores nulos |
| T18–T23 | Cubierto según el informe | En `ArchitectureTests`, `CompositionTests`, `InfrastructureCompositionTests` y `ApiTests`; no reverificado aquí |

## Hallazgos del código (análisis, no ejecutado)
1. **Escrituras sobre entidades no rastreadas.** Todas las lecturas y `Query` usan `AsNoTracking`. `Update`/`Remove` sobre una entidad devuelta por `GetAsync` dependen del attach implícito: `Update` generará un UPDATE de todas las columnas y `Remove` falla si la clave no está fijada. El test actual solo usa una entidad ya rastreada por `AddAsync`, así que no cubre el flujo real. Casos nuevos: **T24** leer con `GetAsync`, modificar, `Update` + `SaveChangesAsync` y comprobar en un segundo contexto; **T25** leer con `GetAsync`, `Remove` + `SaveChangesAsync` y comprobar que desaparece.
2. **Desbordamiento en paginación.** Con `skip > int.MaxValue`, `GetPagedAsync` devuelve lista vacía en silencio; no está probado ni documentado en el contrato. **T26**: `GetPagedAsync(int.MaxValue, int.MaxValue, ...)` devuelve lista vacía sin excepción. Alternativa a decidir: lanzar `ArgumentOutOfRangeException`.
3. **Validación de `includeStrings` sin prueba.** El código lanza `ArgumentException` con include nulo o en blanco. **T27**: include `""` o `null` lanza `ArgumentException`. **T28**: include inexistente lanza `InvalidOperationException` de EF Core.
4. **Include no aseverado.** **T29**: tras `GetPagedAsync` con include, `Child` no es `null`; sin include sí lo es. Detecta que el include se descarte.
5. **Tipos no mapeados.** `AppDbContext` no declara `DbSet` y el repositorio usa `Set<TEntity>()`. **T30**: `Repository<T>` sobre un tipo fuera del modelo produce `InvalidOperationException` clara; útil como mensaje de aprendizaje.
6. **Cancelación parcial.** `FirstOrDefaultAsync` y `AnyAsync` no se prueban con token cancelado; conviene parametrizar el test existente.
7. **Ruido en producción.** `DependencyInjection.cs` conserva el bloque de comentario con el flujo de clase; recomiendo retirarlo.
8. **Codificación de `IRepository.cs`.** Los comentarios XML muestran caracteres corruptos (`p?gina`, `cancelaci?n`). Guardar el archivo como UTF-8 con BOM. No rompe la compilación, pero degrada la documentación.
9. **Mensaje de `AddInfrastructure`.** El texto de la `InvalidOperationException` contiene una secuencia `\u00f3` literal; revisar para que el mensaje se lea correctamente.

## Valoración
La implementación cubre los casos de aceptación del expediente y la mayoría de los casos de 2B. Las decisiones divergentes (`orderBy` opcional, firma con `IConfiguration`, sin total de registros) están declaradas y son coherentes con la síntesis. Las brechas detectadas son de robustez y precisión de aserciones, no de diseño.

## Verificaciones pendientes
- Casos nuevos T24–T30 y refuerzo de T06 y T11.
- Reejecutar `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx` tras añadirlos.
- Revisión humana del código y de las pruebas (pasos 5 y 6 del flujo de clase).

## Siguiente paso
Entregar T24–T30 al Implementador o al alumno, junto con las correcciones menores de codificación y la limpieza del comentario en `DependencyInjection.cs`. Informe guardado en `docs/informes/BASE-002/2B-pruebas.md`.
