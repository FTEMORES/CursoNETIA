# Debugging

Sigue .github/copilot-instructions.md y docs/informes/README.md. Trabaja directamente con la petición y los archivos disponibles; no requiere documentos previos de proceso. Resuelve o asigna el ID de tarea y su carpeta según esa convención y docs/informes/indice.md.

Diagnostica el error con el código, síntoma y salidas disponibles. Separa hechos de hipótesis y propone la reparación mínima. Con el rol Implementador, si el usuario pidió corregir, aplica la reparación y compruébala; con un rol lector, entrega diagnóstico y pasos.

Al implementar una reparación, sigue docs/github-pr.md: abre o actualiza el draft PR de la tarea por GitHub MCP y solicita Copilot code review, sin exigir un issue. Revisa los checks de seguridad, cobertura y calidad del cambio actual. Respeta una petición de sólo trabajo local; no abras PR por un diagnóstico sin cambios.

Guarda la entrega sustantiva en el archivo fijo del rol dentro de la carpeta resuelta docs/informes/TAREA-NNN/: 05-implementacion.md para Implementador; 02-arquitectura.md, 03-pruebas.md o 06-revision.md para el rol lector correspondiente. Los lectores sólo editan su informe y la inscripción mínima en el índice si inician la tarea; no editan código, tests, configuración ni informes ajenos. Para el mismo objetivo actualiza el archivo. Comunica ID y ruta exacta realmente guardada. Respeta una petición explícita de sólo chat o sin archivos; si no puedes guardar, entrega el contenido y declara que no fue guardado.
