---
applyTo: "src/AulaPedidos.Api/**/*.cs"
---

Usa Minimal APIs. Mantén el negocio en Application y Domain. Configura DI y middleware desde Api. Describe rutas y respuestas con OpenAPI. Aplica validación, ProblemDetails, autenticación y autorización cuando lo requiera la funcionalidad; no implementes mecanismos ajenos a la petición. No registres secretos.

Aplica el flujo de clase de .github/copilot-instructions.md: trabaja directamente con la petición y el contexto disponible, sin requisitos de documentos de proceso.

Para BASE-001, registra la biblioteca Mediator de martinothamar con su código generado: Mediator.SourceGenerator se instala en Api y AddMediator se invoca en la composición. No usar MediatR ni instalar el generador en todas las capas. Selecciona un lifetime compatible con futuras dependencias scoped.
