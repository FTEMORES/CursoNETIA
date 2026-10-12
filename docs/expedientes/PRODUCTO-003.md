# PRODUCTO-003: CRUD de productos

## Objetivo
Añadir el caso de uso completo de Producto (alta, consulta, actualización y baja) sobre la base vigente (BASE-002: Repository Pattern + Unit of Work con EF Core/SQLite y Mediator de martinothamar).
Se introduce la primera entidad de negocio en Domain.

## Alcance

### Domain
- Entidad `Producto` con:
  - `Id` (`Guid`).
  - `Sku` (string, identificador de producto, único por negocio; validar no vacío).
  - `Nombre` (string, validar no vacío).
  - `Precio` (tipo numérico apropiado, por ejemplo `decimal`; validar no negativo).
- Sin dependencias de EF Core, HTTP ni Mediator en Domain.
- Decidir si implementa `IAggregateRoot` (tipo ya existente en `AulaPedidos.Domain.Abstractions`, usado por el contrato `IRepository<T>` no persistente que convive en Application).

### Application
- Comandos (vía Mediator de martinothamar, sin MediatR ni mediador propio):
  - `CrearProductoCommand` (Sku, Nombre, Precio) → `Guid` o DTO de resultado.
  - `ActualizarProductoCommand` (Id, Nombre, Precio) → confirma éxito o entidad no encontrada.
  - `EliminarProductoCommand` (Id) → confirma éxito o entidad no encontrada.
- Consultas:
  - `ObtenerProductoPorIdQuery` (Id) → DTO de producto o nulo/no encontrado.
  - `ListarProductosQuery` (con paginación, reutilizando `GetPagedAsync` de `IRepository<T>`) → lista de DTOs.
- Validación de entrada en los handlers (Sku/Nombre no vacíos, Precio no negativo) antes de tocar el repositorio.
- Usar `IRepository<Producto>` e `IUnitOfWork` ya definidos en `AulaPedidos.Application.Abstractions.Persistence` (BASE-002). No crear un segundo contrato de repositorio paralelo.
- Definir cómo se comunica "no encontrado" (excepción de dominio/aplicación, resultado nulo o un tipo de resultado) — decisión a confirmar por el Arquitecto.

### Infrastructure
- Mapear `Producto` en `AulaPedidosDbContext` (DbSet y configuración mínima: clave primaria `Id`, restricciones de longitud/unicidad de `Sku` si se decide aplicar a nivel de base).
- Reutilizar `EfRepository<T>`/`EfUnitOfWork` existentes; no se espera una implementación de repositorio específica de Producto salvo que el Arquitecto lo justifique.
- Migraciones o `EnsureCreated`: confirmar con la decisión ya pendiente de BASE-002.

### Api
- Minimal API endpoints bajo un grupo `/productos` (o el prefijo que decida el Arquitecto):
  - `POST /productos` → crea.
  - `GET /productos/{id}` → obtiene por id.
  - `GET /productos` → lista paginada.
  - `PUT /productos/{id}` → actualiza.
  - `DELETE /productos/{id}` → elimina.
- Los endpoints despachan a Mediator; no contienen lógica de negocio.
- Devolver códigos HTTP apropiados (201/200/204/404/400).

### Pruebas
- Pruebas de Application (validación de comandos/queries, posiblemente con un repositorio en memoria o fake si no se usa SQLite directamente).
- Pruebas de integración con SQLite en memoria (igual que BASE-002) para el CRUD completo de Producto contra `EfRepository<Producto>`.
- Pruebas de endpoints (si existe el proyecto de integración de Api) para las rutas anteriores.
- Actualizar `ArchitectureTests`/`CompositionTests` si las listas cerradas de tipos declarados por proyecto dejan de coincidir tras añadir `Producto` y los nuevos tipos.

## Fuera de alcance
- Otras entidades del dominio de pedidos (pedidos, clientes, etc.).
- Autorización/autenticación de los endpoints.
- Especificación de catálogo, variantes de producto o impuestos.

## Reglas de arquitectura
- Domain no referencia EF Core, HTTP ni Mediator.
- Application orquesta mediante comandos/queries y contratos de persistencia ya existentes.
- Infrastructure implementa persistencia; Api compone y expone HTTP.
- E/S asíncrona con `CancellationToken` en toda la cadena.

## Decisiones pendientes (humanas)
1. Si `Producto` implementa `IAggregateRoot` y qué implica eso en el repositorio usado (`IRepository<T>` de `Abstractions` vs. el de `Abstractions.Persistence`).
2. Cómo se representa "no encontrado" en Application (excepción, nulo, tipo de resultado).
3. Si `Sku` debe ser único y si esa regla se valida en Application, en Infrastructure (índice único) o en ambos.
4. Prefijo y forma exacta de las rutas HTTP y los contratos de petición/respuesta (DTOs).
5. Si se añade un proyecto de pruebas nuevo o se reutilizan los dos ya existentes en `CursoNETIA.slnx`.

## Aceptación
- `dotnet build CursoNETIA.slnx` sin errores.
- `dotnet test CursoNETIA.slnx` sin errores, incluyendo el CRUD de Producto sobre SQLite en memoria y los endpoints HTTP.
- Domain conserva ausencia de dependencias técnicas salvo la nueva entidad `Producto`.

## Limitaciones
- Este expediente se redactó sin ejecutar código ni crear pruebas; es la base para que Arquitecto, Pruebas e Implementador trabajen la tarea.
