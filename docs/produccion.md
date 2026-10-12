# Preparación para producción

La solución incluye controles técnicos ejecutables, pero no se declara lista para cualquier producción: los requisitos operativos dependen de la organización, los datos, el volumen y la plataforma. Antes de liberar, registrar evidencia y decisiones pendientes con una persona responsable.

| Área | Estado actual | Trabajo exigido antes de producción |
|---|---|---|
| Identidad | JWT local generado fuera de la API | OIDC real, MFA, ciclo de usuarios, claves rotables, claims acordados |
| Secretos | user-secrets y `.env` local | Bóveda, identidad de workload, rotación y escaneo histórico |
| Base de datos | SQLite local, SQL Server Compose | HA/backup, restauración probada, cifrado, capacidad y permisos mínimos |
| Esquema | Migraciones separadas por proveedor | Script revisado, prueba sobre copia, backup, despliegue expand/contract |
| Outbox | Worker de un solo proceso y entrega repetible | Reclamo atómico multiworker, idempotencia duradera, mensajes fallidos, retención |
| Caché | Memoria de proceso | Política de coherencia para réplicas, TTL medido, invalidez, stampede |
| Resiliencia | Retry/circuit breaker HTTP | Presupuesto de timeouts, jitter/carga, SLO y pruebas de caos controladas |
| Red | Compose local | TLS externo, proxy confiable, segmentación y egress restringido |
| Contenedores | Build multietapa | Imágenes aprobadas/escaneadas, registro, límites CPU/RAM y actualización |
| Telemetría | OTel y dashboard local | Colector/backend protegido, muestreo, retención, alertas y control de PII |
| Health checks | Vida y disponibilidad DB | Política de reinicio/retirada, dependencias críticas y umbrales |
| Pruebas | Unitarias e integración local | SQL Server real en CI, carga representativa, contratos externos y recuperación |
| Entrega | Pipeline verificable | Entornos, aprobaciones, SBOM según política, rollback y trazabilidad |
| Operación | Runbook base | Guardia, propietarios, objetivos SLO, manual de incidentes y simulacros |

## Ensayo de liberación

Revisar la evidencia de la liberación siguiendo el [protocolo común](multiagente-copilot.md). Una conclusión del coordinador no autoriza un despliegue. Separar a quien implementa de quien acepta y nombrar a la persona responsable de liberar. Los datos sintéticos y las credenciales locales no autorizan una publicación en producción.

1. Crear una versión identificable y registrar commit/image digest real.
2. Ejecutar build y tests; guardar resultado y cobertura con su alcance.
3. Revisar cambios de esquema y generar script SQL; no migrar automáticamente varias réplicas al arrancar.
4. Aprovisionar configuración externa y comprobar que no hay modo demo ni endpoints de diagnóstico públicos innecesarios.
5. Desplegar en staging, ejecutar smoke funcional y pruebas de autorización con identidades del entorno.
6. Observar errores, latencia, recursos y outbox; acordar umbrales de go/no-go antes de desplegar.
7. Hacer despliegue gradual compatible con la versión anterior del esquema.
8. Validar y registrar responsable. Si falla, volver a imagen anterior sólo si el esquema sigue compatible; recuperar datos exige un plan distinto.

## Escenario de capacidad

La empresa solicita 500 solicitudes por segundo y 99.9% de disponibilidad. La respuesta profesional no es «Clean Architecture ya escala». Definir mezcla de tráfico, tamaño de datos, duración, región y dependencia externa; medir staging; calcular capacidad y cuello de botella; proponer presupuesto y SLO. Una prueba en laptop no representa ese entorno.

## Operación mínima

Investigar primero alcance del incidente, despliegue reciente, salud, latencia y trazas correlacionadas. No reiniciar indiscriminadamente ni borrar outbox para ocultar pendientes. Mitigar con cambios reversibles y preservar evidencia sin datos sensibles. Tras recuperar, documentar causa, factores contribuyentes, prueba de regresión y dueño/fecha de prevención.
