# Producto-003 — CRUD de productos

## Objetivo
Implementar CRUD de productos con ID `Guid`, SKU, nombre y precio, incluyendo comandos, consultas, endpoints, repositorios y pruebas. Usar nombres de código en inglés, .NET 10 y Minimal APIs.

Este expediente prepara el trabajo: no acredita implementación, pruebas ejecutadas ni aprobación humana de reglas adicionales. Modalidad: selección manual de roles en Visual Studio. No requiere commit, diff, consultas previas ni congelación.

## Contexto consultado
- `.github/copilot-instructions.md`, aportado al chat.
- `docs/multiagente-copilot.md`.
- `CursoNETIA.slnx`.
- `src/AulaPedidos.Api/Program.cs`.
- `src/AulaPedidos.Application/AulaPedidos.Application.csproj`.
- `src/AulaPedidos.Application/Abstractions/Persistence/IRepository.cs`.
- `src/AulaPedidos.Infrastructure/AulaPedidos.Infrastructure.csproj`.
- `src/AulaPedidos.Infrastructure/DependencyInjection.cs`.
- `src/AulaPedidos.Infrastructure/Persistence/AppDbContext.cs`.
- `src/AulaPedidos.Infrastructure/Persistence/Repository.cs`.
- `src/AulaPedidos.Infrastructure/Persistence/UnitOfWork.cs`.
- `tests/AulaPedidos.Api.IntegrationTests/AulaPedidos.Api.IntegrationTests.csproj`.
- `tests/AulaPedidos.Infrastructure.Tests/AulaPedidos.Infrastructure.Tests.csproj`.

Observado: Application usa Mediator.Abstractions 3.0.2 y Api registra Mediator scoped. Existe EF Core SQLite, IRepository<TEntity>, Repository<TEntity> y UnitOfWork. Las consultas del repositorio son AsNoTracking. AppDbContext no declara entidades y Program.cs no declara endpoints de productos. Hay proyectos xUnit de infraestructura, arquitectura e integración API; no se han leído sus casos existentes.

Limitaciones: no se pudo leer `docs/paquetes/BASE-001-paquete-comun.md`. Antes del traslado, la consulta de `docs/expedientes/Producto-003.md` no devolvió un archivo existente. No se ejecutaron comandos ni se inspeccionó exhaustivamente el repositorio. El Implementador debe revisar convenciones, pruebas y mecanismo de actualización del esquema antes de editar.

## Alcance solicitado
1. **Domain:** Product con Id (Guid), Sku, Name y Price (decimal). Reglas independientes de EF y HTTP.
2. **Application:** CreateProductCommand, UpdateProductCommand, DeleteProductCommand, GetProductByIdQuery y GetProductsQuery; handlers y DTO de salida. Usar Mediator de martinothamar, no MediatR ni un mediador propio.
3. **Infrastructure:** persistencia SQLite real y mapeo de Product. Reutilizar IRepository<Product>, Repository<> e IUnitOfWork; añadir un repositorio específico únicamente por necesidad demostrada. No eliminar datos ni recrear bases compartidas para actualizar el esquema.
4. **Api:** grupo Minimal APIs `/products`, delegación a Mediator y propagación de CancellationToken.
5. **Pruebas:** dominio, handlers, repositorio y endpoints. Reutilizar xUnit y proyectos existentes; añadir proyectos de pruebas sólo cuando sea necesario para respetar capas y convenciones.

## Propuestas, no decisiones humanas
- Generar Id en el servidor y conservarlo durante la actualización.
- Sku y Name obligatorios, rechazando sólo espacios y eliminando espacios exteriores.
- Price decimal no negativo; aceptar cero. No introducir moneda, IVA ni redondeos sin requisito.
- Proponer SKU único con comparación coherente y protección frente a concurrencia en SQLite. **La unicidad y la sensibilidad a mayúsculas no están decididas**: aclararlas si cambian materialmente la implementación, sin presentarlas como requisitos aprobados.
- No inventar límites de longitud ni máximo de precio. Justificar cualquier restricción técnica.
- Actualización completa mediante PUT; listado simple con orden estable. Paginación, filtros y consulta por SKU quedan fuera salvo necesidad demostrada.
- Errores HTTP consistentes, preferiblemente ProblemDetails, sin exponer excepciones técnicas ni datos sensibles.

## Contrato HTTP propuesto
| Operación | Endpoint | Éxito | Errores |
|---|---|---|---|
| Crear | POST /products | 201, DTO y Location | 400; 409 si se decide unicidad y hay duplicado |
| Listar | GET /products | 200, lista | Según contrato documentado |
| Consultar | GET /products/{id} | 200 | 404 inexistente |
| Actualizar | PUT /products/{id} | 200, DTO actualizado | 400; 404; 409 condicionado a unicidad |
| Eliminar | DELETE /products/{id} | 204 | 404 inexistente |

Documentar y probar el comportamiento de GUID mal formado conforme al enrutamiento elegido.

