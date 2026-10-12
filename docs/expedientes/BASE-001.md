# BASE-001: base técnica de AulaPedidos

## Estado

- Tipo: alcance inicial.
- Estado de la decisión: propuesta pendiente de validación humana mediante la implementación y sus comprobaciones.
- Rol de este documento: delimitar el trabajo; no acredita que las capacidades estén implementadas.

## Objetivo

Establecer una base técnica mínima para AulaPedidos sobre .NET 10 y Minimal APIs, conservando `CursoNETIA.slnx` y la separación en Domain, Application, Infrastructure y Api. La base debe ofrecer composición explícita mediante `AddApplication` y `AddInfrastructure`, OpenAPI sólo en Development, Health Checks en `/health`, contratos para Repository Pattern y la biblioteca `Mediator` de martinothamar, además de proyectos de pruebas, sin introducir dominio ni persistencia reales.

## Estado base observado

Revisión documental realizada sin ejecutar comandos:

- `CursoNETIA.slnx` contiene los cuatro proyectos de producción bajo `src`: Api, Application, Domain e Infrastructure.
- Los cuatro proyectos apuntan a `net10.0`, con nullable e implicit usings habilitados.
- Domain no declara referencias a otras capas.
- Application referencia Domain.
- Infrastructure referencia Application y Domain.
- Api referencia Application e Infrastructure y usa `Microsoft.AspNetCore.OpenApi`.
- `Program.cs` registra OpenAPI y lo mapea dentro de `Development`.
- `Program.cs` expone actualmente `/health/live` mediante una Minimal API manual; no es un Health Check y no coincide con la ruta requerida `/health`.
- No se encontraron los proyectos de pruebas consultados ni aparecen proyectos de pruebas en la solución.
- No se observaron en los archivos consultados `AddApplication`, `AddInfrastructure`, contratos de repositorio, registro de Mediator, entidades, lógica de negocio ni configuración de base de datos.
- `AulaPedidos.Api.http` conserva una petición de plantilla a `/weatherforecast/` que no corresponde al estado actual de `Program.cs`.
- `docs/adr/002-persistencia-y-outbox.md` describe decisiones para una fase futura; BASE-001 no debe materializar SQLite, SQL Server, outbox ni workers.

## Fuentes consultadas

- `.github/copilot-instructions.md` (aportado como instrucción del espacio de trabajo).
- `docs/multiagente-copilot.md`.
- `docs/expedientes/README.md`.
- `CursoNETIA.slnx`.
- `src/AulaPedidos.Api/AulaPedidos.Api.csproj`.
- `src/AulaPedidos.Api/Program.cs`.
- `src/AulaPedidos.Api/AulaPedidos.Api.http`.
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`.
- `src/AulaPedidos.Domain/AulaPedidos.Domain.csproj`.
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`.
- `docs/adr/002-persistencia-y-outbox.md`.

No se encontró `docs/paquetes/BASE-001-paquete-comun.md`; por tanto, este alcance se apoya en la petición actual y en los archivos enumerados.

## Alcance incluido

### Estructura y dependencias

- Conservar `CursoNETIA.slnx`, los nombres actuales y `net10.0`.
- Mantener las cuatro capas y sus responsabilidades:
  - Domain: futuro modelo y reglas, sin dependencias de otras capas.
  - Application: casos de uso y contratos; depende sólo de Domain.
  - Infrastructure: futuras implementaciones técnicas; depende de Application y, cuando sea necesario, de Domain.
  - Api: composición y transporte mediante Minimal APIs; referencia Application e Infrastructure.
- Añadir a la solución los proyectos de pruebas bajo `tests` sin alterar las fronteras anteriores.

### Composición

