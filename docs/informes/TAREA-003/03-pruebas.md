# TAREA-003 — Pruebas (fase inicial)

- **ID:** TAREA-003 (alias Producto-003)
- **Objetivo:** CRUD de productos
- **Rol:** Pruebas
- **Fecha:** 2026-10-13

## Archivos consultados
- `docs/expedientes/Producto-003.md`
- `src/AulaPedidos.Api/Program.cs` (no declara endpoints `/products`)
- `docs/informes/README.md`, `docs/informes/indice.md`, `docs/github-pr.md`

No se leyeron los tests existentes. No se ejecutó ningún comando.

## Casos propuestos (diseño; ninguno ejecutado)

| Requisito | Caso | Entrada | Resultado esperado | Defecto que detecta | Comprobación |
|---|---|---|---|---|---|
| Crear | POST válido | Sku, Name, Price | 201, Guid no vacío, Location; GET posterior coincide | No persiste o Location erróneo | Pendiente |
| Validación | Sku/Name vacío o solo espacios; Price < 0 | POST o PUT | 400 ProblemDetails, nada guardado | Validación ausente | Pendiente |
| Precio cero | Price = 0 | POST | 201 (propuesta, no decidido) | Rechazo indebido | Pendiente |
| Listar | Sin datos y con datos | GET /products | 200 `[]`; con datos, orden estable documentado | Orden inestable o error en vacío | Pendiente |
| 404 | Id inexistente | GET, PUT, DELETE | 404 | Excepción o 200/204 erróneo | Pendiente |
| Actualizar | PUT válido | PUT /products/{id} | 200, Id conservado, datos persistidos | Id regenerado, cambios no guardados | Pendiente |
| Eliminar | DELETE existente | DELETE y luego GET | 204 y luego 404 | Borrado no persistido | Pendiente |
| Ruta | GUID mal formado | GET /products/abc | 404 con `{id:guid}` (documentar) | Comportamiento no definido | Pendiente |
| Repositorio | SQLite aislado | Alta, actualización, borrado, consultas | Datos persistidos; consultas AsNoTracking | Tracking indebido, base compartida | Pendiente |
| Unicidad (no decidida) | Sku duplicado | POST o PUT | 409; el propio Sku se conserva; colisión concurrente controlada | Duplicados, falso conflicto | Pendiente |
| Transversal | CancellationToken y arquitectura | Handlers y endpoints | Token propagado; Domain sin EF/HTTP; Application sin Infrastructure | Capas violadas | Pendiente |

## Clasificación de la información
- **Análisis del código:** `Program.cs` no tiene endpoints de productos.
- **Evidencia ejecutada por otros:** ninguna disponible.
- **Verificaciones pendientes:** todas las de la tabla, `dotnet build CursoNETIA.slnx` y `dotnet test CursoNETIA.slnx`.

## Discrepancia
El comentario del Arquitecto en el issue habla de `Description` y `AlternateDescription`; el expediente define Sku, Name y Price. Estos casos usan el expediente. El Coordinador debe aclarar el modelo y la unicidad de Sku.

## Publicación en GitHub
- Repositorio: FTEMORES/CursoNETIA
- Issue: #2 — https://github.com/FTEMORES/CursoNETIA/issues/2
- Fase/marcador: `pruebas-inicial` / `<!-- aulapedidos:TAREA-003:pruebas-inicial -->`
- Comentario: id 6157667834 — https://github.com/FTEMORES/CursoNETIA/issues/2#issuecomment-6157667834
- Fecha: 2026-10-13
- Estado: publicado

## Siguiente paso
Aclarar modelo y unicidad; el Implementador cubre los casos y ejecuta build/test; después, evaluación posterior en una sección separada.
