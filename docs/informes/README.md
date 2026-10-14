# Informes de los agentes

Esta es la convención vigente de guardado para todos los perfiles de `.github/agents/` y sus prompts. Cada tarea tiene una carpeta fija en `docs/informes/TAREA-NNN/`. El objetivo se escribe dentro de los documentos y en el [índice](indice.md); no forma parte del nombre de la carpeta.

## Identificar la tarea y asignar el consecutivo

1. Lee `docs/informes/indice.md`. Busca el ID o la ruta indicados por el usuario, en los adjuntos o en el contexto de la tarea de este chat. Los alias históricos del índice identifican la misma tarea. Comprueba que el objetivo coincide; un número escrito en un ejemplo no identifica la petición actual.
2. Si es un seguimiento del mismo objetivo, conserva su ID. Si el chat empieza sin ID, reutiliza una tarea sólo cuando la coincidencia de objetivo y alcance sea inequívoca. Nunca elijas la última carpeta por fecha o por tener el número mayor. Ante dos tareas posibles, pide sólo la aclaración necesaria y continúa el análisis que no dependa de ella.
3. Si el objetivo es nuevo, consulta el índice y las carpetas `TAREA-*` de `docs/informes/`. Asigna **máximo número ocupado + 1**, considerando ambos inventarios, incluidos registros reservados y tareas terminadas. No rellenes huecos ni recicles números. Usa al menos tres dígitos: `TAREA-004`, `TAREA-005`, …, `TAREA-999`, `TAREA-1000`. No reinicies por rol, categoría, fecha o sesión.
4. Registra ID, objetivo, alias si los hay y carpeta antes de entregar prompts a otros roles. El Coordinador suele hacer esta inscripción; en una petición directa puede hacerla el primer rol que atienda la tarea. No se necesita pedir al usuario un nombre ni un expediente previo. Si una carpeta existe sin fila, lee su contexto y recupera su inscripción sin reasignar su número a otro objetivo.
5. Relee el índice y el destino justo antes de guardar. Si otro chat reservó ese número para un objetivo distinto, conserva su trabajo y calcula de nuevo el siguiente. La reserva mediante archivos no es atómica: al iniciar tareas en paralelo, un solo chat asigna los IDs antes de repartir el trabajo. Los demás roles reutilizan el ID recibido y no editan simultáneamente el índice.

Si falta acceso al índice o al listado de carpetas, no adivines el siguiente número. Continúa el análisis y entrega el contenido en el chat, indicando que la asignación y el guardado están pendientes. Si el ID y la carpeta ya están confirmados en el contexto, puedes preparar la ruta exacta, pero no declarar un guardado que no realizaste.

## Nombres fijos

Todas las rutas de esta tabla son relativas a `docs/informes/TAREA-NNN/`:

| Rol | Archivo | Contenido |
|---|---|---|
| Ideas | `00-ideas.md` | Requisitos, supuestos, preguntas resueltas, opciones y borrador de decisión |
| Coordinador | `01-coordinacion.md` | Objetivo, alcance, aceptación, plan y prompts; también es el expediente cuando se solicita uno |
| Arquitecto | `02-arquitectura.md` | Análisis de estructura, dependencias y propuesta; corresponde a la consulta 2A |
| Pruebas | `03-pruebas.md` | Casos y esperados; evaluación posterior en una sección separada; corresponde a la consulta 2B |
| Coordinador | `04-sintesis.md` | Fuentes realmente leídas, acuerdos, diferencias y siguiente paso |
| Implementador | `05-implementacion.md` | Cambios, archivos afectados, comandos y resultados reales, pendientes |
| Revisor | `06-revision.md` | Hallazgos con ubicación e impacto, recomendaciones y límites |
| Coordinador | `07-cierre.md` | Evidencias, pendientes y decisión humana real, si existe |
| Coordinador, sólo si se pide | `01-paquete-comun.md` | Contexto compartido para consultas separadas, sin informes de otros roles |

Los prefijos ordenan los documentos; **no obligan a ejecutar todos los pasos** ni son números de versión. Crea sólo los archivos necesarios para la petición. Una solicitud directa al Implementador puede producir únicamente `05-implementacion.md`; una solicitud de síntesis al Coordinador produce `04-sintesis.md`. No crees archivos vacíos para completar la tabla.

Cada entrega sustantiva se guarda en su archivo, también fuera del flujo multiagente. Las preguntas intermedias y los saludos no crean tareas ni informes. Si Ideas prepara requisitos, alternativas o un borrador de ADR, los conserva en `00-ideas.md`; las preguntas todavía abiertas quedan identificadas como tales.

