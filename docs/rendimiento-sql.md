# Medición de consultas y planes en SQL Server

Este procedimiento produce evidencia de lecturas y del plan real antes y después de un índice. Usa datos sintéticos en una tabla temporal de la sesión; no modifica tablas de AulaPedidos. Después contrasta el experimento con la consulta real del repositorio. Un resultado favorable aquí no equivale a un benchmark de la aplicación completa.

## Preparar conexión y captura

1. Ejecutar `dotnet run --project tools/AulaPedidos.DevTools -- setup` y, desde la raíz del repositorio, `docker compose up -d sqlserver`. Esperar que `docker compose ps` muestre SQL Server saludable. Docker Desktop debe ejecutar contenedores Linux.
2. Abrir SQL Server Management Studio en una estación de trabajo local. Conectar a **`localhost,14333`**, autenticación SQL Server, usuario `sa`, usando la clave local generada en `.local/secrets.json` (`SqlPassword`). Introducirla sin proyectar ni copiarla a Copilot. La opción de confiar en el certificado del servidor se permite aquí sólo para este contenedor local. En un servidor corporativo se usan identidad, permisos y certificados aprobados.
3. Abrir **Nueva consulta**. Mantener la misma pestaña y conexión durante toda la medición: la tabla `#PedidosBenchmark` pertenece a esa sesión y desaparece al cerrarla.
4. Ejecutar el bloque de preparación que sigue. Luego activar **Consulta > Incluir plan de ejecución real** (`Ctrl+M`) antes de ejecutar las consultas de medición. El plan real aparece después de ejecutar; el plan estimado no contiene las mismas mediciones. [Documentación de planes reales](https://learn.microsoft.com/en-us/sql/relational-databases/performance/display-an-actual-execution-plan?view=sql-server-ver17).

## Preparar 50,000 pedidos sintéticos

Ejecutar el bloque una sola vez en una pestaña nueva. Si se necesita empezar nuevamente, abrir otra pestaña; no borrar datos de la aplicación.

```sql
SET NOCOUNT ON;

CREATE TABLE #PedidosBenchmark
(
    Id uniqueidentifier NOT NULL PRIMARY KEY,
    CustomerId nvarchar(100) COLLATE Latin1_General_100_BIN2 NOT NULL,
    CreatedAt datetimeoffset NOT NULL,
    Total decimal(18,2) NOT NULL,
    Detalle nvarchar(200) NOT NULL
);

-- Cinco dígitos generan suficientes números sin depender de tablas de usuarios.
;WITH Digito AS
(
    SELECT n FROM (VALUES (0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) d(n)
), Numeros AS
(
    SELECT a.n + 10*b.n + 100*c.n + 1000*d.n + 10000*e.n AS n
    FROM Digito a CROSS JOIN Digito b CROSS JOIN Digito c
    CROSS JOIN Digito d CROSS JOIN Digito e
)
INSERT INTO #PedidosBenchmark (Id, CustomerId, CreatedAt, Total, Detalle)
SELECT NEWID(),
       CONCAT(N'cliente-', n % 500),
       DATEADD(SECOND, n, CONVERT(datetimeoffset, '2026-01-01T00:00:00+00:00')),
       CONVERT(decimal(18,2), 10 + (n % 10000) / 100.0),
       REPLICATE(N'x', 150)
FROM Numeros
WHERE n < 50000;

SELECT COUNT(*) AS FilasTotales,
       COUNT(DISTINCT CustomerId) AS Clientes
FROM #PedidosBenchmark;
```

Resultado esperado: **50,000 filas y 500 clientes**. `Id` tiene índice de clave primaria; todavía no existe uno para el filtro por cliente. Los identificadores son aleatorios, pero el número de filas y la distribución por cliente son constantes.

## Obtener la línea base

Con plan real activado, ejecutar este bloque tres veces seleccionándolo en SSMS. Guardar la segunda y tercera ejecución en una tabla de evidencia. No incluir el tiempo de generar datos ni el de crear el índice. No limpiar cachés del servidor con comandos administrativos.

```sql
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

SELECT Id, CustomerId, CreatedAt, Total
FROM #PedidosBenchmark
WHERE CustomerId = N'cliente-42'
ORDER BY CreatedAt DESC, Id
OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY
OPTION (RECOMPILE);

SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;
```

Abrir **Mensajes** y registrar lecturas lógicas, CPU y tiempo transcurrido de ejecución. Las lecturas lógicas cuentan páginas consultadas en memoria; no son el número de filas devueltas. El plan permite comparar filas leídas/devueltas, ordenamientos y accesos elegidos por el optimizador. [Significado de STATISTICS IO](https://learn.microsoft.com/en-us/sql/t-sql/statements/set-statistics-io-transact-sql?view=sql-server-ver17).

Interpretación: devolver veinte filas no implica que el motor haya leído únicamente veinte. El plan y las estadísticas muestran el trabajo necesario para encontrarlas y ordenarlas.

## Agregar un índice y medir la misma consulta

```sql
CREATE INDEX IX_PedidosBenchmark_Cliente_Fecha_Id
ON #PedidosBenchmark (CustomerId, CreatedAt DESC, Id)
INCLUDE (Total);
```

Volver a ejecutar **exactamente** el bloque de medición anterior tres veces y guardar la segunda/tercera ejecución. Registrar también si cambió el operador de acceso y si desapareció el ordenamiento. Se espera menor trabajo para este filtro selectivo, pero el resultado a entregar es la observación real, no una cifra prefijada.

| Escenario | Filas retornadas | Lecturas lógicas | CPU/tiempo | Operadores relevantes |
|---|---:|---:|---|---|
| Sin índice por cliente, ejecución 2 | | | | |
| Sin índice por cliente, ejecución 3 | | | | |
| Con índice, ejecución 2 | | | | |
| Con índice, ejecución 3 | | | | |

Interpretación: el índice puede aprovechar el filtro y el orden de este caso, pero ocupa espacio y cuesta mantenerlo en `INSERT` y `UPDATE`. No debe agregarse a todas las tablas ni trasladarse sin medir la consulta real.

`OPTION (RECOMPILE)` reduce la influencia de un plan previo en esta comparación controlada; no es una recomendación de incorporarlo a cada consulta de producción. El plan puede cambiar por versión de SQL Server, estadísticas, recursos o distribución. Una lectura física cero puede ser resultado de la caché de páginas, no ausencia de trabajo.

## Contrastar con AulaPedidos

1. Detener primero API o receptor locales que ocupen 5080/5099. Iniciar el Compose completo, ejecutar `dotnet run --project tools/AulaPedidos.DevTools -- smoke` contra su API y abrir una pestaña SSMS en la base **AulaPedidos**. El comando crea datos de ejemplo para la inspección.
2. Consultar únicamente los datos sintéticos existentes. El siguiente SQL reproduce el filtro y la página principal; la query EF completa también carga las líneas y realiza una consulta de conteo.

```sql
USE AulaPedidos;
GO
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

DECLARE @CustomerId nvarchar(100) =
(
    SELECT TOP (1) CustomerId
    FROM dbo.Orders
    ORDER BY CreatedAt DESC, Id
);
SELECT Id, CustomerId, CreatedAt, Total, Status
FROM dbo.Orders
WHERE CustomerId = @CustomerId
ORDER BY CreatedAt DESC, Id
OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY;

SELECT name, collation_name
FROM sys.columns
WHERE object_id = OBJECT_ID(N'dbo.Orders') AND name = N'CustomerId';

SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;
```

3. Abrir `src/AulaPedidos.Infrastructure/Persistence/Repositories.cs`, `ListByCustomerAsync`, y comparar filtro, orden, `Include`, paginación y `AsNoTracking`. Abrir `EntityConfigurations.cs` para identificar el índice existente. El índice temporal incluyó columnas distintas: no afirmar que ése es ya el índice de la aplicación.
4. Para inspeccionar SQL generado, colocar un breakpoint después de construir `query` en ese repositorio y evaluar `query.ToQueryString()` en el depurador de Visual Studio. Esto muestra ese punto del LINQ; añadir mentalmente o inspeccionar también el `Include`/`OrderBy`/`Skip`/`Take` aplicado después. La consulta que se mide debe corresponder a la operación completa que se quiere optimizar.
5. Revisar `CreateOrderHandler`: `GetByIdsAsync` carga en una consulta los productos de todas las líneas. Comparar el método del repositorio con un `GetByIdAsync` dentro de un bucle. No confundir este acceso de catálogo con la consulta de listado de pedidos.

**Registro de evidencia:** conservar una captura sanitizada del plan antes y después, una tabla de mediciones, la hipótesis confirmada o descartada y una limitación. Cuando ya no se necesiten, detener API y receptor con `docker compose stop api notifications`; SQL Server y los volúmenes se conservan. Si SQL Server no está disponible, ejecutar las pruebas relacionales y leer el SQL generado como contingencia; dejar la medición como pendiente. No registrar cifras de rendimiento inventadas ni afirmar que se ejecutó SQL Server cuando no existe esa evidencia.

## Criterios de validez

La carga completa de `Order.Items` desde un contexto nuevo es una comprobación independiente de integridad de persistencia. El [protocolo común](multiagente-copilot.md) aplica a los cambios que modifiquen esta consulta o sus índices.

Conservar plan, lecturas, tiempo y condiciones equivalentes antes y después en el registro del cambio. Distinguir la tabla temporal de la consulta real. Si falta SQL Server, la medición queda pendiente: una hipótesis o un acuerdo de agentes no reemplaza datos observados.
