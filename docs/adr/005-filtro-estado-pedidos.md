# ADR 005 Filtro de estado para pedidos propios

- Estado: aceptada.
- Contexto: la lista de pedidos propios requiere distinguir pedidos enviados de pedidos cancelados sin exponer pedidos de otros clientes.
- Decisión: `GET /api/v1/orders` acepta el parámetro opcional `status` con los valores `Submitted` o `Cancelled`. La autorización por propietario permanece obligatoria. Un valor distinto devuelve 400. `TotalCount` se calcula después de aplicar los filtros de cliente y estado, y antes de paginar.
- Alternativas: crear rutas separadas por estado duplica el contrato; filtrar sólo en el cliente transfiere datos no necesarios y no preserva una semántica de conteo consistente.
- Consecuencias: omitir `status` conserva el listado anterior; clientes deben tratar `TotalCount` como el tamaño del conjunto filtrado. Las pruebas deben cubrir ambos estados, un valor inválido, aislamiento por cliente y paginación.
- Evidencia: [contrato HTTP](../api.md) y [solicitudes manuales](../../requests/AulaPedidos.http).
