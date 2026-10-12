# BASE-001 — 2B Pruebas

## Objetivo

Definir, antes de implementar BASE-001, casos verificables para asegurar que las referencias respetan las fronteras entre `Domain`, `Application`, `Infrastructure` y `Api`, y que la base técnica no introduce lógica de negocio ni persistencia ficticia.

## Rol

Pruebas (paso 2B). Este informe propone casos y resultados esperados derivados de los requisitos. No modifica código ni proyectos de pruebas y no acredita ejecuciones que no se han realizado.

## Archivos realmente consultados

- `.github/copilot-instructions.md`, suministrado como instrucción del espacio de trabajo.
- `docs/multiagente-copilot.md`.
- `docs/expedientes/BASE-001.md`.
- `CursoNETIA.slnx`.
- `src/AulaPedidos.Api/AulaPedidos.Api.csproj`.
- `src/AulaPedidos.Api/Program.cs`.
- `src/AulaPedidos.Api/AulaPedidos.Api.http`.
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`.
- `src/AulaPedidos.Domain/AulaPedidos.Domain.csproj`.
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`.

No se encontró `docs/paquetes/BASE-001-paquete-comun.md`. Tampoco existía previamente este informe en la ruta solicitada.

## Análisis del código actual

- `CursoNETIA.slnx` sólo contiene los cuatro proyectos de producción; aún no contiene proyectos de pruebas.
- Las referencias de proyecto observadas son coherentes con la dirección requerida: Domain no referencia proyectos; Application referencia Domain; Infrastructure referencia Application y Domain; Api referencia Application e Infrastructure.
- Los cuatro proyectos apuntan a `net10.0`.
- En los archivos de proyecto consultados no aparecen EF Core, proveedores de base de datos ni `MediatR`. Tampoco aparece todavía el paquete `Mediator` requerido.
- El `Program.cs` actual no llama a `AddApplication` ni `AddInfrastructure`. Conserva un endpoint manual `/health/live`; esto confirma que BASE-001 aún no está implementada, aunque no constituye lógica de negocio.
- En los archivos consultados no se observan entidades, casos de uso, comandos, consultas, handlers, repositorios concretos ni reglas de negocio. Esta observación no sustituye una inspección automatizada de todo el código de producción.

## Casos propuestos

La columna **Comprobación** indica cómo debe convertir el Implementador el requisito en una prueba automatizada o en una revisión reproducible.