## Casos de aceptación propuestos
- Crear devuelve Guid no vacío y Location; una consulta posterior recupera los datos persistidos.
- Listado vacío devuelve 200 y []; con datos usa un orden documentado.
- Actualizar conserva Id y persiste SKU, nombre y precio.
- Eliminar persiste la eliminación y la consulta posterior devuelve 404.
- Consultar, actualizar y eliminar un Id inexistente devuelve 404.
- SKU/nombre vacíos o con sólo espacios y precio negativo se rechazan sin guardar cambios; cero se acepta conforme a la propuesta.
- Repositorio y UnitOfWork se comprueban con SQLite aislado, incluyendo consultas AsNoTracking y actualización/eliminación.
- Si se aprueba unicidad: probar duplicados en alta y edición, conservación del SKU propio y colisión concurrente; normalización e índice deben concordar.
- Integración HTTP con handlers reales y base aislada, sin tocar almacenamiento compartido.
- Preservar pruebas de arquitectura: Domain sin EF/HTTP; Application sin Infrastructure.
- Propagar CancellationToken en I/O. Compilar no demuestra por sí solo cobertura funcional.

## Pasos y prompts copiables
### 1. Implementador — ruta directa
> Implementa Producto-003 leyendo docs/expedientes/Producto-003.md y .github/copilot-instructions.md. Crea CRUD de Product con Id Guid, Sku, Name y Price decimal; comandos Create/Update/Delete y consultas GetById/GetAll con handlers de Mediator de martinothamar; endpoints POST/GET/GET por Id/PUT/DELETE bajo /products; persistencia real EF Core SQLite, repositorios y pruebas. Revisa código y tests pertinentes antes de editar. Reutiliza IRepository<Product>, Repository<> e IUnitOfWork; no dupliques infraestructura. Conserva capas, .NET 10 y CursoNETIA.slnx. Distingue requisitos de propuestas y pregunta sólo por reglas materialmente ambiguas. Trabaja en pasos pequeños: dominio/mapeo, casos de uso/persistencia, endpoints y pruebas. No elimines datos ni recrees bases compartidas. Ejecuta dotnet build CursoNETIA.slnx y dotnet test CursoNETIA.slnx si tus herramientas lo permiten. Comunica resultados reales y pendientes; no declares aprobadas propuestas ni ejecutadas comprobaciones sin evidencia.

### 2. Revisor y Pruebas — tras implementar, chats separados
> Revisor: revisa la implementación real de Producto-003 frente a docs/expedientes/Producto-003.md y las instrucciones del proyecto. No modifiques código ni ejecutes comandos. Evalúa capas, Mediator, repositorio/UnitOfWork, AsNoTracking, validaciones, errores HTTP y seguridad del esquema. Entrega hallazgos comprobables con referencias y pendientes, sin inventar resultados.

> Pruebas: evalúa Producto-003 frente a docs/expedientes/Producto-003.md, los tests reales y las salidas aportadas. No modifiques código, tests ni configuración. Guarda docs/informes/Producto-003/2B-pruebas.md, comprobando antes su existencia y objetivo y conservando decisiones humanas y notas ajenas. Incluye objetivo, rol, archivos consultados, cobertura positiva/negativa/de borde, evidencia real, propuestas, supuestos, comprobaciones pendientes y siguiente paso. No declares ejecutadas pruebas sin evidencia.

### Alternativa opcional: consultas 2A/2B antes de implementar
No son prerrequisitos para empezar. Cada rol consulta requisitos y proyecto sin leer el informe del otro.

> Arquitecto: analiza el CRUD Producto-003 usando docs/expedientes/Producto-003.md y código pertinente, sin leer el informe de Pruebas. No implementes ni ejecutes comandos. Propón dominio, comandos/consultas de Mediator, repositorio, persistencia SQLite y endpoints. Señala reglas sin decidir. Guarda docs/informes/Producto-003/2A-arquitectura.md con objetivo, rol, archivos realmente consultados, conclusiones, propuestas, supuestos, comprobaciones y siguiente paso. Comprueba si existe y corresponde al mismo objetivo; conserva decisiones humanas y notas ajenas. Si pertenece a otro objetivo, conserva el original y usa carpeta con sufijo descriptivo, comunicando la ruta.

> Pruebas: analiza requisitos CRUD de Producto-003 usando docs/expedientes/Producto-003.md y archivos pertinentes, sin leer el informe de Arquitecto. No implementes tests ni ejecutes comandos. Define casos positivos, negativos y de borde para dominio, handlers, repositorio y HTTP, distinguiendo requisitos de propuestas. Guarda docs/informes/Producto-003/2B-pruebas.md con objetivo, rol, fuentes reales, conclusiones, propuestas, supuestos, comprobaciones pendientes y siguiente paso. Comprueba existencia y objetivo; conserva decisiones humanas y notas ajenas. Si pertenece a otro objetivo, conserva el original y usa carpeta con sufijo descriptivo, comunicando la ruta.

### 3. Coordinador — sólo si se utiliza 2A/2B
> Sintetiza Producto-003 leyendo docs/informes/Producto-003/2A-arquitectura.md y docs/informes/Producto-003/2B-pruebas.md. Guarda docs/informes/Producto-003/3-sintesis.md con objetivo, fuentes, acuerdos, diferencias, alcance recomendado, casos de aceptación, pendientes y prompt autocontenido para el Implementador. Distingue propuesta de decisión humana. No sobrescribas informes originales; comprueba existencia y objetivo de la síntesis y conserva decisiones humanas y notas ajenas. Si pertenece a otro objetivo, usa carpeta con sufijo descriptivo. Si falta un informe o falla su lectura, registra la limitación y sintetiza sólo lo disponible. Comunica rutas leídas y guardada. No implementes código ni ejecutes comandos.

## Pendientes y cierre
Resolver reglas de SKU cuando sean necesarias, inspeccionar tests y mecanismo de esquema, implementar y verificar. No se han aportado informes 2A/2B ni realizado síntesis. La aceptación del resultado corresponde al humano y requiere evidencia real.