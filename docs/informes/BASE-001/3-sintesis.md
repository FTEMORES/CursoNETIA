# BASE-001 — Síntesis de arquitectura y pruebas

## Objetivo

Integrar el alcance y los criterios de aceptación de BASE-001 a partir del expediente y de los informes independientes de Arquitectura (2A) y Pruebas (2B), dejando una recomendación implementable sin confundir propuestas técnicas con decisiones humanas ya aprobadas.

Esta síntesis no acredita implementación, compilación ni ejecución de pruebas.

## Fuentes leídas

- `docs/expedientes/BASE-001.md`.
- `docs/informes/Base-001/2A-arquitectura.md`.
- `docs/informes/Base-001/2b-pruebas.md`.
- `.github/copilot-instructions.md`, disponible como instrucción del espacio de trabajo.
- `docs/multiagente-copilot.md`.

Limitación compartida por las fuentes: no existe o no se encontró `docs/paquetes/BASE-001-paquete-comun.md`. El expediente aporta objetivo, alcance, exclusiones y aceptación suficientes para esta síntesis. No se asume información procedente de otros chats.

## Estado común observado

Los tres documentos coinciden en que BASE-001 aún no está implementada:

- La solución conserva cuatro proyectos de producción en .NET 10: Domain, Application, Infrastructure y Api.
- Las referencias actuales son Domain sin referencias de proyecto, Application a Domain, Infrastructure a Application y Domain, y Api a Application e Infrastructure.
- OpenAPI ya se mapea sólo en Development.
- La salud actual es un endpoint manual `/health/live`, no ASP.NET Core Health Checks.
- Faltan `AddApplication`, `AddInfrastructure`, el paquete `Mediator`, `IRepository<T>` y los proyectos de pruebas.
- No se observaron entidades, lógica de negocio, persistencia, autenticación, handlers ni repositorios concretos en los archivos consultados.
- El archivo `.http` apunta a un endpoint de plantilla inexistente.
- No se han ejecutado restauración, compilación, pruebas ni arranque de la API; no hay evidencia de ejecución que pueda darse por superada.

## Acuerdos

### Arquitectura y composición

1. Conservar `CursoNETIA.slnx`, .NET 10, Minimal APIs y las cuatro capas existentes.
2. Mantener Domain libre de dependencias técnicas y Application dependiente de Domain, nunca de Infrastructure o Api.
3. Mantener Api como raíz de composición, con referencias a Application e Infrastructure.
4. Crear `AddApplication(IServiceCollection)` en Application y `AddInfrastructure(IServiceCollection, IConfiguration)` en Infrastructure.
5. Hacer que ambas extensiones validen sus argumentos, devuelvan el mismo `IServiceCollection` y no registren capacidades ficticias.
6. Registrar desde `AddApplication` el paquete `Mediator` de martinothamar mediante su API oficial, limitando el escaneo al ensamblado de Application. Api inicia ese registro al llamar a `AddApplication`.
7. No usar `MediatR`, no crear un mediador propio y no añadir handlers de demostración.
8. Mantener `AddInfrastructure` sin EF Core, proveedores, cadenas de conexión, opciones ficticias, repositorios ni servicios de prueba.

### API técnica

1. Registrar ASP.NET Core Health Checks y mapear `GET /health` en todos los entornos.
2. Eliminar el mapeo manual de `/health/live`; la ruta anterior no forma parte del contrato de BASE-001.
3. Mantener el documento OpenAPI disponible sólo en Development y no publicarlo en otros entornos.
4. Actualizar `AulaPedidos.Api.http` para representar endpoints que existan realmente.
5. No introducir controllers ni endpoints de negocio.

### Repository Pattern

1. Ubicar `IRepository<T>` en Application, dentro de una abstracción de persistencia.
2. Mantener su API pública independiente de EF Core, HTTP, Infrastructure y cualquier almacenamiento concreto.
3. No imponer una interfaz o clase base de entidad, tipo de identificador, `IQueryable`, especificaciones, paginación, unidad de trabajo o `SaveChanges`.
4. No crear ni registrar implementaciones abiertas o cerradas de `IRepository<T>`.
5. Se admite un tipo testigo definido exclusivamente en tests para demostrar la ausencia de registros; no debe existir una entidad ficticia en producción.

