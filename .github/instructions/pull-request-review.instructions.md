---
applyTo: "**"
---

# Revisiones de pull requests

Estas instrucciones se aplican cuando se revisa un PR, incluida la revisión de GitHub Copilot. Responde en español y conserva los nombres del código en inglés. Revisa los cambios reales y su contexto, sin implementar. El servicio remoto GitHub Copilot code review entrega comentarios en el PR y no genera documentos del flujo de clase. El perfil local Revisor sí conserva su informe `06-revision.md` en la carpeta canónica de la tarea, según las instrucciones comunes.

## Seguridad

- Busca vulnerabilidades reproducibles: autorización ausente o incorrecta, acceso a datos ajenos, validación insuficiente, inyección SQL, deserialización insegura, exposición de secretos/datos personales, rutas o URLs controladas por entradas y permisos excesivos.
- En EF Core revisa parametrización, transacciones, coherencia de Unit of Work, consultas y límites de datos. No solicites autenticación u otras funciones nuevas sólo porque aún no existen: señala el riesgo cuando el cambio y sus requisitos la necesiten.
- Revisa dependencias y sus alertas, y workflows por ejecución de entradas no confiables, exposición de credenciales, permisos de escritura innecesarios y uso inseguro de `pull_request_target`.
- Usa resultados disponibles de CodeQL, Dependency Review, NuGet Audit y secret scanning como evidencia. No copies secretos en comentarios ni interpretes ausencia de acceso a alertas como ausencia de vulnerabilidades.

## Pruebas y cobertura

- Contrasta cada cambio de comportamiento con sus requisitos y con casos positivos, negativos y de borde. Señala pruebas que no verifican resultados, oculten excepciones, dependan del orden o reproduzcan únicamente la implementación.
- Revisa los resultados de tests y el informe de cobertura del commit vigente. El control exige al menos 80 % de cobertura de líneas combinada del código mantenido: sólo se excluyen archivos autogenerados de `obj/` y también se publica el reporte sin filtro. No equivale a cobertura de ramas, de cada archivo ni de cada línea nueva. Señala rutas nuevas importantes sin pruebas aunque pase el umbral global.
- No deduzcas porcentajes leyendo el código. No recomiendes reducir el umbral, omitir tests ni añadir exclusiones de código mantenido para aprobar el check. Separa resultados observados, hipótesis y evidencia pendiente.

## Calidad y arquitectura

- Revisa defectos, regresiones, contratos públicos, manejo de errores, nulabilidad, recursos, concurrencia, rendimiento, async/await y propagación de CancellationToken.
- Conserva las referencias Domain → ninguna otra capa; Application → Domain; Infrastructure → Application y Domain cuando corresponda; Api → Application/Infrastructure. Usa la biblioteca Mediator existente, sin inventar otro mediador o introducir MediatR.
- Evalúa legibilidad, nombres y complejidad cuando afecten mantenimiento o comportamiento. Evita comentarios de estilo que ya resuelvan los analizadores o propuestas fuera del alcance.
- No trates una advertencia de compilación como resuelta hasta ver el cambio y la comprobación correspondiente. No confundas un análisis CodeQL ejecutado correctamente con cero alertas abiertas.

## Cómo informar

Para cada hallazgo indica gravedad, archivo/línea, condición que lo activa, consecuencia y corrección concreta. Prioriza problemas verificables de seguridad y corrección; identifica las dudas como dudas. No inventes fallos para completar una lista ni declares aprobación humana. Si están disponibles MCP, checks y artefactos, consulta sólo el contexto necesario y enlaza la evidencia; si no lo están, declara la limitación.
