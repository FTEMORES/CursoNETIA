# BASE-001 — Análisis 2A de arquitectura

## Objetivo y rol

- **Objetivo:** proponer la estructura, referencias, contratos y cambios mínimos para establecer la base técnica de AulaPedidos en .NET 10, sin implementar todavía código de producción ni pruebas.
- **Rol:** Arquitecto, paso 2A.
- **Alcance:** cuatro capas, composición, Mediator de martinothamar, Repository Pattern, Health Checks, OpenAPI y estructura de pruebas definidos por BASE-001.

## Archivos realmente consultados

- `.github/copilot-instructions.md` (aportado en el contexto de la sesión).
- `docs/multiagente-copilot.md`.
- `docs/expedientes/BASE-001.md`.
- `CursoNETIA.slnx`.
- `src/AulaPedidos.Api/AulaPedidos.Api.csproj`.
- `src/AulaPedidos.Api/Program.cs`.
- `src/AulaPedidos.Api/AulaPedidos.Api.http`.
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`.
- `src/AulaPedidos.Domain/AulaPedidos.Domain.csproj`.
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`.
- `docs/adr/002-persistencia-y-outbox.md`.

No se encontró `docs/paquetes/BASE-001-paquete-comun.md`. Tampoco se encontraron `Directory.Build.props`, `Directory.Packages.props`, `global.json`, los `Class1.cs` habituales de plantilla ni un informe 2A anterior en la ruta solicitada o en la ruta canónica comprobada. La ausencia del paquete común no bloquea este análisis porque el expediente contiene objetivo, alcance y aceptación suficientes.

## Estructura actual observada

La solución contiene únicamente cuatro proyectos de producción bajo `src`, todos con `TargetFramework` `net10.0`, nullable e implicit usings habilitados:

```text
CursoNETIA.slnx
└─ src
   ├─ AulaPedidos.Domain
   ├─ AulaPedidos.Application
   ├─ AulaPedidos.Infrastructure
   └─ AulaPedidos.Api
```

Referencias actuales:

```text
Domain          → ninguna capa
Application     → Domain
Infrastructure  → Application + Domain
Api             → Application + Infrastructure
```

La separación básica ya existe. `Infrastructure → Domain` es válida cuando una implementación técnica usa tipos de dominio, pero actualmente no hay tal implementación; para BASE-001 es una referencia innecesaria. Api conserva correctamente el papel de raíz de composición.

Estado funcional relevante:

- Api tiene `Microsoft.AspNetCore.OpenApi` 10.0.12 y publica OpenAPI sólo en Development.
- `/health/live` es un endpoint manual, no ASP.NET Core Health Checks.
- El archivo `.http` pide `/weatherforecast/`, endpoint que no existe.
- No se observan puntos de composición de Application o Infrastructure, Mediator, repositorios, persistencia, dominio de negocio ni proyectos de pruebas.

## Estructura propuesta

Mantener los cuatro proyectos y añadir sólo dos proyectos de pruebas:

```text
src/
├─ AulaPedidos.Domain/
│  └─ (vacío de modelo hasta que exista dominio real)
├─ AulaPedidos.Application/
│  ├─ DependencyInjection.cs
│  └─ Abstractions/Persistence/IRepository.cs
├─ AulaPedidos.Infrastructure/
│  └─ DependencyInjection.cs
└─ AulaPedidos.Api/
   ├─ Program.cs
   └─ AulaPedidos.Api.http
tests/
├─ AulaPedidos.ArchitectureTests/
│  └─ LayerDependencyTests.cs
└─ AulaPedidos.Api.IntegrationTests/
   ├─ HealthEndpointTests.cs
   ├─ OpenApiEndpointTests.cs
   └─ DependencyInjectionTests.cs
```

Los nombres de archivos de prueba son orientativos; importa separar arquitectura de integración, no crear más proyectos o carpetas que los necesarios.

## Dependencias propuestas

### Entre proyectos

```text
Domain                         → ninguna capa
Application                    → Domain
Infrastructure                 → Application
Api                            → Application + Infrastructure
ArchitectureTests              → los cuatro proyectos de producción
Api.IntegrationTests           → Api (y sólo referencias adicionales si una prueba las necesita)
```

Se recomienda retirar la referencia directa `Infrastructure → Domain` mientras Infrastructure no use tipos de dominio. Si aparece una implementación técnica que realmente los necesite, podrá recuperarse sin invertir dependencias.

### Paquetes y framework

