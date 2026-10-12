# BASE-002 — Síntesis de arquitectura y pruebas

## Objetivo

Preparar la implementación de Repository Pattern y Unit of Work sobre la base actual, manteniendo la arquitectura por capas. Los contratos deben estar en Application; EF Core y SQLite deben quedar en Infrastructure. El repositorio debe ser reutilizable y permitir consultas con predicados, orden, includes y paginación sobre `IQueryable`, sin crear entidades ni lógica de negocio en Domain.

## Rol

Coordinación, paso 3: síntesis de los informes 2A y 2B.

## Fuentes leídas

- `docs/Base-002/2A-Arquitectura.md`.
- `docs/Base-002/2b-pruebas.md`.
- `.github/copilot-instructions.md`.

El informe 2A indica que consultó el expediente, la solución, los proyectos productivos, la configuración y las pruebas de arquitectura. El informe 2B indica que consultó el expediente, la solución, los contratos actuales, los proyectos de Infrastructure y de pruebas y las pruebas de arquitectura. Ambos señalan que `docs/paquetes/BASE-001-paquete-comun.md` no está disponible. No se ha inventado contenido para suplir esa ausencia.

## Estado actual relevante

- La solución tiene cuatro proyectos productivos y dos proyectos de pruebas, todos en `net10.0`.
- Domain no contiene tipos de producción y no debe modificarse para esta tarea.
- Application contiene `IRepository<T> where T : class`, actualmente limitado a `AddAsync` y `RemoveAsync`.
- Infrastructure contiene únicamente la extensión `AddInfrastructure`; no hay todavía contexto EF Core, repositorio concreto ni Unit of Work.
- Api ya invoca `AddApplication`, `AddInfrastructure` y registra Mediator de martinothamar.
- Infrastructure todavía no declara EF Core ni SQLite.
- Las pruebas de arquitectura reflejan BASE-001: esperan un repositorio mínimo, prohíben paquetes de persistencia y esperan pocos tipos en Infrastructure. Estas expectativas tendrán que actualizarse junto con BASE-002.

## Acuerdos entre las fuentes

1. **Separación por capas:** Application contiene contratos; Infrastructure contiene EF Core, SQLite, `DbContext`, repositorio y Unit of Work; Domain permanece sin persistencia; Api compone mediante DI.
2. **Repositorio reutilizable:** las consultas deben aceptar `Expression<Func<T, bool>>` y componerse sobre `IQueryable` antes de materializar resultados.
3. **Consulta paginada:** aplicar predicado, includes, orden y después `Skip`/`Take`; validar `pageNumber >= 1` y `pageSize >= 1`.
4. **Firma de `params`:** `params string[] includeStrings` debe ser el último parámetro; `CancellationToken` debe estar antes para permitir cancelación.
5. **Unit of Work mínimo:** exponer `SaveChangesAsync(CancellationToken)` sin filtrar `DbContext` ni tipos de EF Core a Application.
6. **Pruebas:** usar SQLite en memoria, con conexión abierta durante cada prueba o fixture, contexto aislado y datos deterministas.
7. **Cobertura:** probar predicados, composición, orden ascendente y descendente, includes, páginas, validación, alta, actualización si se incluye, eliminación, guardado, DI y cancelación.
8. **Verificación:** la implementación debe terminar con `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx`.

## Diferencias y cuestiones no resueltas

No hay una contradicción técnica grave entre los informes, pero sí decisiones que ambos dejan abiertas:

