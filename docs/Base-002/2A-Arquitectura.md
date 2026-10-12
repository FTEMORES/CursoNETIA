# BASE-002 — Diseño de arquitectura

## Objetivo

Diseñar la evolución de BASE-001 para incorporar Repository Pattern y Unit of Work, manteniendo las cuatro capas. Los contratos deben residir en Application y las implementaciones técnicas deben residir en Infrastructure mediante Entity Framework Core y SQLite. El repositorio debe ser reutilizable y permitir consultas filtradas, ordenadas, con includes y paginación mediante predicados composibles sobre `IQueryable`.

## Rol

Arquitectura (paso 2A).

## Archivos realmente consultados

- `docs/expedientes/BASE-002.md`.
- `CursoNETIA.slnx`.
- `.github/copilot-instructions.md`.
- `src/AulaPedidos.Domain/AulaPedidos.Domain.csproj`.
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`.
- `src/AulaPedidos.Application/DependencyInjection.cs`.
- `src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs`.
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`.
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`.
- `src/AulaPedidos.Api/AulaPedidos.Api.csproj`.
- `src/AulaPedidos.Api/Program.cs`.
- `src/AulaPedidos.Api/appsettings.json`.
- `tests/AulaPedidos.ArchitectureTests/AulaPedidos.ArchitectureTests.csproj`.
- `tests/AulaPedidos.ArchitectureTests/ArchitectureTests.cs`.
- `tests/AulaPedidos.Api.IntegrationTests/AulaPedidos.Api.IntegrationTests.csproj`.

No se pudo leer `docs/paquetes/BASE-001-paquete-comun.md` porque no está disponible en la ruta indicada. El diseño se basa en el código actual y en el expediente proporcionado.

## Estructura actual y dependencias

La solución contiene cuatro proyectos productivos y dos proyectos de pruebas, todos orientados a `net10.0`:

```text
Domain        (sin referencias de proyecto ni paquetes)
	↑
Application   (Domain, Mediator.Abstractions, DI abstractions)
	↑
Infrastructure (Application, Configuration abstractions, DI abstractions)
	↑
Api           (Application, Infrastructure, Mediator.SourceGenerator)
```

Actualmente:

- Domain no declara tipos de producción.
- Application ya contiene `IRepository<T>` en `AulaPedidos.Application.Abstractions.Persistence`, con restricción `where T : class` y únicamente `AddAsync` y `RemoveAsync`.
- Application registra los adaptadores de Mediator.
- Infrastructure solo contiene `AddInfrastructure`; todavía no hay `DbContext`, repositorio EF Core ni Unit of Work.
- Api ya invoca `AddApplication`, `AddInfrastructure` y registra Mediator desde `Program.cs`.
- `appsettings.json` solo contiene logging y `AllowedHosts`; no hay cadena de conexión.
- Las pruebas de arquitectura actuales describen BASE-001: esperan un repositorio mínimo, prohíben paquetes de persistencia y esperan que Infrastructure solo declare `DependencyInjection`. Tendrán que evolucionar con BASE-002; no deben interpretarse como aceptación final de esta tarea.

## Diseño propuesto

### Domain

Sin cambios. BASE-002 no necesita crear una entidad base, una propiedad `Id` ni reglas de negocio. El repositorio genérico debe trabajar con `T` restringido a `class`, porque BASE-001 mantiene el dominio sin entidades.

### Application: contratos

Conservar `IRepository<T>` en `AulaPedidos.Application.Abstractions.Persistence` y ampliarlo con operaciones de consulta y modificación necesarias para BASE-002. Application puede usar `System.Linq`, `System.Linq.Expressions` y `CancellationToken`; no debe referenciar EF Core, SQLite ni `DbContext`.

La forma recomendada del contrato de consulta es:

```csharp
Task<List<T>> GetAsync(
	Expression<Func<T, bool>>? predicate = null,
	Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
	CancellationToken cancellationToken = default,
	params string[] includeStrings);

Task<List<T>> GetPagedAsync(
	int pageNumber,
	int pageSize,
	Expression<Func<T, bool>>? predicate = null,
	Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
	CancellationToken cancellationToken = default,
	params string[] includeStrings);
