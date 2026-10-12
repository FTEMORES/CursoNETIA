# AulaPedidos: instrucciones para la clase

Trabaja en español y usa nombres de código en inglés. El proyecto usa .NET 10, Minimal APIs y la solución CursoNETIA.slnx.

## Flujo de trabajo
- Atiende directamente la petición del usuario según el rol seleccionado.
- Para analizar o implementar no se requieren expediente, identificador, commit aportado por el alumno, diff previo, consultas A/B, síntesis, congelación de contexto ni aprobación de guardado.
- Una petición de crear, completar o corregir código autoriza al Implementador a realizar ese trabajo local dentro del alcance solicitado.
- Lee los archivos disponibles y continúa con la información suficiente. Pregunta sólo por un dato que cambie materialmente el resultado y no pueda obtenerse del proyecto.
- Los documentos de docs/expedientes son contexto opcional. Sus estados antiguos no detienen el trabajo.
- docs/harness-avanzado conserva material para una clase posterior; no aplica al flujo actual.
- No simules otros agentes, informes, comandos ni resultados. En Visual Studio el alumno selecciona los roles manualmente.

## Informes entre chats
Cuando se use el flujo 2A/2B, cada rol guarda su conclusión al terminar:
- Arquitecto: docs/informes/<tarea>/2A-arquitectura.md.
- Pruebas: docs/informes/<tarea>/2B-pruebas.md.
- Coordinador, paso 3 (síntesis o paso siguiente): lee esos dos archivos y guarda docs/informes/<tarea>/3-sintesis.md.
Usa el nombre de tarea indicado en la petición o documentación adjunta; para la base actual usa BASE-001. Si no hay nombre, usa tarea-actual y comunica la ruta, sin detener el trabajo para pedir un identificador.
Una petición de análisis a estos roles incluye guardar el informe de la tarea. No exige confirmación adicional ni documento previo.
Arquitecto y Pruebas pueden editar únicamente su informe, sin cambiar código, tests, configuración ni informes de otros roles. Esta restricción es de instrucciones; editfiles no impone aislamiento de rutas.
Antes de guardar, comprueba si existe y corresponde al mismo objetivo. Para la misma tarea, actualiza conservando decisiones humanas y notas ajenas; si pertenece a otro objetivo, conserva el archivo y usa una carpeta con sufijo descriptivo, comunicando la ruta al alumno.
El informe debe contener objetivo, rol, archivos realmente consultados, conclusiones, propuestas, supuestos, comprobaciones realizadas o pendientes y siguiente paso. No copies toda la conversación ni secretos; no inventes evidencias.
En 2A y 2B analiza la petición y archivos de proyecto, sin usar el informe del otro para formar tu propuesta. El Coordinador los combina después.
Si falta un informe, el Coordinador trabaja con el disponible y registra la ausencia. Si difieren en objetivo o código analizado, señala la diferencia; no inventa consenso ni exige repetir un proceso completo.
Si editfiles no está disponible, entrega el informe listo para guardar y su ruta; declara que no fue guardado. El alumno puede adjuntarlo al siguiente chat.

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
- No publiques, elimines datos compartidos ni cambies credenciales sin una petición expresa.

## Comprobación
Desde la raíz: dotnet build CursoNETIA.slnx.
Cuando existan proyectos de pruebas: dotnet test CursoNETIA.slnx.
Para la API: dotnet run --project src/AulaPedidos.Api.

## Alcance actualizado de BASE-001
Repository Pattern y la biblioteca Mediator de martinothamar están expresamente solicitados. No usar MediatR ni un mediador propio. El alcance vigente está en docs/paquetes/BASE-001-paquete-comun.md. Preparar contratos de Repository sin almacenamiento ficticio y registrar Mediator desde Api. Mantener ausencia de entidades y lógica de negocio.