| Requisito | Caso | Entrada | Resultado esperado | Comprobación |
|---|---|---|---|---|
| CA 3: dirección de dependencias | REF-001 — Domain aislado | Ensamblado y archivo de proyecto de `AulaPedidos.Domain` | Domain no referencia `AulaPedidos.Application`, `AulaPedidos.Infrastructure`, `AulaPedidos.Api`, ASP.NET Core, EF Core ni bibliotecas de persistencia. | Prueba de arquitectura sobre referencias de ensamblado y validación de `ProjectReference`/`PackageReference`. Detecta inversión de dependencias o contaminación técnica del dominio. |
| CA 3: Application depende de Domain | REF-002 — frontera de Application | Ensamblado y archivo de proyecto de `AulaPedidos.Application` | Entre los proyectos de producción, Application sólo referencia Domain. No referencia Infrastructure ni Api; tampoco EF Core o HTTP. Se admite la dependencia necesaria para composición y `Mediator` solicitada expresamente. | Prueba de arquitectura más inspección del proyecto. Detecta acceso de casos de uso a transporte o implementaciones técnicas. |
| CA 3: Infrastructure implementa detalles | REF-003 — frontera de Infrastructure | Ensamblado y archivo de proyecto de `AulaPedidos.Infrastructure` | Infrastructure puede referenciar Application y, cuando sea necesario, Domain; nunca referencia Api. | Prueba de arquitectura sobre dependencias. Detecta dependencia de infraestructura hacia la raíz de composición/transporte. |
| CA 3 y 4: Api compone | REF-004 — frontera de Api | Archivo de proyecto y `Program.cs` de Api | Api referencia Application e Infrastructure, invoca `AddApplication` y `AddInfrastructure`, y no contiene implementaciones de repositorio ni reglas de negocio. | Inspección de referencias y prueba/revisión de composición. Detecta composición incompleta o lógica ubicada en transporte. |
| CA 1 y 12: pruebas incluidas | REF-005 — solución completa | `CursoNETIA.slnx` tras implementar | La solución conserva los cuatro proyectos de producción e incluye, como mínimo, proyectos separados para pruebas de arquitectura e integración. | Parsear o revisar la solución en una prueba de estructura; además, `dotnet test CursoNETIA.slnx` debe descubrir ambos proyectos. Detecta pruebas creadas pero no agregadas a la solución. |
| CA 5: Mediator correcto | REF-006 — paquete autorizado | Grafo de paquetes restaurado y archivos de proyecto | Existe `Mediator` de martinothamar en la capa de composición correspondiente; no existe paquete `MediatR`, tipo mediador propio ni referencia accidental a ambos. | Inspección automatizada de referencias/paquetes y búsqueda semántica de implementaciones propias de mediador. Detecta paquete equivocado o duplicación casera. |
| CA 6: contrato Repository agnóstico | REF-007 — ubicación y dependencias de `IRepository<T>` | Tipo genérico `IRepository<T>` de producción | El contrato está en Application y su API pública no expone tipos de EF Core, Infrastructure, ASP.NET/HTTP ni una implementación concreta. No obliga a clase base, interfaz de entidad o tipo de identificador aún inexistentes. | Reflexión sobre ensamblado para localizar el tipo y revisar tipos de parámetros/retornos; complementar con inspección de dependencias. Detecta acoplamiento de almacenamiento o decisiones de dominio prematuras. |
| CA 7: ninguna implementación registrada | REF-008 — resolución negativa de Repository | Contenedor construido con `AddApplication` y `AddInfrastructure` y un tipo testigo definido sólo en el proyecto de pruebas | Solicitar `IEnumerable<IRepository<TestEntity>>` devuelve una colección vacía; resolver directamente `IRepository<TestEntity>` no produce una implementación. Ningún repositorio se registra como abierto o cerrado. | Prueba de composición. El tipo testigo permanece en tests, no en producción. Detecta repositorios vacíos, en memoria o registros ficticios. |
| CA 11: ausencia de dominio | NEG-001 — sin modelos de negocio | Todos los tipos declarados en ensamblados de producción | No existen entidades, agregados, value objects ni reglas de negocio. Sólo se admiten contratos y extensiones de composición requeridos por BASE-001. | Prueba de arquitectura por espacios de nombres/tipos permitidos y revisión semántica de los tipos encontrados. No basarse únicamente en buscar palabras. Detecta introducción anticipada de modelo de negocio. |
| CA 11: ausencia de casos de uso | NEG-002 — sin comportamiento de negocio | Tipos de Application y registros de Mediator | No existen comandos, consultas, handlers ni servicios de negocio; `Mediator` puede estar registrado sin handlers ficticios. | Reflexión para detectar implementaciones de interfaces de request/handler de la biblioteca y revisión de servicios registrados. Detecta demostraciones ficticias que amplían el alcance. |
| CA 7 y 11: ausencia de persistencia | NEG-003 — sin almacenamiento | Archivos de proyecto, tipos de producción, configuración y contenedor | No existen EF Core, `DbContext`, migraciones, proveedores, cadenas de conexión, repositorios concretos, colecciones estáticas ni datos de ejemplo. | Inspección de paquetes y tipos, revisión de configuración y prueba negativa de DI. Detecta persistencia real o simulada fuera de alcance. |
| CA 11: Api sin reglas | NEG-004 — endpoints estrictamente técnicos | Endpoints descubiertos en Api y código de `Program.cs` | La API base sólo expone infraestructura técnica requerida, como Health Checks y OpenAPI condicionado al entorno; no publica endpoints de pedidos ni calcula decisiones de negocio. | Prueba de integración/inspección del origen de endpoints. Detecta lógica de negocio introducida directamente en Minimal APIs. |
| CA 11: sin capacidades futuras | NEG-005 — exclusiones críticas | Referencias, servicios registrados y tipos de producción | No existen autenticación/autorización, outbox, workers, mensajería externa ni adaptadores HTTP de negocio. | Prueba de arquitectura y revisión de registros DI/paquetes. Detecta materialización anticipada de decisiones futuras. |