- **Operación `Update`:** 2A recomienda incluirla solo si el caso de uso la necesita; 2B propone probarla si forma parte del contrato final. No debe añadirse automáticamente por completar un CRUD genérico.
- **Predicado y orden opcionales:** 2A propone `predicate` y `orderBy` anulables; 2B deja la prueba condicionada a esa decisión. Debe decidirse antes de fijar las firmas.
- **Orden sin `orderBy` en paginación:** aceptar la llamada es posible, pero el resultado no es determinista. La propuesta es permitirlo en el contrato y exigir orden en los usos que requieran páginas estables, salvo decisión humana distinta.
- **Total de resultados:** ambos documentos orientan `GetPagedAsync` a devolver solo la página. No se propone incluir el total sin un requisito adicional.
- **Includes:** se propone mantener `params string[]` con rutas de navegación y probar `Include(string)`. La prueba requiere entidades auxiliares técnicas, no entidades de Domain.
- **Ubicación de pruebas:** 2B recomienda reutilizar `AulaPedidos.Api.IntegrationTests` para evitar añadir un tercer proyecto, aunque un proyecto específico de Infrastructure sería más claro. Añadirlo exigiría actualizar la solución y la prueba que espera exactamente dos proyectos de pruebas.
- **Migraciones y creación de esquema:** quedan pendientes. No se debe automatizar `EnsureCreated` como comportamiento de producción sin una decisión expresa.
- **Transacciones explícitas:** la propuesta común es no introducirlas en esta base; `SaveChangesAsync` es suficiente hasta que exista un caso de uso que requiera una abstracción transaccional.

## Alcance recomendado

### Application

1. Conservar `IRepository<T> where T : class` en `AulaPedidos.Application.Abstractions.Persistence`.
2. Añadir `GetAsync` y `GetPagedAsync` con `Expression<Func<T, bool>>?`, `Func<IQueryable<T>, IOrderedQueryable<T>>?`, `CancellationToken` y `params string[] includeStrings`, usando `params` en último lugar.
3. Mantener el contrato libre de EF Core, SQLite y `DbContext`.
4. Crear `IUnitOfWork` con `Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)`.
5. Mantener `AddAsync` y `RemoveAsync`; añadir `Update` únicamente si la decisión humana confirma que forma parte del alcance.

### Infrastructure

1. Añadir referencias compatibles de `Microsoft.EntityFrameworkCore` y `Microsoft.EntityFrameworkCore.Sqlite`.
2. Crear un `DbContext` técnico, por ejemplo `AulaPedidosDbContext`.
3. Implementar el repositorio genérico con `Set<T>()`, includes por cadena, predicados, orden, `Skip`/`Take` y materialización asíncrona.
4. Implementar Unit of Work delegando en el contexto.
5. Registrar contexto, repositorio genérico e Unit of Work como scoped en `AddInfrastructure`.
6. Leer la conexión desde configuración, preferiblemente `ConnectionStrings:DefaultConnection`, sin secretos.

### Api y configuración

- Reutilizar la llamada existente `AddInfrastructure(builder.Configuration)`.
- Añadir únicamente la configuración no sensible necesaria para SQLite.
- No añadir endpoints ni lógica de negocio.

### Pruebas

- Mantener las pruebas de arquitectura separadas de las pruebas funcionales de SQLite.
- Usar SQLite en memoria, no el proveedor InMemory de EF Core.
- Crear entidades de prueba técnicas, como `TestItem` y `TestChild`, dentro del proyecto de pruebas.
- Actualizar las expectativas de arquitectura para permitir EF Core/SQLite en Infrastructure y las nuevas clases técnicas, conservando las fronteras entre capas.
- Probar DI, consultas, paginación, escritura, Unit of Work y cancelación.

## Casos de aceptación mínimos

1. `IRepository<T>` e `IUnitOfWork` están en Application y no exponen EF Core.
2. Domain no adquiere referencias ni tipos de persistencia.
3. Infrastructure registra y resuelve el repositorio genérico y Unit of Work con ciclo de vida scoped.
4. Un predicado devuelve únicamente los elementos coincidentes.
5. El orden ascendente y descendente se respeta.
6. Los includes cargan la navegación solicitada.
7. `GetPagedAsync` devuelve la primera, intermedia y última página correctamente.
8. La combinación predicado + orden + paginación se aplica en ese orden.
9. Valores de página menores que 1 se rechazan.
10. Add, Remove y, si se decide, Update persisten correctamente tras `SaveChangesAsync`.
11. Los cambios no se consideran persistidos antes de `SaveChangesAsync`.
12. La cancelación se propaga a las operaciones asíncronas.
13. La solución compila y todas las pruebas pasan.

## Decisiones humanas pendientes

Estas son propuestas de diseño, no decisiones humanas ya aprobadas:

