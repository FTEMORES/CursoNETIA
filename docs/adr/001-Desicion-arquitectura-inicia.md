# ADR 0001: Estructura de proyectos con Clean Architecture para AulaPedidos

**Estado:** Aceptado

## Contexto

AulaPedidos es un proyecto didáctico para enseñar a alumnos (con conocimientos previos de DI, DDD, etc.) cómo diseñar una arquitectura limpia apoyándose en IA y un buen harness de pruebas. Actualmente solo existe el proyecto `AulaPedidos.Api` (minimal API) sin separación de capas. Se necesita una estructura que separe claramente responsabilidades y sea fácil de explicar en clase.

## Decisión

Se adopta una estructura de 4 proyectos, uno por capa, cada uno en su propia carpeta bajo `src/`:

- `AulaPedidos.Domain`: entidades y reglas de negocio puras, sin dependencias externas.
- `AulaPedidos.Application`: casos de uso, interfaces de repositorios/servicios, orquestación. Depende solo de `Domain`.
- `AulaPedidos.Infrastructure`: implementaciones técnicas (persistencia, servicios externos). Depende de `Application`/`Domain`.
- `AulaPedidos.Api`: capa de presentación (ya existente). Depende de `Application` e `Infrastructure` (esta última solo para el registro de DI en `Program.cs`).

Regla de dependencias: Domain <- Application <- Infrastructure/Api. Ninguna capa interna conoce a una externa.

Estructura de carpetas:

CursoNETIA.slnx
src/
 - AulaPedidos.Api/ (AulaPedidos.Api.csproj)
 - AulaPedidos.Application/ (AulaPedidos.Application.csproj)
 - AulaPedidos.Domain/ (AulaPedidos.Domain.csproj)
 - AulaPedidos.Infrastructure/ (AulaPedidos.Infrastructure.csproj)

## Alternativas consideradas

1. Monolito en un solo proyecto con carpetas internas: más simple de montar, pero no enseña el desacoplamiento real entre capas (el compilador no obliga a respetar la regla de dependencias).
2. Estructura por features (vertical slices) en vez de por capas: útil en proyectos grandes, pero menos didáctica para introducir Clean Architecture clásica a alumnos que la ven por primera vez.
3. 4 proyectos por capa (elegida): balance adecuado para fines educativos; el compilador refuerza la regla de dependencias y facilita explicar visualmente el grafo de referencias en Visual Studio.

## Consecuencias

- Mayor claridad pedagógica: los alumnos ven físicamente la separación de capas en la solución.
- Más configuración inicial (4 .csproj y referencias entre ellos) frente a un solo proyecto.
- Facilita añadir después proyectos de test por capa (Domain.Tests, Application.Tests, etc.) sin reestructurar.
- Requiere disciplina para no violar la regla de dependencias (ej. que Domain no referencie Infrastructure).