## Diseño recomendado de las comprobaciones

1. Crear un proyecto de pruebas de arquitectura que cargue los cuatro ensamblados y compruebe dependencias reales, no sólo el texto de los `.csproj`.
2. Mantener una lista explícita de fronteras permitidas por proyecto. Para Application, separar referencias a proyectos de producción de paquetes técnicos expresamente autorizados, como `Mediator` y las abstracciones mínimas de DI.
3. Inspeccionar los tipos de producción mediante reflexión para detectar implementaciones de handlers, repositorios y `DbContext`. Los nombres sirven como señal adicional, no como única evidencia.
4. En la prueba negativa de repositorio, declarar `TestEntity` exclusivamente en tests. Construir un `ServiceCollection`, llamar a ambas extensiones y comprobar que no aparece ninguna implementación de `IRepository<TestEntity>`.
5. Hacer que las pruebas fallen mostrando la referencia, tipo o registro prohibido encontrado; así el defecto será accionable.
6. Complementar estas pruebas con los casos de integración de `/health` y OpenAPI exigidos por el expediente, aunque no sean el foco de este informe.

## Evidencia ejecutada por otros

- El expediente declara una revisión documental previa sin ejecutar comandos. Registra como estado observado las referencias actuales entre capas y la ausencia de pruebas, contratos de repositorio, Mediator y lógica de negocio.
- No se aportaron salidas de compilación, ejecución de pruebas, restauración de paquetes ni arranque de la API. Por tanto, no hay resultados ejecutados que puedan darse por superados.

## Verificaciones pendientes

- Confirmar mediante inspección completa o pruebas de arquitectura que no existen otros archivos/tipos de producción fuera de los consultados con lógica de negocio o dependencias prohibidas.
- Tras la implementación, ejecutar `dotnet build CursoNETIA.slnx` y conservar el resultado real.
- Tras la implementación, ejecutar `dotnet test CursoNETIA.slnx` y conservar total de pruebas, aprobadas, fallidas y omitidas.
- Verificar el grafo restaurado para confirmar que el paquete es `Mediator` de martinothamar y que no entra `MediatR` de forma directa ni transitiva.
- Revisar que los proyectos de pruebas estén incluidos en `CursoNETIA.slnx` y apunten a `net10.0`.
- Ejecutar la prueba negativa del contenedor para demostrar la ausencia de implementaciones de `IRepository<T>`.
- Ejecutar los casos de integración de salud y OpenAPI requeridos por BASE-001.

## Conclusiones

El estado actual consultado conserva correctamente la dirección básica de referencias, pero BASE-001 sigue pendiente: faltan los proyectos de pruebas, la composición, `Mediator` y el contrato Repository. La ausencia de lógica de negocio parece cumplirse en los archivos revisados, pero debe quedar protegida mediante pruebas negativas sobre referencias, tipos y registros de DI. Las comprobaciones REF-001 a REF-008 y NEG-001 a NEG-005 definen el esperado y el defecto que debe detectar cada caso.

## Supuestos

- “Referencias” comprende referencias entre proyectos, paquetes NuGet y dependencias visibles en las API públicas de los tipos.
- Las abstracciones técnicas mínimas necesarias para `AddApplication`, `AddInfrastructure` y `Mediator` no se consideran lógica de negocio.
- Un tipo auxiliar definido exclusivamente dentro de tests no incumple la prohibición de añadir entidades ficticias a producción.
- Infrastructure puede conservar su referencia actual a Domain, aunque no debe añadir código que la use sin necesidad.

## Siguiente paso

El Implementador debe materializar estas comprobaciones en los proyectos de pruebas acordados al implementar BASE-001, sin añadir tipos de negocio a producción. Después debe compilar y ejecutar todas las pruebas. El alumno o responsable debe conservar las salidas reales y remitirlas a Pruebas para una evaluación posterior, diferenciada de este análisis inicial.

## Ruta guardada

`docs/informes/Base-001/2b-pruebas.md`

---

## Evaluación posterior

### Alcance y evidencia disponible