- **Application:** referencia al paquete `Mediator` de martinothamar, en una versión comprobada como compatible con .NET 10. El registro debe quedar encapsulado en `AddApplication`. No usar `MediatR`, no crear `IMediator` propio y no añadir handlers de muestra.
- **Application:** si no llega de forma suficiente mediante Mediator, referencia explícita a `Microsoft.Extensions.DependencyInjection.Abstractions` para declarar la extensión sobre `IServiceCollection`.
- **Infrastructure:** `Microsoft.Extensions.DependencyInjection.Abstractions` y `Microsoft.Extensions.Configuration.Abstractions` para `AddInfrastructure(IServiceCollection, IConfiguration)`, evitando una referencia amplia e innecesaria a ASP.NET Core.
- **Api:** mantener `Microsoft.AspNetCore.OpenApi`. Health Checks y `MapHealthChecks` forman parte del framework compartido de ASP.NET Core; no hace falta introducir un proveedor de salud externo.
- **Pruebas:** usar **xUnit** en ambos proyectos, con `xunit`, `xunit.runner.visualstudio` y `Microsoft.NET.Test.Sdk`. El proyecto de integración requiere además `Microsoft.AspNetCore.Mvc.Testing`. Para las reglas simples de referencias no se necesita inicialmente una biblioteca adicional de arquitectura: pueden inspeccionarse las referencias de ensamblado desde pruebas `[Fact]` de xUnit.

No se fija desde este análisis una versión de `Mediator` ni de nuevos paquetes sin restauración o catálogo central que permita verificarla. Deben alinearse con .NET 10 y, donde corresponda, con la línea 10.0.x ya usada por OpenAPI.

## Contratos y composición

### `IRepository<T>`

Debe vivir en Application, por ejemplo en `AulaPedidos.Application.Abstractions.Persistence`, sin referencias a EF Core, HTTP o Infrastructure. Al no existir todavía entidad, identidad, agregado, consultas ni unidad de trabajo, no conviene imponer `GetById`, `IQueryable`, especificaciones, paginación, `SaveChanges` o CRUD completo.

Propuesta mínima:

```csharp
public interface IRepository<T> where T : class
{
	Task AddAsync(T entity, CancellationToken cancellationToken = default);
	Task RemoveAsync(T entity, CancellationToken cancellationToken = default);
}
```

Esta forma satisface el punto de extensión solicitado sin decidir un tipo de identificador ni exponer detalles de almacenamiento. Las operaciones de lectura y actualización deben añadirse cuando un caso de uso real defina su semántica. No debe existir implementación ni registro abierto de `IRepository<>` en BASE-001. Si se prefiere evitar incluso la semántica prematura de altas y bajas, una interfaz temporal sin miembros sería más conservadora, pero aportaría poco valor operativo; por ello se recomienda el contrato anterior como compromiso mínimo.

### `AddApplication`

Una extensión pública `AddApplication(this IServiceCollection services)` debe:

1. validar `services` frente a `null`;
2. registrar Mediator mediante la API generada/oficial de la versión elegida y limitar el escaneo al ensamblado de Application;
3. devolver el mismo `IServiceCollection` para composición fluida.

Aunque aún no haya handlers, el registro prepara la infraestructura solicitada. Debe validarse durante la implementación la firma exacta de `AddMediator` correspondiente a la versión restaurada, sin inventar un adaptador propio.

### `AddInfrastructure`

Una extensión pública `AddInfrastructure(this IServiceCollection services, IConfiguration configuration)` debe validar argumentos y devolver `services`. En BASE-001 no debe leer cadenas de conexión ni registrar EF Core, repositorios, opciones ficticias o servicios de prueba. `configuration` se mantiene porque forma parte del contrato acordado para el futuro punto de composición.

### Composición de Api

`Program.cs` debe seguir siendo una Minimal API y realizar, en orden conceptual:

1. `AddApplication()` y `AddInfrastructure(builder.Configuration)`;
2. `AddOpenApi()` y `AddHealthChecks()`;
3. construir la aplicación;
4. mapear OpenAPI sólo cuando `app.Environment.IsDevelopment()`;
5. mapear `MapHealthChecks("/health")` en todos los entornos;
6. ejecutar la aplicación.

Eliminar `/health/live`. Añadir `public partial class Program;` al final permite usar `WebApplicationFactory<Program>` sin cambiar el modelo de hospedaje ni introducir una clase Startup.

## Archivos por cambiar en una implementación posterior

