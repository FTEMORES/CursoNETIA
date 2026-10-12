# BASE-001 — Base técnica implementada

## Alcance

Implementación local solicitada a partir de `docs/informes/Base-001/3-sintesis.md`. Se conservan la solución `CursoNETIA.slnx`, .NET 10, las cuatro capas y Minimal APIs. No se añaden entidades, handlers, reglas, casos de uso, almacenamiento, autenticación ni otras capacidades de negocio.

- Application expone `AddApplication(IServiceCollection)` e `IRepository<T>` en `Abstractions/Persistence`. El contrato tiene `T : class`, `Task AddAsync(T entity, CancellationToken cancellationToken = default)` y `Task RemoveAsync(T entity, CancellationToken cancellationToken = default)`. No define identidad, consultas, unidad de trabajo ni garantías de almacenamiento; no existe implementación ni registro del repositorio.
- Infrastructure expone `AddInfrastructure(IServiceCollection, IConfiguration)`, valida ambos argumentos y devuelve la colección sin añadir servicios ni consultar configuración. Se retira su referencia directa no utilizada a Domain.
- Api compone las capas y Mediator, registra ASP.NET Core Health Checks y publica `/health` en todos los entornos. `/health/live` se retira. `/openapi/v1.json` sólo se publica en Development. `Program` es público y parcial para integración; el archivo `.http` apunta a las rutas vigentes.
- La solución incorpora `tests/AulaPedidos.ArchitectureTests` y `tests/AulaPedidos.Api.IntegrationTests` con xUnit y `net10.0`.

## Diferencia respecto a la síntesis: raíz de composición de Mediator

La síntesis recomienda generar y registrar Mediator desde Application. Sin embargo, `.github/copilot-instructions.md` exige el registro desde Api y `.github/instructions/api.instructions.md` exige instalar `Mediator.SourceGenerator` sólo en Api y llamar allí a `AddMediator`. Se siguen estas instrucciones del proyecto:

- Application referencia `Mediator.Abstractions` y registra mediante `TryAddScoped` los contratos oficiales `ISender` e `IPublisher`, que resuelven el `IMediator` compuesto por Api. No crea un mediador propio.
- Api instala el generador oficial y llama a `AddMediator`, con `options.Assemblies = [typeof(AulaPedidos.Application.DependencyInjection)]` y `ServiceLifetime.Scoped`.
- Los registros de Application mantienen una referencia de ensamblado efectiva a Mediator. Sin uso real de sus tipos, el compilador omite esa referencia y el generador 3.0.2 rechaza escanear Application con `MSG0007`. No se añaden mensajes ni handlers ficticios para evitar esa restricción.
- `AddApplication` por sí solo no proporciona una implementación resoluble de `IMediator`: la composición completa necesita el registro oficial de Api. Las pruebas hospedadas verifican que `IMediator`, `ISender` e `IPublisher` resuelven la misma instancia dentro de un scope y distintas instancias entre scopes.

## Archivos de implementación

- `CursoNETIA.slnx`.
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`.
- `src/AulaPedidos.Application/DependencyInjection.cs`.
- `src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs`.
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`.
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`.
- `src/AulaPedidos.Api/AulaPedidos.Api.csproj`.
- `src/AulaPedidos.Api/Program.cs`.
- `src/AulaPedidos.Api/AulaPedidos.Api.http`.
- `tests/AulaPedidos.ArchitectureTests/AulaPedidos.ArchitectureTests.csproj`.
- `tests/AulaPedidos.ArchitectureTests/ArchitectureTests.cs`.
- `tests/AulaPedidos.ArchitectureTests/CompositionTests.cs`.
- `tests/AulaPedidos.Api.IntegrationTests/AulaPedidos.Api.IntegrationTests.csproj`.
- `tests/AulaPedidos.Api.IntegrationTests/ApiTests.cs`.
- Este documento.

Se conservan los cambios locales previos en `.github`, expediente e informes, y la eliminación previa del archivo de Domain. No se reescriben documentos de otros roles ni se registra aceptación humana.

## Versiones verificadas

| Paquete | Versión |
|---|---|
| Mediator.Abstractions / Mediator.SourceGenerator, martinothamar | 3.0.2 |
| Microsoft.Extensions.DependencyInjection.Abstractions / Configuration.Abstractions | 10.0.12 |
| Microsoft.AspNetCore.OpenApi / Mvc.Testing | 10.0.12 |
| xunit | 2.9.3 |
| xunit.runner.visualstudio | 4.0.0 |
| Microsoft.NET.Test.Sdk | 18.10.1 |

## Comprobaciones ejecutadas

Ejecutadas por el implementador mediante las herramientas del espacio de trabajo:

- `dotnet build CursoNETIA.slnx --tl:off -v:minimal`: correcto, **0 errores y 0 advertencias**.
- `dotnet test CursoNETIA.slnx --tl:off -v:minimal`: **32 pruebas aprobadas, 0 fallidas y 0 omitidas**; 21 de arquitectura/composición y 11 de integración.
- `dotnet list CursoNETIA.slnx package --include-transitive --format json`: versiones restauradas comprobadas; sin MediatR ni EF Core. Las pruebas también inspeccionan `project.assets.json` de cada proyecto de producción para impedir persistencia y mensajería no solicitadas.
- `git diff --check` y comprobación focalizada de los archivos de producción: sin incidencias.
- Integración mediante `WebApplicationFactory<Program>`: `/health` devuelve 200 y `Healthy` en Development, Production y Staging; `/health/live` y la ruta de plantilla devuelven 404; OpenAPI devuelve un documento JSON válido en Development y 404 en Production y Staging.
- Fronteras comprobadas mediante referencias de proyectos, dependencias de ensamblados compilados y grafo restaurado. Contrato público, restricciones de tipos de producción, composición sin base de datos y ausencia de implementaciones abiertas o cerradas del repositorio comprobados.

Incidencias intermedias resueltas: referencia omitida a Mediator (`MSG0007`) y una prueba negativa que inicialmente confundía los auxiliares generados por Mediator/el compilador con tipos propios de negocio. Los totales anteriores corresponden a la ejecución final correcta.

## Límites y siguiente paso

- Las restricciones negativas fijan la base actual de BASE-001; deberán revisarse cuando se autoricen capacidades reales en otra tarea. No son una auditoría exhaustiva de seguridad ni de toda lógica posible.
- No se ha arrancado la API con `dotnet run` ni se ha publicado. La comprobación HTTP ejecutada utiliza hospedaje de pruebas.
- Para comprobarla manualmente desde Visual Studio, seleccionar Api como proyecto de inicio y el perfil `http`: `/health` debe devolver `Healthy`; `/openapi/v1.json` debe responder sólo en Development. Para probar Production sin alterar configuración guardada, usar el terminal con `dotnet run --project src/AulaPedidos.Api --no-launch-profile -- --environment Production --urls http://localhost:5002`.
- No se encontró `docs/paquetes/BASE-001-paquete-comun.md`; la petición directa, la síntesis y las instrucciones disponibles aportaron el alcance.
- Pendientes la revisión independiente y la decisión de cierre del responsable humano. Este documento acredita la ejecución local indicada, no esas decisiones.
