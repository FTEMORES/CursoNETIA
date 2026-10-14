# AulaPedidos: instrucciones para la clase

Trabaja en español y usa nombres de código en inglés. El proyecto usa .NET 10, Minimal APIs y la solución CursoNETIA.slnx.

## Cuando GitHub Copilot revise un PR
- Aplica `.github/instructions/pull-request-review.instructions.md`: revisa seguridad, pruebas/cobertura y calidad con evidencia del diff y los checks del commit actual.
- En una revisión de PR comenta los hallazgos en la revisión; no actúes como Implementador ni crees informes locales, ramas, commits o PR adicionales. Las reglas de guardado de los perfiles de clase se aplican a esos perfiles, no al servicio de code review de GitHub.

## Flujo de trabajo
- Atiende directamente la petición del usuario según el rol seleccionado.
- Para analizar o implementar no se requieren expediente, identificador, commit aportado por el alumno, diff previo, consultas A/B, síntesis, congelación de contexto ni aprobación de guardado.
- Una petición de crear, completar o corregir código autoriza al Implementador a realizar el trabajo y publicar un draft PR mediante GitHub MCP según `docs/github-pr.md`, salvo petición explícita de sólo trabajo local. Un issue previo no es obligatorio.
- Las entregas de Arquitecto, Pruebas y la síntesis del Coordinador incluyen publicar un resumen en el issue de la misma tarea mediante GitHub MCP, según `docs/github-pr.md`, sin confirmación adicional. El Coordinador resuelve el issue antes de repartir consultas; cada rol registra el comentario real o la publicación pendiente en su informe. Respeta una petición explícita de sólo chat, sólo local o no publicar.
- Lee los archivos disponibles y continúa con la información suficiente. Pregunta sólo por un dato que cambie materialmente el resultado y no pueda obtenerse del proyecto.
- Los documentos de docs/expedientes son contexto opcional. Sus estados antiguos no detienen el trabajo.
- docs/harness-avanzado conserva material para una clase posterior; no aplica al flujo actual.
- No simules otros agentes, informes, comandos ni resultados. En Visual Studio el alumno selecciona los roles manualmente.

## Entregables de todos los agentes
- Aplica siempre [la convención de informes](../docs/informes/README.md), también en peticiones directas a un rol. Es la fuente única para destinos, nombres y consecutivos; las rutas en informes, expedientes y ADR históricos son antecedentes, no instrucciones de guardado.
- Todo entregable se guarda bajo `docs/informes/TAREA-NNN/`, con el nombre fijo de su rol o fase. Antes de escribir, consulta `docs/informes/indice.md` y resuelve o registra la tarea según la convención. El número pertenece a la tarea y se comparte entre todos los roles.
- Un objetivo nuevo recibe el siguiente número libre después del máximo registrado o existente. Un seguimiento del mismo objetivo conserva su ID y actualiza el archivo correspondiente. No elijas la última tarea por defecto, ni inventes carpetas por título, fecha, categoría o sufijo.
- La petición incluye guardar el entregable sin confirmación adicional. No se generan archivos por saludos o preguntas intermedias. Respeta una indicación explícita de sólo chat o no guardar.
- La edición local de Coordinador, Arquitecto, Pruebas, Revisor e Ideas se limita a sus entregables y a añadir o reparar la inscripción de su propia tarea en el índice. No cambian código, tests, configuración ni informes ajenos. Además, Arquitecto y Pruebas publican sus propios resúmenes en el issue; el Coordinador gestiona el issue y publica la síntesis según la guía. Las listas de herramientas GitHub de cada perfil restringen operaciones disponibles, pero no los argumentos, las rutas ni el destinatario de cada llamada; verifica siempre su alcance.
- Los prompts que entregues a otro rol deben llevar el ID, la carpeta, las rutas concretas de entrada y salida y el repositorio, número y URL del issue verificado, o su estado pendiente. Conserva decisiones humanas, notas ajenas y evidencia anterior al actualizar; no inventes evidencias, consenso ni resultados.
- En las consultas separadas 2A/2B, Arquitecto y Pruebas analizan requisitos y código sin usar el informe ni los comentarios del otro rol para formar su propuesta; consultan comentarios para deduplicar sólo después de concluirla. El Coordinador lee los informes realmente disponibles, registra ausencias o diferencias y guarda la síntesis en la misma tarea.
- Si no puedes consultar el índice/inventario o guardar, declara la limitación y entrega el contenido preparado; no inventes un consecutivo ni afirmes que se guardó.

## Arquitectura
- Domain: entidades y reglas, sin dependencias de EF, HTTP ni otras capas.
- Application: casos de uso y contratos; depende de Domain.
- Infrastructure: implementaciones técnicas; depende de Application y, cuando corresponda, Domain.
- Api: composición y transporte mediante Minimal APIs; referencia Application e Infrastructure.
- Conserva CursoNETIA.slnx y el trabajo existente. Consulta código y documentación pertinente; la documentación de la aplicación completa expresa un objetivo, no capacidades ya implementadas.
- Introduce paquetes, patrones y servicios sólo cuando sean necesarios para la tarea. No supongas que existen mediador, Result, repositorios o persistencia: compruébalo.
- Las utilidades del proyecto se escriben en C#; no crear scripts de PowerShell.

## Calidad
- Cambios pequeños y coherentes con las convenciones existentes.
- Usa async/await y CancellationToken para I/O; evita .Result y .Wait().
- Valida las entradas y protege reglas del dominio.
- No guardes secretos ni datos sensibles en código o logs.
- Al implementar, compila la solución y ejecuta verificaciones proporcionales al cambio. Informa resultados reales y pendientes.
- La publicación de la rama y draft PR de implementación está autorizada por este flujo. No publiques releases, despliegues, elimines datos compartidos ni cambies credenciales sin una petición expresa. No marques un PR como listo ni lo fusiones automáticamente.

## Comprobación
Desde la raíz: dotnet build CursoNETIA.slnx.
Cuando existan proyectos de pruebas: dotnet test CursoNETIA.slnx.
Para la API: dotnet run --project src/AulaPedidos.Api.

## Alcance actualizado de BASE-001
Repository Pattern y la biblioteca Mediator de martinothamar están expresamente solicitados. No usar MediatR ni un mediador propio. El alcance vigente está en docs/paquetes/BASE-001-paquete-comun.md. Preparar contratos de Repository sin almacenamiento ficticio y registrar Mediator desde Api. Mantener ausencia de entidades y lógica de negocio.
