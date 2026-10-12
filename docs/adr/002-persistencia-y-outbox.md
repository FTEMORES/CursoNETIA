# ADR 002: proveedores locales y entrega de eventos

- Estado: aceptada con limitaciones explícitas.
- Contexto: el desarrollo local debe poder comenzar sin Docker, y el sistema debe evitar perder una notificación después de guardar un pedido.
- Decisión: SQLite para puesta en marcha local, SQL Server para recorrido de contenedores, migraciones independientes; guardar agregado y mensaje outbox en transacción local. Entregar posteriormente mediante worker y adaptador HTTP.
- Alternativas: notificar antes de guardar puede generar mensajes de pedidos inexistentes; notificar después sin persistir intención puede perder mensajes; una transacción distribuida agrega complejidad operativa sin una necesidad demostrada.
- Consecuencias: entrega al menos una vez, latencia eventual y necesidad de idempotencia. Las pruebas con SQLite no sustituyen la validación con SQL Server. Un worker de referencia no resuelve reclamo seguro con varias réplicas.
- Evidencia: tests transaccionales, fila outbox pendiente y recuperación al reanudar notificador.
- Revisar al escalar trabajadores, incorporar broker real o exigir garantías operativas específicas.
