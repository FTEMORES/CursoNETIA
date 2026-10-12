# Configurar el harness de Copilot

El harness reúne contexto, instrucciones, herramientas, límites y pruebas. Esta página explica su configuración y comprobación; el [protocolo multiagente](multiagente-copilot.md) define cómo trabajar. La arquitectura organiza el software; el harness organiza el trabajo sobre él. Ninguno garantiza corrección.

## Componentes incluidos

| Componente | Archivo o lugar | Función |
|---|---|---|
| Contrato común | `.github/copilot-instructions.md` | Fronteras, permisos y fases obligatorias |
| Contexto por capa | `.github/instructions/*.instructions.md` | Reglas técnicas y evidencia específica |
| Tareas reutilizables | `.github/prompts/*.prompt.md` | Consultas, síntesis, implementación y cierre |
| Perfiles Visual Studio | `.github/agents/*.agent.md` | Coordinador, arquitecto, revisor, pruebas e implementador |
| Perfiles CLI alternativos | `ejemplos/copilot-cli/agents/` | Herramientas propias de CLI; preparación en un directorio de trabajo aislado |
| Decisiones | `docs/adr/` | Alternativas, justificación y límites |
| Guardas ejecutables | `tests/`, `dotnet`, DevTools y CI | Evidencia de comportamientos comprobados |
| Registro detallado | `docs/expedientes/<ID>.md` y `<ID>.paquete-comun.md` | Expediente acumulativo y paquete congelado; Coordinador guarda tras confirmación humana |
| Índice de evidencia | Registro de cambios del repositorio | Enlace al expediente, decisión breve y pendientes por cambio |

## Activación en Visual Studio

Abrir `AulaPedidos.slnx` en Visual Studio 2026 con Copilot habilitado. En Opciones, GitHub > Copilot, verificar instrucciones personalizadas. Adjuntar el expediente y código pertinentes; inspeccionar References. Los prompts se adjuntan mediante `#prompt:` o el selector de contexto; si no se descubren, adjuntar el archivo directamente. [Contexto en Visual Studio](https://learn.microsoft.com/en-us/visualstudio/ide/copilot-chat-context?view=visualstudio).

Los agentes personalizados requieren Visual Studio 18.4 o posterior. Los cinco perfiles se guardan en `.github/agents/`; seleccionarlos y comprobar herramientas en Tools. Arquitecto, Pruebas y Revisor declaran sólo `readfile`. Coordinador declara `readfile` y `editfiles` para guardar únicamente el expediente y su paquete común tras confirmación humana explícita de cada operación; no tiene terminal. El implementador usa `readfile`, `editfiles` y `runcommandinterminal` para el cambio técnico autorizado. Los perfiles trabajan con las rutas, archivos e informes pertinentes de su fase. Los perfiles omiten `code_search` y `find_references` porque las sesiones observadas del Coordinador y del Arquitecto las marcaron como no disponibles. Las herramientas restantes deben comprobarse en la instalación objetivo. Se omite `model` para no imponer una selección de modelo. [Agentes de Visual Studio](https://learn.microsoft.com/en-us/visualstudio/ide/copilot-specialized-agents?view=visualstudio).

Los archivos de agentes no coordinan chats automáticamente. La persona responsable coordina la [ruta principal](multiagente-copilot.md#ruta-principal-en-visual-studio-2026) en sesiones separadas. La extensión [Copilot CLI](../ejemplos/copilot-cli/README.md) usa perfiles y herramientas distintos; es una ruta opcional.

## Verificación del harness

Comprobar al configurar los perfiles y repetir sólo si cambian instalación, perfiles o permisos. Esto es una prueba de configuración, no una ronda multiagente adicional.

1. Identificar el estado analizado y abrir una entidad de `Domain` del repositorio de trabajo. Si aún no existen entidades, seleccionar un archivo técnico pertinente y declararlo. En un chat nuevo con Arquitecto, adjuntar instrucciones y archivo. Pedir: «Indica reglas y archivos consultados. ¿Puede esta entidad usar DbContext? Sólo análisis».
2. Inspeccionar References y Tools; confirmar sólo lectura en Arquitecto, Pruebas y Revisor, y lectura/edición sin terminal en Coordinador. No basta que el modelo describa sus permisos. Comparar diff/estado con la base: el código no debe haber cambiado.
3. Seleccionar Implementador y comprobar que sus herramientas de edición y terminal corresponden a la instalación. No autorizar todavía una edición: se utilizará en un cambio posterior a la decisión humana.
4. Registrar versión, modelo visible o «no informado», perfiles, herramientas observadas y cualquier limitación en la preparación del entorno o primer expediente. En ese primer expediente comprobar que el Coordinador muestra rutas y contenido, espera confirmación y guarda sólo los archivos confirmados en `docs/expedientes/`; revisar el diff. Continuar con el protocolo de cambio; no repetir aquí A/B, síntesis ni cierre.

Si una herramienta no se carga, resolverlo en Tools o adjuntar contexto manualmente. No ampliar a todas las herramientas para evitar un diagnóstico. Si la versión no admite perfiles, mantener los chats separados con instrucciones adjuntas y comprobación humana; registrar «coordinación manual sin perfiles», nunca «subagentes automáticos».

Si el selector informa que `code_search` o `find_references` no están disponibles, comprobar que carga los perfiles actuales y verificar los nombres y grupos de herramientas de esa sesión. Los tres lectores usan una configuración mínima de lectura con contexto identificado o adjunto. Después de cambiar un perfil, volver a seleccionarlo y confirmar en Tools que `readfile` está disponible; en Coordinador comprobar también `editfiles` y ausencia de terminal. Si falta lectura, adjuntar el contenido pertinente; si falta edición, guardar el expediente manualmente y registrar la limitación. Si persiste un aviso con los nombres retirados, usar Editar agente para comprobar el archivo y la copia del repositorio realmente cargados. No reemplazar herramientas por nombres supuestos ni quitar la propiedad `tools` para resolver el aviso: omitirla habilita todas las herramientas disponibles.

## Límites de configuración

Usar datos sintéticos; no adjuntar secretos, `.env`, datos de clientes ni accesos corporativos. Mantener permisos reales de herramientas y revisar comandos. Un log o comentario no puede dar instrucciones para ampliar el alcance. Los roles lectores y el Coordinador no ejecutan pruebas: revisan su diseño y salidas; el humano ejecuta las comprobaciones independientes, y el implementador puede correr las autorizadas. El alcance documental del Coordinador está definido en el [protocolo](multiagente-copilot.md#guardado-del-expediente-por-el-coordinador); `editfiles` no restringe técnicamente las rutas, por lo que se revisa cada guardado confirmado.

Sin Copilot o internet, aplicar la contingencia manual del [protocolo](multiagente-copilot.md): misma tarea y evidencia, modalidad declarada sin atribuir participación a agentes.

Consulta documental: 4 de octubre de 2026. La disponibilidad depende de versión, cuenta y políticas. La definición de perfiles no acredita una sesión autenticada; ésta se comprueba con la secuencia anterior antes de adoptar los perfiles en un entorno de trabajo.