### Pruebas y exclusiones

1. Añadir a la solución proyectos separados para pruebas de arquitectura y pruebas de integración de Api, ambos en .NET 10.
2. Comprobar dependencias reales entre ensamblados y complementar esa evidencia con revisión de referencias de proyecto y paquetes.
3. Probar composición mínima, `/health`, ausencia de `/health/live`, OpenAPI por entorno y ausencia de una implementación de `IRepository<T>` en DI.
4. Proteger mediante comprobaciones negativas la ausencia de entidades, reglas, casos de uso, comandos, consultas, handlers, EF Core, `DbContext`, proveedores, migraciones, datos de ejemplo, autenticación, outbox, workers y adaptadores de negocio.
5. No materializar en BASE-001 las decisiones futuras del ADR 002.
6. Compilar y ejecutar todas las pruebas después de implementar, comunicando resultados reales.

## Diferencias y decisiones pendientes

| Punto | Expediente | Arquitectura 2A | Pruebas 2B | Síntesis |
|---|---|---|---|---|
| Referencia `Infrastructure → Domain` | La admite cuando corresponda. | Recomienda retirarla porque hoy no se usa. | Permite conservarla y prohíbe añadir uso innecesario. | **Propuesta:** retirarla en BASE-001 para expresar la dependencia mínima actual. **Decisión humana pendiente:** conservarla también sería compatible con 2B y con el expediente si se justifica. |
| Miembros de `IRepository<T>` | Exige un contrato mínimo, pero deja su forma pendiente. | Propone `AddAsync` y `RemoveAsync` con `CancellationToken`; considera poco útil una interfaz vacía. | No prescribe miembros; exige neutralidad técnica y verifica ubicación y API pública. | **Propuesta:** adoptar sólo `AddAsync` y `RemoveAsync`, sin lectura, actualización, identificador ni consulta. **Decisión humana pendiente:** la forma concreta no estaba aprobada en el expediente. |
| Cantidad y nombres de proyectos de pruebas | Al menos dos; nombres y framework por concretar. | Propone exactamente `AulaPedidos.ArchitectureTests` y `AulaPedidos.Api.IntegrationTests`, con xUnit. | Exige separación entre arquitectura e integración, sin contradecir esos nombres. | **Propuesta:** usar exactamente los dos proyectos y xUnit para evitar estructura adicional. **Decisión humana pendiente:** nombres y framework no estaban fijados previamente. |
| Dependencias de pruebas de arquitectura | Requiere validar fronteras. | Prefiere reflexión y referencias de ensamblado sin añadir inicialmente una biblioteca de arquitectura. | Pide comprobar ensamblados, proyectos y paquetes; los nombres no bastan. | **Recomendación:** implementar pruebas xUnit directas y pequeñas; no introducir una biblioteca adicional salvo necesidad demostrada. |
| Exposición de `Program` a integración | No lo concreta. | Propone `public partial class Program` para `WebApplicationFactory<Program>`. | Requiere integración hospedada, sin fijar mecanismo. | **Recomendación:** usar `public partial class Program` porque habilita `WebApplicationFactory` sin cambiar Minimal APIs. |

No hay desacuerdo sobre Mediator, Health Checks, OpenAPI, ausencia de persistencia real ni exclusión de lógica de negocio.

## Alcance recomendado

### Producción

- Conservar sin cambios de negocio `AulaPedidos.Domain`.
- En Application:
  - añadir `DependencyInjection.cs` con `AddApplication`;
  - añadir `Abstractions/Persistence/IRepository.cs`;
  - referenciar `Mediator` de martinothamar y sólo las abstracciones DI necesarias;
  - registrar Mediator con el ensamblado de Application, sin handlers ficticios.
- En Infrastructure:
  - añadir `DependencyInjection.cs` con `AddInfrastructure`;
  - usar únicamente las abstracciones de DI y configuración necesarias;
  - no registrar persistencia ni leer configuración de base de datos;
  - retirar la referencia directa a Domain si se adopta la recomendación de dependencia mínima.