- Crear `AddApplication(IServiceCollection)` en Application.
- Crear `AddInfrastructure(IServiceCollection, IConfiguration)` en Infrastructure. Se admite `IConfiguration` para preparar opciones futuras, pero BASE-001 no debe leer cadenas de conexión ni registrar proveedores de datos.
- Mantener `Program.cs` como raíz de composición y hacer que invoque ambas extensiones.
- Registrar la biblioteca NuGet `Mediator`, cuyo autor es martinothamar, a través de la composición de Application iniciada desde Api.
- No instalar ni usar `MediatR` y no crear un mediador propio.
- No añadir handlers ficticios únicamente para demostrar el registro.

### Minimal APIs, OpenAPI y salud

- Conservar el arranque como Minimal API, sin controllers.
- Mantener OpenAPI accesible únicamente cuando el entorno sea `Development`.
- Sustituir el endpoint manual de salud por ASP.NET Core Health Checks: registrar `AddHealthChecks()` y mapear `MapHealthChecks("/health")`.
- No añadir comprobaciones de base de datos, servicios externos ni dependencias inexistentes.
- Actualizar el archivo `.http` para que represente los endpoints realmente disponibles en esta base.

### Repository Pattern

- Declarar `IRepository<T>` como contrato genérico en Application, sin dependencia de EF Core ni de Infrastructure.
- Mantener el contrato mínimo y agnóstico de almacenamiento. No imponer una clase base, una interfaz de entidad, un tipo de identificador o semántica de consultas que todavía no estén definidos por el dominio.
- No crear implementaciones en memoria, colecciones estáticas, datos de ejemplo ni repositorios vacíos registrados en DI.
- Preparar en Infrastructure únicamente el punto de composición para futuras implementaciones. La primera implementación concreta queda fuera de BASE-001.

### Pruebas

- Crear al menos dos proyectos de pruebas y agregarlos a `CursoNETIA.slnx`:
  - pruebas de arquitectura para las dependencias entre capas y las exclusiones críticas;
  - pruebas de integración de Api para `/health` y la disponibilidad de OpenAPI según el entorno.
- Añadir una comprobación de composición que demuestre que `AddApplication` y `AddInfrastructure` pueden registrarse y construir el contenedor con la configuración mínima.
- Las pruebas no deben introducir entidades o casos de uso ficticios en los proyectos de producción.

## Fuera de alcance

- Entidades, agregados, value objects, reglas o cualquier lógica de negocio.
- Casos de uso, comandos, consultas o handlers de negocio.
- EF Core, `DbContext`, migraciones y cualquier proveedor o instancia de base de datos.
- Repositorios concretos, incluidos repositorios en memoria o simulaciones en producción.
- Seed, datos de ejemplo, outbox, workers, mensajería externa y adaptadores HTTP.
- Autenticación, autorización, usuarios y secretos.
- Controllers, UI, Docker y despliegue.
- MediatR o una implementación propia del patrón mediator.
- Implementar ahora las decisiones futuras descritas en el ADR 002.

## Criterios de aceptación

1. `CursoNETIA.slnx` sigue siendo la solución y contiene los cuatro proyectos de producción más los proyectos de pruebas acordados.
2. Todos los proyectos apuntan a .NET 10 y la solución compila sin advertencias o errores nuevos atribuibles a BASE-001.
3. Las referencias respetan Domain <- Application <- Infrastructure y la composición desde Api; Domain no adquiere dependencias técnicas.
4. Api llama a `AddApplication` y `AddInfrastructure` sin registrar servicios de negocio o persistencia inexistentes.
5. El paquete utilizado es `Mediator` de martinothamar; no existe referencia a `MediatR` ni un mediador propio.
6. `IRepository<T>` existe en Application y no depende de EF Core, HTTP o una implementación concreta.
7. No existe implementación registrada de `IRepository<T>` mientras no haya almacenamiento real.
8. `GET /health` devuelve una respuesta saludable mediante ASP.NET Core Health Checks.
9. `/health/live` deja de ser el contrato de salud de BASE-001.
10. OpenAPI está expuesto en Development y no está expuesto fuera de Development.
11. No se han añadido entidades, lógica de negocio, base de datos, autenticación, outbox ni repositorios ficticios.
12. Las pruebas automatizadas cubren fronteras de arquitectura, composición, salud y comportamiento de OpenAPI por entorno.
13. `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx` finalizan correctamente cuando el Implementador ejecute las comprobaciones.

