# Flujo multiagente de AulaPedidos

Cada tarea guarda los entregables de sus agentes en una sola carpeta: `docs/informes/TAREA-NNN/`. El [contrato de informes](informes/README.md) define los nombres, la asignación del consecutivo y las reglas de actualización. El [índice de tareas](informes/indice.md) relaciona cada identificador con su objetivo y sus antecedentes. Estas reglas sustituyen las rutas y condiciones de guardado de versiones anteriores del flujo.

## Comenzar o continuar una tarea

Trabaja directamente con la petición y los archivos disponibles. No se exige expediente, commit, diff, consultas previas, síntesis ni otra aprobación de rutina. Una petición definida de implementación puede ir directamente al Implementador.

Antes de guardar, resuelve la tarea por el identificador, la ruta, el contexto actual o un alias inequívoco del índice. No elijas la última tarea por defecto. Para un objetivo nuevo, registra el siguiente consecutivo conforme al contrato, considerando los identificadores ya registrados y las carpetas existentes; no pidas al alumno que invente el nombre. El primer rol que atienda directamente una tarea nueva puede hacer ese registro mínimo.

Conserva la misma carpeta mientras continúe el objetivo. Los alias históricos como `BASE-001`, `BASE-002` o `Producto-003` se resuelven mediante el índice. Sus documentos anteriores permanecen donde están como antecedentes; los nuevos entregables se guardan en la carpeta `TAREA-NNN` registrada.

## Entregables por rol

Los nombres siguientes son fijos dentro de `docs/informes/TAREA-NNN/`. Sus prefijos ordenan los entregables; no representan rondas obligatorias.

| Rol | Archivo | Contenido |
|---|---|---|
| Ideas | `00-ideas.md` | Requisitos, supuestos, dudas y propuesta preparada para el siguiente rol |
| Coordinador | `01-coordinacion.md` | Objetivo, alcance, requisitos, plan y prompts; también sirve como expediente cuando se solicite |
| Arquitecto | `02-arquitectura.md` | Estructura, dependencias, alternativas útiles y cambios propuestos |
| Pruebas | `03-pruebas.md` | Casos, resultados esperados, evidencia disponible y comprobaciones pendientes |
| Coordinador | `04-sintesis.md` | Fuentes, acuerdos, diferencias y alcance recomendado |
| Implementador | `05-implementacion.md` | Cambios realizados, comprobaciones reales y pendientes |
| Revisor | `06-revision.md` | Hallazgos con ubicación, impacto, recomendación y límites |
| Coordinador | `07-cierre.md` | Evidencia disponible, pendientes y decisión humana cuando exista |

Si el usuario solicita un paquete común, el Coordinador guarda `01-paquete-comun.md` en esa misma carpeta. No lo exige para continuar ni crea otras variantes de nombre para planes, expedientes o síntesis.

Una nueva entrega del mismo rol y objetivo actualiza su archivo fijo conservando decisiones humanas, evidencia y notas ajenas. Pruebas distingue el análisis inicial de la evaluación posterior. No crea carpetas con sufijos libres ni renombra archivos según el título del tema. No modifica el informe de otro rol ni inventa su participación.

## Pasar trabajo entre chats

El Coordinador crea o reutiliza el issue de la tarea antes de repartir consultas, según la [guía de GitHub](github-pr.md). Entrega prompts que incluyan el identificador y la carpeta exacta, los archivos de entrada pertinentes, el destino fijo del rol y el repositorio, número y URL verificados del issue, o su estado pendiente. Los perfiles no activan por sí solos otros chats: en Visual Studio el alumno selecciona manualmente el rol. La [guía de configuración](harness-copilot.md) explica cómo comprobar los perfiles y sus herramientas.

Cuando sean útiles dos análisis separados, Arquitecto y Pruebas estudian la petición y los archivos del proyecto sin usar el informe ni los comentarios del otro rol para formar su propuesta. Cada uno concluye y guarda su propio informe antes de consultar comentarios para deduplicación; después publica su resumen en el issue compartido. Pruebas mantiene separados el diseño inicial y la evaluación posterior. La síntesis del Coordinador lee `02-arquitectura.md` y `03-pruebas.md` de la tarea identificada; no supone acceso a los otros chats. Si falta uno, sintetiza lo disponible y registra la ausencia. Una diferencia de objetivos o evidencia se declara, sin inventar consenso.

El Coordinador guarda `04-sintesis.md` y publica su propio comentario resumen de acuerdos, diferencias, alcance recomendado, aceptación y pendientes en ese mismo issue; actualizar el cuerpo del issue no sustituye el comentario. Los tres pasos conservan en sus informes el id/URL del comentario y estado real. Los reintentos no duplican el resumen ni editan comentarios ajenos. Si falta MCP o un issue inequívoco, guardan el resumen preparado y el pendiente, y continúan el análisis local. Una petición de sólo chat, sólo local o no publicar prevalece.

El Implementador usa la petición humana y los informes pertinentes, incluida `04-sintesis.md` cuando exista. Su ausencia no bloquea una petición directa. Después, Pruebas puede evaluar las salidas reales y Revisor los archivos o cambios disponibles. Dos informes no sustituyen comprobaciones ni garantizan corrección.

Al implementar código, el Implementador publica o actualiza el draft PR de la tarea mediante GitHub MCP según la [guía de PR y revisiones](github-pr.md), incluso sin issue, salvo petición de trabajo sólo local. Solicita Copilot code review y consulta los checks de seguridad, cobertura y calidad. El informe conserva la URL y la evidencia reales; si la conexión falta, deja la publicación pendiente.

## Guardado y alcance

Cada entrega sustantiva del rol se guarda conforme al contrato, sin confirmación adicional de guardado. Las preguntas intermedias y los saludos no crean tareas ni informes. Si el usuario pide expresamente sólo una respuesta en el chat, no guardar u otro destino, se respeta esa instrucción sin cambiar la convención para futuras tareas. Al terminar, comunica las rutas realmente leídas y guardadas. Si falta lectura o edición, declara la limitación y entrega el contenido con su destino previsto, sin afirmar que fue guardado.

La edición local de Ideas, Arquitecto, Pruebas y Revisor se limita a su informe. El Coordinador guarda los documentos de coordinación que le corresponden. Pueden añadir o reparar la inscripción de su propia tarea en el índice, conservando las demás filas. Arquitecto, Pruebas y la síntesis publican además sus propios resúmenes en el issue conforme a la guía. Estos roles no editan código ni tests ni ejecutan comandos. El Implementador realiza el cambio técnico solicitado y las comprobaciones proporcionales, además de su informe.

No edites simultáneamente un mismo archivo. La herramienta de edición no impone por sí sola el aislamiento de rutas: comprueba el destino, conserva cambios ajenos y revisa el resultado. Distingue siempre propuestas, decisiones humanas, comandos ejecutados y comprobaciones pendientes. Guardar una propuesta no equivale a aprobarla ni a aceptar el cambio.