```

La firma solicitada se adapta para que `params string[] includeStrings` sea el último parámetro, requisito del lenguaje C#. El `CancellationToken` queda antes de `params` y conserva soporte para I/O cancelable. El orden propuesto de la consulta es:

1. Obtener el `IQueryable<T>` base.
2. Aplicar cada include.
3. Aplicar `predicate` si no es nulo.
4. Aplicar `orderBy` si no es nulo.
5. En el método paginado, aplicar `Skip((pageNumber - 1) * pageSize)` y `Take(pageSize)`.
6. Materializar de forma asíncrona en una lista.

La validación de `pageNumber` y `pageSize` debe producir `ArgumentOutOfRangeException` cuando sean menores que 1. Conviene validar antes de construir o ejecutar la consulta. Si no se proporciona `orderBy`, la paginación no tiene un orden determinista; se recomienda aceptar la llamada, pero documentar que el consumidor debe proporcionar orden cuando necesite páginas estables.

Para evitar ambigüedad entre sobrecargas y mantener un contrato pequeño, `GetAsync` y `GetPagedAsync` deben ser los métodos de lectura principales. `AddAsync`, `RemoveAsync` y una operación de actualización explícita solo deben incluirse si el caso de uso la necesita; con EF Core, `Update` normalmente no requiere I/O y puede ser una operación síncrona. Esta decisión debe cerrarse antes de implementar para no copiar un CRUD genérico innecesario.

### Unit of Work

Crear `IUnitOfWork` en el mismo espacio de nombres de persistencia de Application. El contrato mínimo recomendado es:

```csharp
Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
```

No debe exponer `DbContext`, `DbSet`, transacciones de EF Core ni tipos de Infrastructure. En esta base no se recomienda añadir transacciones explícitas: `SaveChangesAsync` cubre la unidad de persistencia de la operación y evita introducir una abstracción antes de existir un caso de uso que la requiera. Si posteriormente aparecen procesos de varios pasos con requisitos transaccionales explícitos, se puede añadir un contrato específico sin filtrar EF Core.

### Infrastructure: EF Core y SQLite

Añadir en `AulaPedidos.Infrastructure.csproj` los paquetes necesarios, al menos:

- `Microsoft.EntityFrameworkCore`.
- `Microsoft.EntityFrameworkCore.Sqlite`.
- La versión debe ser compatible con `net10.0` y mantenerse coherente con el resto de paquetes Microsoft del repositorio.

Crear en Infrastructure:

- Un `DbContext` técnico, por ejemplo `AulaPedidosDbContext`, derivado de `DbContext`.
- Un repositorio genérico EF Core que implemente `IRepository<T>`.
- Una implementación de `IUnitOfWork` delegada al contexto.

El repositorio debe obtener el conjunto mediante `Set<T>()`, construir la consulta sobre `IQueryable<T>` y materializarla con los métodos async de EF Core. Los includes de texto se aplican con `Include(string)` en Infrastructure, no en Application. No se debe devolver un `IQueryable` creado sobre un contexto cuyo ciclo de vida ya no pueda garantizarse; el contrato propuesto devuelve listas materializadas y conserva la composición dentro de la ejecución del repositorio.

Las operaciones de escritura deben usar el contexto y respetar el `CancellationToken` cuando exista I/O. `AddAsync` puede delegar en EF Core; `Remove` y, si se incluye, `Update`, solo modifican el estado y `SaveChangesAsync` confirma la unidad de trabajo.

### Registro y configuración

Ampliar `AddInfrastructure(IConfiguration configuration)` para:

1. Registrar el contexto con `AddDbContext<AulaPedidosDbContext>` y `UseSqlite`.
2. Registrar `IUnitOfWork` como scoped.
3. Registrar el repositorio genérico como `IRepository<>` → implementación EF genérica, también scoped.

La cadena debe leerse desde configuración, preferiblemente `ConnectionStrings:DefaultConnection`. Añadir solo configuración no sensible en `src/AulaPedidos.Api/appsettings.json`, por ejemplo una ruta de archivo SQLite relativa o configurable por entorno. No guardar credenciales ni secretos. No usar `EnsureCreated` automáticamente en producción y no introducir migraciones hasta decidirlas expresamente; para las pruebas se puede usar una conexión SQLite en memoria gestionada por el proyecto de pruebas.

`Program.cs` ya invoca `AddInfrastructure(builder.Configuration)`, por lo que no necesita una segunda ruta de registro. Solo debe cambiar si se requiere una configuración explícita adicional.

## Archivos previstos para cambiar

### Producción

- `src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs`: ampliar el contrato con consultas y, si se confirma, operaciones de actualización.
- `src/AulaPedidos.Application/Abstractions/Persistence/IUnitOfWork.cs`: nuevo contrato.
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`: paquetes EF Core y SQLite.
- `src/AulaPedidos.Infrastructure/Persistence/AulaPedidosDbContext.cs`: nuevo contexto.
- `src/AulaPedidos.Infrastructure/Persistence/Repository.cs`: nueva implementación genérica; el nombre final debe seguir las convenciones existentes.
- `src/AulaPedidos.Infrastructure/Persistence/UnitOfWork.cs`: nueva implementación.
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`: registro del contexto, repositorio y Unit of Work.
- `src/AulaPedidos.Api/appsettings.json`: cadena SQLite no sensible, si se decide configurar una ruta por defecto.

### Pruebas y arquitectura

- `tests/AulaPedidos.ArchitectureTests/ArchitectureTests.cs`: actualizar las expectativas de BASE-001 para permitir los contratos, implementaciones y paquetes EF Core autorizados, manteniendo las fronteras de capas.
- `tests/AulaPedidos.Api.IntegrationTests/AulaPedidos.Api.IntegrationTests.csproj`: añadir referencias o paquetes solo si las pruebas de integración necesitan inspeccionar la persistencia.
- Nuevas pruebas de repositorio, preferiblemente en un proyecto de pruebas de Infrastructure si se crea, o en el proyecto de integración existente si se mantiene la estructura actual.

No se propone modificar Domain ni crear endpoints o entidades de negocio en BASE-002.

## Dependencias y fronteras

- Domain sigue sin dependencias.
- Application depende de Domain y solo de abstracciones de plataforma para contratos (`Expression`, `IQueryable`, tareas y cancelación).
- Infrastructure depende de Application y contiene toda referencia a EF Core y SQLite.
- Api depende de Application e Infrastructure y realiza la composición mediante DI.
- No usar MediatR ni un mediador propio. Mediator de martinothamar permanece como dependencia independiente de esta persistencia.

## Riesgos y decisiones

1. **Contrato actual mínimo:** las pruebas vigentes exigen exactamente `AddAsync` y `RemoveAsync`; BASE-002 requiere cambiar esas expectativas de forma intencionada.
2. **Tipos de `IQueryable` en Application:** son adecuados para expresar predicados y orden, pero no deben exponer el contexto ni forzar EF Core en Application.
3. **Includes por cadena:** cumplen la firma solicitada, pero no tienen comprobación fuerte en compilación. No introducir un Specification Pattern en esta base.
4. **Total de páginas:** `GetPagedAsync` devolverá solo la página. Si un caso de uso necesita total, se recomienda un método separado o un resultado paginado explícito; no añadirlo sin requisito.
5. **Orden nulo:** se acepta para consultas generales, pero se recomienda exigir `orderBy` en los usos paginados que requieran estabilidad.
6. **Cancelación:** `CancellationToken` debe llegar hasta `ToListAsync` y `SaveChangesAsync`; no se deben usar `.Result` ni `.Wait()`.
7. **SQLite:** el proveedor es una dependencia de Infrastructure, nunca de Application o Domain.
8. **Migraciones:** quedan fuera del mínimo actual. Debe decidirse antes de automatizar creación o actualización de esquema.

## Aceptación propuesta

- La solución compila con `dotnet build CursoNETIA.slnx`.
- Las pruebas de arquitectura confirman las referencias permitidas y que EF Core/SQLite solo aparecen en Infrastructure y en los proyectos de prueba necesarios.
- Una prueba con SQLite verifica predicado, orden, includes y paginación con `Skip`/`Take`.
- Se verifican `AddAsync`, `RemoveAsync`, actualización si se incorpora y `SaveChangesAsync` mediante Unit of Work.
- La cancelación se propaga a las operaciones asíncronas.
- No se crean entidades ni lógica de negocio en Domain.
- No se exponen `DbContext` ni tipos EF Core desde Application.

## Comprobaciones realizadas y pendientes

Comprobado mediante lectura: estructura de la solución, referencias de proyectos, paquetes actuales, registro existente de DI, contrato actual de `IRepository<T>`, configuración actual de Api y expectativas de las pruebas de arquitectura.

Pendiente: compilación y ejecución de pruebas, porque esta revisión no ejecuta comandos. También queda pendiente confirmar el contenido de BASE-001, ausente en la ruta indicada, y cerrar con el responsable si se desea `Update`, total de elementos, transacciones explícitas y migraciones.

## Siguiente paso

Revisar este diseño junto con la estrategia de pruebas de BASE-002. Después, el Implementador puede aplicar los cambios mínimos, actualizar las pruebas de arquitectura afectadas y verificar la solución.