## Casos de aceptación

| Caso | Preparación | Resultado esperado |
|---|---|---|
| Salud positiva | Api iniciada con configuración mínima | `GET /health` responde con estado HTTP satisfactorio y usa Health Checks. |
| Ruta anterior | Api iniciada | `/health/live` no se conserva como endpoint contractual. |
| OpenAPI en Development | Entorno `Development` | El documento OpenAPI está disponible. |
| OpenAPI fuera de Development | Entorno distinto de `Development` | El documento OpenAPI no está publicado. |
| Composición mínima | Contenedor configurado sin base de datos | `AddApplication` y `AddInfrastructure` completan el registro y el proveedor se construye. |
| Frontera de Domain | Inspección automatizada de referencias | Domain no referencia Application, Infrastructure ni Api. |
| Mediator correcto | Inspección de paquetes y registros | Se usa `Mediator` de martinothamar y no `MediatR`. |
| Persistencia ausente | Inspección de referencias y servicios | No hay EF Core, proveedores, `DbContext`, cadenas de conexión ni repositorios concretos. |
| Dominio ausente | Inspección de producción | No se introducen entidades, reglas ni handlers ficticios. |

## Riesgos y decisiones pendientes

- La forma exacta de las operaciones de `IRepository<T>` debe mantenerse mínima. Sin entidades ni estrategia de identificadores, añadir CRUD completo sería una decisión prematura. Es una propuesta técnica, no una decisión humana ya aprobada.
- Deben elegirse nombres concretos y framework de los proyectos de pruebas de acuerdo con las convenciones disponibles al implementar. La exigencia estable es separar arquitectura e integración y agregarlos a la solución.
- La versión de `Mediator` debe ser compatible con .NET 10 y resolverse al implementar; no se fija aquí una versión no verificada.
- No se ejecutaron compilación ni pruebas durante esta definición de alcance.

## Plan breve por rol

1. **Implementador:** aplicar únicamente este alcance en cambios pequeños, sin añadir dominio ni persistencia real.
2. **Pruebas:** revisar los criterios, ejecutar compilación y pruebas, y comunicar resultados reales y pendientes.
3. **Revisor:** comprobar fronteras, paquetes, ausencia de alcance extra y correspondencia entre pruebas y requisitos.
4. **Responsable humano:** aceptar, pedir cambios o dejar pendientes a partir de la evidencia aportada.

## Prompt autocontenido para el Implementador

> Implementa BASE-001 en `CursoNETIA.slnx` sobre .NET 10. Conserva las capas AulaPedidos.Domain, Application, Infrastructure y Api y sus dependencias. Añade `AddApplication` y `AddInfrastructure`; usa Minimal APIs; publica OpenAPI sólo en Development; sustituye `/health/live` por ASP.NET Core Health Checks en `/health`. Declara en Application un contrato mínimo `IRepository<T>` agnóstico de almacenamiento y no crees ni registres repositorios concretos o en memoria. Integra la biblioteca `Mediator` de martinothamar desde la composición iniciada por Api; no uses MediatR ni un mediador propio y no añadas handlers ficticios. Prepara Infrastructure sólo como punto de composición futura, sin EF Core, `DbContext`, proveedor, migraciones ni cadenas de conexión. Añade a la solución proyectos de pruebas para arquitectura e integración de Api y cubre composición, `/health`, OpenAPI por entorno, fronteras y exclusiones. Actualiza el archivo `.http`. No añadas entidades, lógica de negocio, autenticación, outbox, worker, Docker ni datos de ejemplo. Ejecuta `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx` e informa resultados reales, archivos modificados y cualquier pendiente.