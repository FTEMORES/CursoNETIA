---
applyTo: "src/AulaPedidos.Application/**/*.cs"
---

Application depende de Domain y coordina casos de uso mediante contratos. No uses HttpContext ni DbContext concreto. Usa async y CancellationToken para I/O. Reutiliza patrones existentes cuando realmente existan; no agregues mediador, Result o repositorios sin necesidad.

Aplica el flujo de clase de .github/copilot-instructions.md: trabaja directamente con la petición y el contexto disponible, sin requisitos de documentos de proceso.

En BASE-001 están solicitados Repository Pattern y la biblioteca Mediator de martinothamar: define IRepository<T> en Application/Abstractions y usa Mediator.Abstractions para mensajes y handlers cuando correspondan. No usar MediatR ni un mediador propio. No exponer IQueryable o detalles de persistencia. Consulta docs/paquetes/BASE-001-paquete-comun.md.