| Archivo | Cambio mínimo |
|---|---|
| `CursoNETIA.slnx` | Agregar los dos proyectos bajo una carpeta de solución `tests`. |
| `src/AulaPedidos.Application/AulaPedidos.Application.csproj` | Referenciar Mediator y las abstracciones DI necesarias. |
| `src/AulaPedidos.Application/DependencyInjection.cs` | Crear `AddApplication` y registrar Mediator. |
| `src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs` | Declarar el contrato genérico mínimo. |
| `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj` | Quitar la referencia directa no usada a Domain y añadir abstracciones DI/configuración necesarias. |
| `src/AulaPedidos.Infrastructure/DependencyInjection.cs` | Crear `AddInfrastructure`. |
| `src/AulaPedidos.Api/Program.cs` | Componer capas, registrar Health Checks, sustituir la ruta de salud y hacer accesible `Program` a pruebas. |
| `src/AulaPedidos.Api/AulaPedidos.Api.http` | Sustituir la petición inexistente por `/health` y, opcionalmente, el documento OpenAPI de Development. |
| `tests/AulaPedidos.ArchitectureTests/*` | Crear proyecto y pruebas de fronteras y exclusiones. |
| `tests/AulaPedidos.Api.IntegrationTests/*` | Crear proyecto y pruebas de composición, salud y OpenAPI por entorno. |

No se debe modificar Domain ni materializar ahora el ADR 002.

## Justificación y límites

- La propuesta conserva las cuatro capas y hace explícita la composición sin introducir casos de uso inexistentes.
- Quitar la referencia directa innecesaria de Infrastructure a Domain refuerza las fronteras actuales; no prohíbe recuperarla cuando haya una necesidad concreta.
- El repositorio queda en Application porque expresa una necesidad de los futuros casos de uso; las implementaciones técnicas pertenecerán a Infrastructure.
- Mediator pertenece a la composición de Application y Api sólo inicia esa composición. Así Api no conoce handlers ni detalles internos.
- Health Checks sustituye correctamente un endpoint que sólo simulaba salud.
- Dos proyectos de pruebas separan reglas estáticas de arquitectura y comportamiento hospedado de Api sin mezclar responsabilidades.
- No se proponen EF Core, base de datos, repositorio en memoria, outbox, worker, autenticación, entidades, handlers ficticios, controllers ni patrones adicionales.

## Aceptación sugerida

1. La solución mantiene los cuatro proyectos de producción y añade exactamente los dos proyectos de pruebas propuestos, ambos implementados con xUnit y todos en `net10.0`.
2. Las referencias observables respetan `Domain ← Application ← Infrastructure`, con Api como composición; Domain no adquiere dependencias técnicas.
3. Api puede invocar `AddApplication` y `AddInfrastructure` y construir el proveedor con configuración mínima.
4. Application usa el paquete `Mediator` de martinothamar; no hay referencia a `MediatR`, mediador propio ni handlers ficticios.
5. `IRepository<T>` está en Application, es agnóstico del almacenamiento y no tiene implementación ni registro en DI.
6. `GET /health` responde satisfactoriamente mediante ASP.NET Core Health Checks y `/health/live` deja de estar mapeado.
7. El documento OpenAPI está disponible en Development y devuelve no encontrado en un entorno no Development.
8. No aparecen EF Core, `DbContext`, proveedores, cadenas de conexión, entidades, reglas, datos de ejemplo, outbox ni workers.
9. El archivo `.http` sólo representa endpoints existentes en esta base.
10. La implementación posterior supera `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx` sin errores ni advertencias nuevas atribuibles al cambio.

## Comprobaciones realizadas y pendientes

Realizadas mediante lectura, sin ejecutar comandos:

- comprobación de proyectos y referencias declaradas en la solución y los `.csproj`;
- comprobación del registro condicional actual de OpenAPI;
- confirmación de que la salud actual es manual y usa una ruta distinta;
- confirmación de la desalineación del archivo `.http`;
- comprobación de que el ADR de persistencia queda fuera de este alcance.

Pendientes para el Implementador y la validación posterior:

- resolver y confirmar una versión de `Mediator` compatible con .NET 10 y su firma real de registro;
- restaurar paquetes, compilar y ejecutar todas las pruebas;
- comprobar en ejecución `/health`, `/health/live` y `/openapi/v1.json` en Development y fuera de Development;
- inspeccionar el estado resultante para asegurar que no se introdujeron dependencias o capacidades fuera de alcance.

## Supuestos

- El expediente BASE-001 es la fuente vigente de alcance pese a que el paquete común mencionado en las instrucciones no está disponible.
- No existen otros archivos de código relevantes no visibles en las rutas consultadas; si aparecen durante la implementación, deben conservarse salvo conflicto directo con este alcance.
- La ruta estándar del documento OpenAPI se mantiene como `/openapi/v1.json` mientras no se configure otro nombre de documento.

## Siguiente paso

El Coordinador debe leer este informe junto con el informe independiente de Pruebas y preparar la síntesis. Sólo tras la decisión humana debería el Implementador aplicar los cambios y ejecutar las verificaciones indicadas.