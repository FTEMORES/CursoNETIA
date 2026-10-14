# Configurar los agentes de Copilot

Los perfiles, instrucciones y prompts del repositorio organizan el trabajo de los agentes. El [flujo multiagente](multiagente-copilot.md) describe su uso y el [contrato de informes](informes/README.md) fija dónde guarda cada rol. La configuración de archivos no acredita que las herramientas estén disponibles ni que se hayan ejecutado otros agentes.

## Archivos de configuración

| Componente | Ubicación | Función |
|---|---|---|
| Instrucciones comunes | `.github/copilot-instructions.md` | Alcance, arquitectura y reglas compartidas |
| Instrucciones por capa | `.github/instructions/` | Reglas técnicas de cada capa |
| Prompts | `.github/prompts/` | Tareas reutilizables que conservan el identificador y los destinos del contrato |
| Perfiles | `.github/agents/` | Ideas, Coordinador, Arquitecto, Pruebas, Implementador y Revisor |
| Contrato de informes | `docs/informes/README.md` | Carpeta por tarea y nombres fijos de los entregables |
| Índice de tareas | `docs/informes/indice.md` | Consecutivos, objetivos, alias y antecedentes |
| Entregables | `docs/informes/TAREA-NNN/` | Documentos de todos los roles para un mismo objetivo |

## Comprobar la carga en Visual Studio

1. Abre `CursoNETIA.slnx` en la copia del repositorio donde se trabajará. Comprueba que Copilot utiliza las instrucciones y el perfil de `.github/agents/` seleccionado. Adjunta la petición y los archivos pertinentes cuando no estén disponibles en el contexto.
2. Revisa las herramientas efectivamente disponibles en la sesión. Ideas, Arquitecto, Pruebas y Revisor necesitan lectura y edición para su informe; el Coordinador necesita lectura y edición para sus documentos. Estos roles no ejecutan comandos. El Implementador utiliza lectura, edición y terminal para el cambio solicitado. Las herramientas de GitHub que declare un perfil se comprueban por separado, sin dar su conexión por hecha.
3. En la primera tarea, comprueba que el agente resuelve el identificador mediante el índice y guarda en `docs/informes/TAREA-NNN/` con el nombre fijo de su rol. No debe pedir confirmación de guardado ni inventar otra carpeta. Una continuación conserva el identificador y el contenido previo pertinente.
4. Inspecciona los cambios locales: un rol de análisis sólo debe editar su informe y, si hace falta, añadir o reparar su propia inscripción en el índice. El Coordinador conserva sus documentos de proceso y el Implementador los cambios técnicos solicitados. Comprueba que no se modificaron informes ajenos.
5. Para Arquitecto, Pruebas y Coordinador comprueba también GitHub MCP con lectura de issues/comentarios y publicación de comentarios; la actualización de comentarios se usa si está expuesta. El Coordinador necesita además crear/actualizar el issue. Cada perfil declara una lista de herramientas GitHub para su rol; esas listas no limitan los argumentos de cada operación. En una tarea real, verifica que los tres pasos comentan en el mismo issue, que cada informe registra su enlace y que repetir el mismo resultado no crea duplicados. No publiques comentarios de prueba en issues ajenos al trabajo.

Los perfiles no coordinan chats automáticamente. El alumno cambia de rol y pasa un prompt con el identificador, la carpeta exacta, los archivos de entrada necesarios y el repositorio, número y URL del issue confirmado, o su estado pendiente. El Coordinador resuelve ese issue antes de repartir consultas. Adjuntar un documento histórico aporta contexto; no restablece sus reglas antiguas de nombres o guardado.

Para implementar con draft PR, comprueba también la conexión GitHub MCP y las herramientas de ramas, archivos, PR, checks y Copilot incluidas en el perfil. [La configuración de VS Code](../.vscode/settings.json) solicita para este workspace los conjuntos `default`, `actions`, `code_security`, `dependabot`, `secret_protection`, `code_quality`, `governance` y `copilot`; cada perfil sigue limitando las herramientas que puede usar. Tras cambiar la configuración, reinicia el servidor GitHub MCP o ejecuta `MCP: Reset Cached Tools` y comprueba las herramientas reales en el selector del agente. La [guía de PR y revisiones](github-pr.md) distingue la configuración versionada de los permisos y servicios que deben verificarse en GitHub. Declarar una herramienta no conecta ni habilita servicios por sí solo.

## Si falta una herramienta

Después de modificar un perfil, vuelve a seleccionarlo y comprueba las herramientas que la sesión cargó. Si una herramienta declarada no está disponible, informa la limitación y verifica el perfil y la copia del repositorio realmente utilizados. No inventes nombres de herramientas ni elimines la propiedad `tools` para ocultar el problema.

Si falta lectura, usa el contenido adjunto disponible e indica el límite del análisis. Si falta edición, entrega el informe y su destino fijo para guardado manual, declarando que no fue guardado. Si falta GitHub, sigue las reglas del perfil para registrar lo pendiente y continuar el trabajo local autorizado, sin inventar issues, PR ni comentarios publicados. Conserva el resumen preparado en el informe y comprueba duplicados antes de reintentar cuando se recupere la conexión.

Los permisos descritos son instrucciones de alcance; una herramienta general de edición no restringe por sí sola las rutas. Conserva el trabajo ajeno, no adjuntes secretos y distingue las salidas realmente observadas de los pasos sugeridos. La disponibilidad de perfiles y herramientas se verifica en la instalación utilizada; esta guía no afirma una ejecución autenticada.
