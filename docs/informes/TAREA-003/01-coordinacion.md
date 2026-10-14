# TAREA-003 — Coordinación: CRUD de productos

- ID: TAREA-003 (alias Producto-003) · Rol: Coordinador · Carpeta: `docs/informes/TAREA-003/`
- Issue verificado: `FTEMORES/CursoNetIA` #2 — https://github.com/FTEMORES/CursoNETIA/issues/2

## Objetivo
CRUD de `Product` con `Id` (Guid), `Price`, `Description` y `AlternateDescription`. Sustituye como propuesta vigente los campos Sku/Name/Price del expediente histórico `docs/expedientes/Producto-003.md` (antecedente); es decisión humana de esta petición.

## Contexto leído
`.github/copilot-instructions.md`, `docs/multiagente-copilot.md`, `docs/informes/README.md`, `docs/informes/indice.md`, `docs/github-pr.md`, `docs/expedientes/Producto-003.md`. No se leyó código ni se ejecutaron comandos.

## Alcance
Domain (`Product`), Application (Create/Update/Delete, GetById/GetAll, DTO, Mediator de martinothamar), Infrastructure (mapeo EF Core SQLite con `IRepository<Product>` e `IUnitOfWork`), Api (`/products`), pruebas. Fuera: paginación, filtros, SKU, moneda, IVA.

## Aceptación
Ver issue #2. Resumen: ciclo CRUD completo, 404 en Id inexistente, validación sin guardar, build y test correctos.

## Propuestas (no decisiones humanas)
- Id generado en servidor y conservado al actualizar.
- `Price` decimal ≥ 0; `Description` obligatoria; `AlternateDescription` opcional. Sin límites de longitud inventados.
- PUT completo; errores con ProblemDetails.

## Plan y prompts
Publicación en GitHub: issue creado y verificado (#2); sin comentarios aún.

### 1. Arquitecto (2A)
> Rol Arquitecto. Tarea TAREA-003, carpeta `docs/informes/TAREA-003/`. Issue: FTEMORES/CursoNetIA #2, https://github.com/FTEMORES/CursoNETIA/issues/2. Lee `docs/informes/TAREA-003/01-coordinacion.md` y el código existente (sin el informe de Pruebas). Analiza estructura y dependencias del CRUD de `Product` (Id Guid, Price, Description, AlternateDescription). Guarda en `docs/informes/TAREA-003/02-arquitectura.md` y publica tu resumen (fase `arquitectura`) en el issue #2.

### 2. Pruebas (2B)
> Rol Pruebas. Tarea TAREA-003, carpeta `docs/informes/TAREA-003/`. Issue: FTEMORES/CursoNetIA #2, https://github.com/FTEMORES/CursoNETIA/issues/2. Lee `docs/informes/TAREA-003/01-coordinacion.md` y los tests existentes (sin el informe del Arquitecto). Diseña casos y resultados esperados. Guarda en `docs/informes/TAREA-003/03-pruebas.md` y publica tu resumen (fase `pruebas-inicial`) en el issue #2.

### 3. Coordinador (síntesis)
> Rol Coordinador. TAREA-003, issue FTEMORES/CursoNetIA #2. Lee `docs/informes/TAREA-003/02-arquitectura.md` y `03-pruebas.md`; guarda `docs/informes/TAREA-003/04-sintesis.md` y publica el comentario fase `sintesis` en el issue #2.

### 4. Implementador (ruta directa posible)
> Rol Implementador. TAREA-003, carpeta `docs/informes/TAREA-003/`, issue FTEMORES/CursoNetIA #2. Lee `docs/informes/TAREA-003/01-coordinacion.md` (y `04-sintesis.md` si existe) y `.github/copilot-instructions.md`. Implementa el CRUD de `Product` (Id Guid, Price, Description, AlternateDescription) por capas, con Mediator de martinothamar, EF Core SQLite y pruebas. Compila y ejecuta tests; guarda `docs/informes/TAREA-003/05-implementacion.md` y publica el draft PR.

## Pendientes
Decisiones humanas: obligatoriedad de Description, opcionalidad de AlternateDescription, precio 0, longitudes.
