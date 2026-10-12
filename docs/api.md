# Contrato HTTP de referencia

Base local: `http://localhost:5080`. Contrato v1 en rutas `/api/v1`. La solución usa Minimal APIs de ASP.NET Core, organizadas en `Endpoints/ProductsEndpoints.cs` y `Endpoints/OrdersEndpoints.cs` mediante `MapGroup`. OpenAPI se publica en Development en `/openapi/v1.json`, con Swagger UI en `/swagger`. Generar el token con DevTools siguiendo [seguridad](seguridad.md).

| Método y ruta | Entrada | Acceso | Éxito |
|---|---|---|---|
| GET `/api/v1/products?page=1&pageSize=20` | Paginación | Autenticado | 200 página de productos |
| GET `/api/v1/products/{id}` | GUID | Autenticado | 200 ProductDto |
| POST `/api/v1/products` | sku, name, price | Admin + catalog.write | 201 + Location |
| PUT `/api/v1/products/{id}` | sku, name, price, version | Admin + catalog.write | 200 actualizado |
| DELETE `/api/v1/products/{id}?version={guid}` | Versión actual | Admin + catalog.write | 204 |
| GET `/api/v1/orders?page=1&pageSize=20&status=Submitted|Cancelled` | Paginación; `status` opcional | orders.read; sólo propios | 200 página |
| GET `/api/v1/orders/{id}` | GUID | orders.read; dueño | 200 OrderDto |
| POST `/api/v1/orders` | items: productId, quantity | orders.read + orders.write | 201 + Location |
| POST `/api/v1/orders/{id}/cancel` | version | orders.read + orders.write; dueño | 200 actualizado |
| GET `/health/live` | Sin body | Anónimo | 200 proceso vivo |
| GET `/health/ready` | Sin body | Anónimo | 200 healthy;503 al fallar DB |

Los permisos de pedidos se combinan con `RequireAuthorization`: `orders.read` en el grupo y `orders.write` adicional en creación/cancelación. Admin no evita la regla de dueño. El sujeto se obtiene de `sub` validado; no se acepta CustomerId ni precio de línea como fuente confiable desde el body. Los estados se serializan como `Submitted`/`Cancelled`. En `GET /api/v1/orders`, `status` es opcional y acepta sólo esos dos valores; la propiedad sobre el pedido sigue siendo obligatoria aunque se aplique el filtro.

## Reglas y errores

Los DTOs de entrada viven en `Endpoints/ProductInputs.cs` y `Endpoints/OrderInputs.cs`. `AddValidation()` aplica sus DataAnnotations; en records posicionales se anotan las propiedades con `property:`. `ConfigureHttpJsonOptions` conserva la serialización de enums como texto. Los handlers devuelven `TypedResults` y `ResultExtensions` transforma errores de aplicación a ProblemDetails. Las rutas de detalle tienen nombres para que `CreatedAtRoute` genere `Location` en los POST. El rate limiting se aplica al grupo `/api/v1`.

Referencia de plataforma: [Minimal APIs de ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-10.0).

El binding de JSON, parámetros y cuerpos requeridos devuelve 400 sin lanzar una excepción al manejador global (`ThrowOnBadRequest = false`), también en Development. La validación de DTOs devuelve un objeto `errors`; los errores de negocio conservan `code`. La personalización de ProblemDetails añade `traceId`.

SKU: 3..32 ASCII letras/números/guiones, normalizado en mayúsculas; nombre:3..120; precio efectivo0.01..1,000,000 con dos decimales. Pedido:1..50 líneas con productos distintos; cantidad1..100; dueño1..100 sin espacios extremos, comparación ordinal sensible a mayúsculas. Página1..1,000,000 y tamaño1..100. La API no administra existencias: un producto tiene precio y estado lógico, no stock.

400 corresponde a entrada inválida —incluido un `status` distinto de `Submitted` o `Cancelled`—, 401 a identidad ausente/inválida, 403 a permiso o dueño no autorizado, 404 a recurso ausente, 409 a conflicto de estado/versión o unicidad y 429 al límite de solicitudes. Fallos inesperados producen 500 sanitizado. Las respuestas de error usan ProblemDetails según el origen/pipeline; revisar sus extensiones de correlación en el contrato real. Constraints de ruta pueden producir 404 antes de llegar a validación de DTO.

Version es un GUID de concurrencia optimista, no un número incremental. Un cliente conserva la versión leída y la envía al modificar. Si hay409 debe recargar, mostrar conflicto y decidir; no reemplazar ciegamente con la nueva versión. Soft delete oculta el producto de consultas normales pero conserva historia y reserva el SKU según la referencia.

## Ejemplos de payload

```json
{"sku":"ITEM-001","name":"Licencia de servicio","price":1500.00}
```

```json
{"items":[{"productId":"GUID_REAL_DEVUELTO_POR_PRODUCTO","quantity":2}]}
```

El segundo bloque es una plantilla descriptiva: sustituir por un GUID real antes de enviar. No enviar marcadores literales. Para modificación/cancelación usar `version` real de la última lectura. El comando `dotnet run --project tools/AulaPedidos.DevTools -- smoke` crea identificadores válidos y ejercita estos contratos sin requerir inventar valores.

## Versionado y evolución

v1 está implementada mediante ruta explícita. No existe v2 ni negociación automática. Añadir campos opcionales compatibles exige tests de contrato; eliminar o cambiar un significado requiere una estrategia versionada y un periodo de transición. El filtro opcional `status` conserva el comportamiento anterior cuando se omite. `TotalCount` se calcula después de filtrar por cliente y estado, y antes de aplicar la paginación.

## Verificación y evidencia

Validar las actualizaciones de producto con entrada válida, inválida y versión obsoleta. Para los pedidos propios, comprobar `status=Submitted`, `status=Cancelled`, un valor inválido (400), el comportamiento sin filtro y `TotalCount` antes de paginar. El [protocolo común](multiagente-copilot.md) registra la decisión y la evidencia de cada cambio significativo.

La evidencia de una verificación incluye método/ruta, entrada sanitizada, estado HTTP esperado y observado, OpenAPI y el estado exacto del código. Una capacidad planificada no debe documentarse como disponible hasta que esté implementada y probada.