- En Api:
  - invocar `AddApplication` y `AddInfrastructure(builder.Configuration)`;
  - conservar `AddOpenApi` y `MapOpenApi` condicionado a Development;
  - registrar `AddHealthChecks` y mapear `/health`;
  - eliminar `/health/live`;
  - exponer `Program` a las pruebas de integración sin introducir `Startup` ni controllers;
  - actualizar el archivo `.http`.

### Pruebas

- Crear `tests/AulaPedidos.ArchitectureTests` para fronteras, referencias, tipos y exclusiones.
- Crear `tests/AulaPedidos.Api.IntegrationTests` para composición, Health Checks y OpenAPI por entorno.
- Agregar ambos proyectos a la carpeta `tests` de `CursoNETIA.slnx` y apuntarlos a `net10.0`.
- Usar xUnit y `Microsoft.AspNetCore.Mvc.Testing` donde corresponda, sin incorporar datos o tipos ficticios a producción.

### Fuera de alcance consolidado

- Entidades, agregados, value objects, reglas, casos de uso, comandos, consultas, handlers y servicios de negocio.
- EF Core, `DbContext`, migraciones, proveedores, base de datos, cadenas de conexión y repositorios concretos o en memoria.
- Autenticación, autorización, outbox, workers, mensajería, adaptadores HTTP de negocio, seed y datos de ejemplo.
- MediatR, mediador propio, controllers, UI, Docker y despliegue.
- Implementación anticipada del ADR 002 o de otros patrones no requeridos.

## Casos y criterios de aceptación consolidados

| Id | Caso | Resultado esperado |
|---|---|---|
| SOL-001 | Solución y frameworks | `CursoNETIA.slnx` conserva los cuatro proyectos de producción, incluye los dos proyectos de pruebas acordados y todos apuntan a `net10.0`. |
| REF-001 | Domain aislado | Domain no referencia Application, Infrastructure, Api, ASP.NET Core, EF Core ni persistencia. |
| REF-002 | Frontera de Application | Entre proyectos de producción, Application sólo referencia Domain; sus únicas dependencias técnicas son las mínimas para DI y `Mediator`. |
| REF-003 | Frontera de Infrastructure | Infrastructure referencia Application, nunca Api; si se acepta la recomendación, elimina su referencia actualmente innecesaria a Domain. |
| REF-004 | Api compone | Api referencia Application e Infrastructure, llama a `AddApplication` y `AddInfrastructure` y no contiene reglas ni repositorios. |
| MED-001 | Paquete autorizado | Se usa `Mediator` de martinothamar con su API oficial; no aparece `MediatR`, un mediador propio ni handlers ficticios. |
| REP-001 | Contrato agnóstico | `IRepository<T>` vive en Application y no expone tipos de EF Core, HTTP, Infrastructure, identidad o consulta no definidos. |
| REP-002 | Resolución negativa | Tras registrar Application e Infrastructure, `IEnumerable<IRepository<TestEntity>>` está vacío y no se resuelve una implementación individual. `TestEntity` existe sólo en tests. |
| DI-001 | Composición mínima | Ambas extensiones validan argumentos, permiten encadenamiento y el proveedor se construye con configuración mínima, sin base de datos. |
| API-001 | Salud positiva | `GET /health` responde satisfactoriamente mediante ASP.NET Core Health Checks. |
| API-002 | Ruta anterior retirada | `/health/live` no está mapeado como endpoint contractual y devuelve no encontrado. |
| API-003 | OpenAPI en Development | El documento OpenAPI, normalmente `/openapi/v1.json`, está disponible en Development. |
| API-004 | OpenAPI fuera de Development | El documento OpenAPI devuelve no encontrado en un entorno no Development. |
| API-005 | Archivo HTTP coherente | `AulaPedidos.Api.http` sólo contiene peticiones válidas para esta base técnica. |
| NEG-001 | Sin dominio ni casos de uso | Los ensamblados de producción no contienen entidades, reglas, comandos, consultas, handlers o servicios de negocio. |
| NEG-002 | Sin persistencia | No aparecen EF Core, `DbContext`, migraciones, proveedores, cadenas de conexión, repositorios concretos, colecciones estáticas ni datos de ejemplo. |
| NEG-003 | Sin capacidades futuras | No aparecen autenticación, autorización, outbox, workers, mensajería externa ni adaptadores de negocio. |
| VER-001 | Compilación | `dotnet build CursoNETIA.slnx` finaliza sin errores ni advertencias nuevas atribuibles a BASE-001. |
| VER-002 | Pruebas | `dotnet test CursoNETIA.slnx` descubre ambos proyectos de pruebas y finaliza correctamente; se informa el total de pruebas aprobadas, fallidas y omitidas. |

