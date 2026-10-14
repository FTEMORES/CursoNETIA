# ADR 003: Copilot como colaborador supervisado

> Nota de vigencia: el texto siguiente conserva la decisión histórica. Sus requisitos de proceso y evidencia por ronda se sustituyen por el [flujo vigente](../multiagente-copilot.md) y el [contrato de informes](../informes/README.md). No se exige expediente ni una ronda completa para atender una petición directa; los entregables usan la carpeta consecutiva registrada y los nombres fijos del contrato.

- Estado: aceptada.
- Contexto: acelerar generación, análisis y pruebas sin delegar decisiones de negocio y seguridad a una salida probabilística.
- Decisión: instrucciones versionadas, prompts pequeños, herramientas acotadas, revisión humana y verificaciones ejecutables. No fijar un modelo del plan individual. Aplicar el protocolo multiagente de ADR 004: consultas separadas, síntesis con desacuerdos, decisión humana, único escritor y verificación separada.
- Alternativas: aceptar sugerencias sin evidencia es rápido sólo en apariencia; prohibir IA elimina una capacidad útil bajo controles.
- Consecuencias: el equipo debe invertir en criterios y pruebas. Las instrucciones pueden incumplirse; la revisión y los controles técnicos detectan parte de esos fallos. El registro de cambios conserva tanto rechazos como aceptaciones.
- Evidencia: cada cambio o ronda entrega expediente y estado base, informes A/B reales, síntesis, decisión humana, diff, comandos y resultados, prueba negativa y justificación. Registrar lo pendiente; consenso y cambio de rol no demuestran validación independiente.
- Revisar cuando cambien políticas, disponibilidad o capacidades de Visual Studio.