## Continuaciones y límites de escritura

- Un seguimiento actualiza el mismo archivo. Conserva decisiones humanas, notas ajenas y evidencia original relevante; añade una sección de seguimiento con fecha e identifica qué conclusión reemplaza. No presentes una propuesta anterior como vigente si ya cambió. Pruebas conserva por separado el diseño inicial y la evaluación posterior.
- Un objetivo distinto recibe otra tarea. No uses `-final`, `-v2`, nombres descriptivos, mayúsculas alternativas ni carpetas fuera de `docs/informes/` para resolver colisiones. Si una ruta corresponde a otro objetivo, no sobrescribas el documento.
- La edición local de los roles lectores y el Coordinador se limita a sus archivos de la tabla. Pueden añadir o reparar la inscripción de su propia tarea en el índice, conservando las demás filas. No modifican informes de otros roles. El Implementador también puede editar código, tests y documentación técnica dentro del alcance solicitado. La publicación de resúmenes propios en el issue se rige por la [guía de GitHub](../github-pr.md) y no cambia estos destinos locales.
- Esta convención regula los entregables de los agentes. Los archivos del producto (código, tests, documentación técnica o un ADR solicitado expresamente) conservan sus ubicaciones del repositorio y los permisos del rol que los edita. No crees una segunda copia del informe en esas ubicaciones.
- Guardar no necesita otra confirmación. Una instrucción explícita del usuario de sólo chat, no guardar o usar otro destino prevalece; registra y comunica la excepción concreta, sin convertirla en el destino predeterminado de futuras tareas.
- Si no está disponible la edición, entrega el contenido listo para guardar y la ruta resuelta, indicando **no guardado**. Si no se asignó un ID, informa que también queda pendiente esa asignación.

## Contenido y traspaso entre chats

Encabeza cada informe con ID, objetivo, rol y fecha. Incluye archivos realmente consultados, conclusiones, propuestas separadas de decisiones humanas, supuestos, comprobaciones realizadas o pendientes y siguiente paso. No copies conversaciones completas ni secretos, y no inventes resultados.

Todo prompt que se entregue al alumno para otro rol debe incluir el ID, la carpeta canónica y las rutas exactas que ese rol debe leer y escribir. Añade repositorio, número y URL verificados del issue de la tarea, o el motivo por el que esa referencia está pendiente. Resuelve `TAREA-NNN` antes de entregar el prompt; no pidas al siguiente rol que invente una carpeta. No asumas acceso a otros chats.

En 2A/2B, Arquitecto y Pruebas reciben requisitos y código, sin el informe contrario, y concluyen su análisis antes de consultar comentarios para deduplicar su resumen en GitHub. Los comentarios del otro rol tampoco son entrada de su propuesta independiente. El Coordinador combina después `02-arquitectura.md` y `03-pruebas.md` en `04-sintesis.md`. Si falta un informe, registra la ausencia y trabaja con lo disponible. Al usar antecedentes históricos, comprueba que corresponden al objetivo y al estado actual; identifica las rutas realmente leídas.

Arquitecto, Pruebas y la síntesis del Coordinador publican además un resumen en el mismo issue según la [guía de GitHub](../github-pr.md), salvo restricción expresa del usuario. Cada uno registra en su propio informe la sección `Publicación en GitHub`: referencia del issue, fase/marcador, id/URL real del comentario, fecha y estado, o motivo pendiente y resumen preparado. Pruebas distingue comentarios de diseño inicial y evaluación posterior. Esta sección no sustituye el informe completo ni acredita una publicación por sí sola.

Al terminar, comunica el ID y las rutas realmente guardadas. Una lectura posterior debe confirmar que el archivo quedó en la carpeta correcta antes de afirmar que se guardó.

## Informes anteriores a esta convención

El índice reserva `TAREA-001`, `TAREA-002` y `TAREA-003` para los objetivos históricos `BASE-001`, `BASE-002` y `Producto-003`. Sus archivos existentes se conservan sin mover, renombrar ni corregir retroactivamente. Los enlaces del índice permiten encontrarlos.

Si se retoma uno de esos objetivos, los nuevos entregables se guardan en su carpeta `TAREA-NNN` registrada y enlazan los antecedentes necesarios. Las rutas y prompts antiguos dentro de informes, expedientes o ADR no sustituyen esta convención. `docs/expedientes/`, `docs/Base-002/` y las carpetas con nombres históricos ya no son destinos predeterminados de nuevos informes.