Esta evaluación posterior conserva el análisis inicial anterior y contrasta el estado actualmente visible con sus casos. Se revisaron adicionalmente `CursoNETIA.slnx`, los cuatro `.csproj` de producción, `Program.cs`, `AulaPedidos.Api.http`, las extensiones `DependencyInjection.cs` de Application e Infrastructure, `IRepository.cs`, los `.csproj` de los dos proyectos de pruebas, `tests/AulaPedidos.ArchitectureTests/ArchitectureTests.cs` y los criterios de `docs/expedientes/BASE-001.md`.

No se proporcionaron salidas de `dotnet build`, `dotnet test`, restauración ni peticiones HTTP. Los resultados de revisión estática no acreditan compilación, descubrimiento de pruebas ni comportamiento en ejecución. El proyecto de integración está incluido en la solución, pero no se identificó desde las rutas conocidas su archivo fuente; la cobertura concreta de salud y OpenAPI permanece pendiente de evidencia.

### Resultado de la revisión estática

| Requisito | Caso | Entrada | Resultado esperado | Comprobación | Evaluación posterior |
|---|---|---|---|---|---|
| CA 1, 2 y 12 | REF-005 — solución completa | `CursoNETIA.slnx` y seis `.csproj` | Cuatro proyectos de producción y dos de pruebas en `net10.0`. | Revisión de solución y TFM. | **Conforme estáticamente.** La solución contiene los seis proyectos y todos los proyectos consultados declaran `net10.0`. Pendiente `dotnet test`. |
| CA 3 | REF-001 — Domain aislado | `AulaPedidos.Domain.csproj` y prueba arquitectónica | Domain no referencia capas ni tecnología técnica. | Referencias de proyecto/paquete y ensamblado. | **Conforme estáticamente.** Domain no declara referencias ni paquetes; `ArchitectureTests` cubre referencias de proyecto y ensamblado. Pendiente ejecución. |
| CA 3 | REF-002 — frontera de Application | `AulaPedidos.Application.csproj` | Application sólo referencia Domain entre proyectos de producción. | Inspección y prueba arquitectónica. | **Conforme estáticamente.** Sólo referencia Domain; usa `Mediator.Abstractions` y abstracciones de DI. No se observa EF Core ni HTTP. |
| CA 3 | REF-003 — frontera de Infrastructure | `AulaPedidos.Infrastructure.csproj` | Infrastructure no referencia Api. | Inspección y prueba arquitectónica. | **Conforme estáticamente.** Sólo referencia Application; la referencia a Domain no es necesaria en el estado actual. |
| CA 3 y 4 | REF-004 — frontera de Api | `AulaPedidos.Api.csproj` y `Program.cs` | Api compone Application e Infrastructure sin negocio ni repositorios. | Revisión de composición. | **Conforme estáticamente.** Api referencia ambas capas y llama a `AddApplication` y `AddInfrastructure`; no se observa negocio ni repositorios. |
| CA 5 | REF-006 — Mediator correcto | `.csproj`, `Program.cs` y `AddApplication` | Se usa Mediator de martinothamar, sin MediatR ni mediador propio. | Grafo restaurado y revisión de registros. | **Conforme parcialmente.** Se usan `Mediator.Abstractions` y `Mediator.SourceGenerator` 3.0.2, junto a `AddMediator`; no se observa `MediatR` ni clase mediadora propia. Falta validar el grafo restaurado y el origen efectivo de paquetes. |
| CA 6 | REF-007 — contrato Repository agnóstico | `IRepository<T>` | Contrato en Application sin EF, HTTP, Infrastructure ni restricciones de dominio prematuras. | Reflexión e inspección pública. | **Conforme estáticamente.** Está en Application, sólo exige `class`, y expone `AddAsync`/`RemoveAsync` con `CancellationToken`. La prueba arquitectónica inspecciona su forma. |
| CA 7 | REF-008 — ninguna implementación registrada | Extensiones DI y pruebas | No hay implementación ni registro de `IRepository<T>`. | Prueba negativa construyendo DI. | **Conforme parcialmente.** No se observa registro en las extensiones y la prueba arquitectónica impide implementaciones declaradas en producción. Falta construir DI y demostrar que la resolución directa no entrega repositorio y que `IEnumerable<IRepository<TestEntity>>` está vacío. |
| CA 8 y 9 | Salud positiva y ruta anterior | `Program.cs` y `.http` | `GET /health` usa Health Checks; `/health/live` no permanece como contrato. | Prueba de integración HTTP. | **Conforme parcialmente.** Se registra `AddHealthChecks` y `MapHealthChecks("/health")`; no aparece `/health/live`, y `.http` apunta a `/health`. Falta evidencia de HTTP satisfactorio y de retirada de ruta. |
| CA 10 | OpenAPI por entorno | `Program.cs` y `.http` | OpenAPI sólo se publica en Development. | Pruebas HTTP con ambos entornos. | **Conforme parcialmente.** `MapOpenApi()` está condicionado por `IsDevelopment()`. Falta comprobar por integración disponibilidad en Development y ausencia fuera de ese entorno. |
| CA 11 | NEG-001 a NEG-005 — exclusiones | Tipos, paquetes, DI, configuración y Api | Sin dominio, handlers, persistencia, autenticación, outbox, workers, mensajería ni adaptadores de negocio. | Arquitectura, grafo restaurado y revisión semántica. | **Conforme parcialmente.** `ArchitectureTests` restringe tipos, paquetes, configuración y `.http`; los archivos de producción consultados no introducen capacidades excluidas. Falta ejecutar y revisar el grafo restaurado real. |
| CA 12 | Composición mínima | `AddApplication` y `AddInfrastructure` | Ambas extensiones admiten configuración mínima y el proveedor se construye. | Prueba de composición. | **Pendiente.** Las extensiones validan argumentos y devuelven la colección; `Program.cs` las invoca. No se localizó una prueba que construya un `ServiceProvider`, ni hay salida de ejecución. |
| CA 13 | Compilación y pruebas | Salidas de `dotnet build` y `dotnet test` | Ambos comandos finalizan correctamente sin errores o advertencias nuevos atribuibles a BASE-001. | Ejecución real. | **Pendiente sin evidencia.** No se ejecutaron comandos en esta evaluación ni se aportaron salidas de terceros. |

