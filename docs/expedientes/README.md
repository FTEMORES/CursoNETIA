# Expedientes de cambio

El Coordinador propone y conserva el registro del proceso según el [protocolo multiagente](../multiagente-copilot.md). Sólo escribe después de que la persona responsable revise y confirme las rutas y el contenido o diff de cada guardado.

Para un cambio nuevo se usan dos archivos:

- `docs/expedientes/CATEGORY-001.md`: expediente acumulativo con estado, informes A/B originales, síntesis, decisiones humanas y evidencia real.
- `docs/expedientes/CATEGORY-001.paquete-comun.md`: objetivo, reglas, aceptación, alcance y estado base compartidos por A/B, sin informes ni síntesis.

Sustituye `CATEGORY-001` por el identificador acordado. El paquete queda congelado antes de las consultas; adjunta exactamente ese archivo a Arquitecto y Pruebas. El expediente recibe los informes sólo después de que ambos terminen. Si cambia el paquete, identifica su nueva versión y repite los análisis afectados.

## Ejemplo de guardado

1. Pide: «Prepara el expediente CATEGORY-001 y el paquete común. Muéstrame el contenido y las rutas antes de guardar».
2. Revisa la propuesta. Confirma: «Confirmo guardar el contenido mostrado en docs/expedientes/CATEGORY-001.md y docs/expedientes/CATEGORY-001.paquete-comun.md».
3. El Coordinador guarda esos documentos, registra quién confirmó y qué se autorizó, y comunica las rutas realmente guardadas. Si no tiene edición disponible, entrega el texto para guardado manual e informa la limitación.
4. Para incorporar informes, síntesis o cierre, revisa y confirma el nuevo diff del expediente. Una confirmación anterior no autoriza contenido nuevo.

La confirmación de guardado permite conservar documentos; la autorización para implementar y la aceptación final del cambio siguen siendo decisiones humanas explícitas. El Coordinador no cambia código, tests, documentación técnica, bitácoras ni perfiles, ni ejecuta comandos. La herramienta de edición no limita por sí sola las rutas: comprobar permisos efectivos y revisar el diff.

Los expedientes existentes en subcarpetas pueden seguir conservándose manualmente. No moverlos ni reescribirlos para adoptar esta convención. El guardado por el Coordinador se limita a los dos archivos acordados con el formato `<ID>.md` y `<ID>.paquete-comun.md`.