- Confirmar si se incluye una operación `Update`.
- Confirmar si `predicate` y `orderBy` son anulables.
- Confirmar si se permite paginar sin orden explícito.
- Confirmar si se devuelve únicamente la página o también el total.
- Confirmar la ubicación de las pruebas funcionales: proyecto de integración existente o nuevo proyecto de Infrastructure.
- Confirmar estrategia de esquema: migraciones, `EnsureCreated` solo en pruebas u otra alternativa.
- Confirmar si se necesitan transacciones explícitas.
- Confirmar la cadena de conexión por defecto de SQLite.

## Pendientes técnicos

- `docs/paquetes/BASE-001-paquete-comun.md` no está disponible, por lo que no se ha podido contrastar el diseño con ese documento.
- No se han ejecutado compilación ni pruebas durante el análisis.
- Aún no existe la implementación que permita probar SQLite, DI o cancelación.
- Deben actualizarse las pruebas de arquitectura que actualmente codifican restricciones de BASE-001.

## Prompt autocontenido para el Implementador

```text
Implementa BASE-002 en la solución CursoNETIA.slnx, respetando .github/copilot-instructions.md y sin modificar Domain ni crear entidades o lógica de negocio.

Objetivo: incorporar Repository Pattern genérico y Unit of Work. Los contratos deben estar en Application; las implementaciones con EF Core y SQLite deben estar en Infrastructure. Api debe registrar la infraestructura mediante la llamada existente AddInfrastructure(builder.Configuration). No uses MediatR ni un mediador propio; conserva Mediator de martinothamar.

En Application.Abstractions.Persistence:
- Conserva IRepository<T> con where T : class.
- Añade GetAsync y GetPagedAsync usando Expression<Func<T, bool>>? como predicado y Func<IQueryable<T>, IOrderedQueryable<T>>? para ordenar.
- Usa CancellationToken antes de params string[] includeStrings, porque params debe ser el último parámetro.
- En GetPagedAsync valida pageNumber >= 1 y pageSize >= 1 y aplica predicado, includes, orden y después Skip((pageNumber - 1) * pageSize).Take(pageSize).
- No expongas DbContext, DbSet ni tipos de EF Core desde Application.
- Crea IUnitOfWork con Task<int> SaveChangesAsync(CancellationToken cancellationToken = default).
- Mantén AddAsync y RemoveAsync. No añadas Update salvo que documentes y pruebes la decisión.

En Infrastructure:
- Añade versiones compatibles de Microsoft.EntityFrameworkCore y Microsoft.EntityFrameworkCore.Sqlite.
- Crea un DbContext técnico, un repositorio EF genérico y una implementación de IUnitOfWork.
- Construye las consultas sobre IQueryable y materialízalas con async/await y CancellationToken.
- Registra DbContext, IRepository<> e IUnitOfWork como scoped en AddInfrastructure.
- Lee ConnectionStrings:DefaultConnection sin guardar secretos.
- No automatices migraciones ni EnsureCreated en producción; usa la estrategia decidida para las pruebas.

En pruebas:
- Usa SQLite en memoria con una conexión abierta por fixture o prueba; no uses el proveedor InMemory de EF Core.
- Define entidades auxiliares de prueba, por ejemplo TestItem y TestChild, solo en el proyecto de pruebas.
- Prueba DI, predicados, composición, orden ascendente/descendente, includes, primera/intermedia/última página, validación de parámetros, Add/Remove, Update solo si se incorpora, SaveChanges y cancelación.
- Actualiza las pruebas de arquitectura para permitir EF Core/SQLite únicamente donde corresponda y conservar las fronteras de capas.
- Evita añadir un tercer proyecto de pruebas salvo que actualices CursoNETIA.slnx y las expectativas que exigen dos proyectos.

Antes de terminar, ejecuta desde la raíz:
- dotnet build CursoNETIA.slnx
- dotnet test CursoNETIA.slnx

Informa de los archivos modificados, las decisiones tomadas, los resultados reales de build/test y cualquier pendiente. No inventes resultados.
```

## Siguiente paso

Revisar las decisiones humanas pendientes. Después, entregar el prompt autocontenido al Implementador. Esta síntesis no implementa código ni sustituye la revisión humana de las decisiones abiertas.