### Cobertura de pruebas revisada

- **Diseño/código de pruebas revisado:** `ArchitectureTests.cs` cubre estructura de solución, referencias declaradas y compiladas, paquetes, forma del contrato, tipos de producción, configuración y archivo `.http`; materializa buena parte de REF-001 a REF-007 y NEG-001 a NEG-005, sujeto a ejecución.
- **Defecto de cobertura identificado:** REF-008 no incluye la comprobación diseñada que crea `TestEntity` en tests, construye DI y verifica resolución negativa. La inspección de tipos no prueba registros de DI indirectos.
- **Evidencia ejecutada por otros:** ninguna aportada para compilación, restauración, pruebas o endpoints.
- **Verificaciones pendientes:** ejecutar compilación y ambas baterías; inspeccionar y ejecutar las pruebas de integración para `/health`, ausencia de `/health/live` y OpenAPI en Development/no Development; y añadir o ejecutar la composición negativa de Repository.

### Pasos concretos para el Implementador o alumno

1. Ejecutar `dotnet build CursoNETIA.slnx` desde la raíz y registrar advertencias y errores.
2. Ejecutar `dotnet test CursoNETIA.slnx` y conservar por proyecto pruebas totales, aprobadas, fallidas y omitidas.
3. En pruebas, crear una `ServiceCollection` con configuración mínima, invocar `AddApplication` y `AddInfrastructure`, construir el proveedor y comprobar que `GetServices<IRepository<TestEntity>>()` está vacío y que la resolución directa no entrega implementación. `TestEntity` debe existir sólo en tests.
4. Verificar por integración `GET /health` (satisfactoria), `GET /health/live` (no contractual/no encontrado), y `GET /openapi/v1.json` en Development (disponible) y fuera de Development (no publicado).
5. Aportar las salidas reales a una nueva evaluación; hasta entonces no declarar superados CA 8 a CA 10, CA 12 y CA 13.

### Conclusión posterior

La implementación visible satisface estáticamente la estructura solicitada, la composición básica, el contrato Repository, la ausencia aparente de persistencia y el reemplazo del endpoint manual por Health Checks. Quedan pendientes pruebas ejecutadas y dos aspectos de cobertura: la resolución negativa de `IRepository<T>` en DI y la confirmación observable de endpoints por entorno. No existe evidencia suficiente para cerrar BASE-001.

### Ruta guardada

`docs/informes/Base-001/2b-pruebas.md`