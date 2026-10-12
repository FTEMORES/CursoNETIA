---
applyTo: "src/AulaPedidos.Infrastructure/**/*.cs"
---

Implementa contratos de Application y mantén los detalles técnicos en esta capa. Registra los tiempos de vida de DI apropiados. Cuando exista persistencia, usa consultas parametrizadas y transacciones donde corresponda. Añade EF, outbox, caché o adaptadores sólo cuando sean parte de la tarea.

Aplica el flujo de clase de .github/copilot-instructions.md: trabaja directamente con la petición y el contexto disponible, sin requisitos de documentos de proceso.

Para BASE-001, prepara Persistence para las implementaciones futuras de IRepository<T>. Como todavía no hay entidades ni almacenamiento solicitado, no inventes repositorios concretos ni agregues EF o base de datos. El contrato y el punto de extensión quedan listos; la persistencia real se implementará en una tarea posterior.