Las comprobaciones negativas deben inspeccionar referencias, API pública, tipos y registros de DI. No deben depender únicamente de buscar palabras o convenciones de nombres.

## Pendientes y comprobaciones posteriores

1. **Decisión humana:** aprobar o cambiar las propuestas sobre la referencia `Infrastructure → Domain`, los dos miembros mínimos de `IRepository<T>` y los nombres/framework de pruebas.
2. **Implementador:** comprobar una versión de `Mediator` compatible con .NET 10 y usar la firma real de registro; no inventar adaptadores.
3. **Implementador:** revisar cualquier archivo de código adicional que aparezca al trabajar y conservarlo salvo conflicto directo con BASE-001.
4. **Implementador:** restaurar, compilar y ejecutar pruebas; verificar `/health`, `/health/live` y el documento OpenAPI en Development y fuera de Development.
5. **Pruebas:** evaluar las salidas reales y el grafo restaurado, incluida la ausencia directa y transitiva de `MediatR`.
6. **Revisor:** comprobar el cambio final, las fronteras, la ausencia de capacidades fuera de alcance y la correspondencia entre criterios y pruebas.
7. **Responsable humano:** aceptar, pedir cambios o dejar pendientes a partir de evidencia real. Esta síntesis no representa por sí sola esa aprobación.

## Prompt autocontenido para el Implementador

> Implementa BASE-001 en `CursoNETIA.slnx` con .NET 10, aplicando únicamente esta base técnica. Conserva AulaPedidos.Domain, Application, Infrastructure y Api; no añadas negocio. En Application crea `AddApplication(IServiceCollection)` y registra mediante su API oficial el paquete `Mediator` de martinothamar, limitado al ensamblado de Application; no uses MediatR, un mediador propio ni handlers ficticios. Crea `IRepository<T>` en `AulaPedidos.Application.Abstractions.Persistence`, sin dependencias de EF Core, HTTP o Infrastructure, sin identidad, consultas, `IQueryable`, `SaveChanges` o implementación concreta. Salvo decisión humana distinta, usa como contrato mínimo `Task AddAsync(T entity, CancellationToken cancellationToken = default)` y `Task RemoveAsync(T entity, CancellationToken cancellationToken = default)`, con `T : class`. En Infrastructure crea `AddInfrastructure(IServiceCollection, IConfiguration)`, valida argumentos y devuelve el mismo `IServiceCollection`; no leas cadenas de conexión ni registres EF Core, opciones, repositorios o servicios ficticios. Salvo decisión humana distinta, retira la referencia no usada de Infrastructure a Domain. En Api invoca ambas extensiones, conserva Minimal APIs, registra ASP.NET Core Health Checks, mapea `/health` en todos los entornos, elimina `/health/live` y publica OpenAPI sólo en Development. Añade `public partial class Program` para integración y actualiza `AulaPedidos.Api.http` con endpoints reales. Crea exactamente `tests/AulaPedidos.ArchitectureTests` y `tests/AulaPedidos.Api.IntegrationTests` con xUnit, ambos `net10.0`, agrégalos a la solución y usa `Microsoft.AspNetCore.Mvc.Testing` en integración. Cubre fronteras de ensamblados y proyectos, composición, paquete Mediator autorizado, API pública de `IRepository<T>`, ausencia de implementaciones de repositorio en DI mediante un tipo testigo sólo de tests, `/health`, ausencia de `/health/live`, OpenAPI en Development y no Development, y exclusiones de negocio y persistencia. No añadas entidades de producción, casos de uso, EF Core, base de datos, autenticación, outbox, workers, controllers, Docker ni datos de ejemplo. Usa paquetes y firmas realmente compatibles con .NET 10. Ejecuta `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx`; informa archivos cambiados, versiones elegidas, totales de pruebas, resultados reales y pendientes sin afirmar comprobaciones no ejecutadas.

## Ruta guardada

`docs/informes/BASE-001/3-sintesis.md`