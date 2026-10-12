---
name: revisar-nombres
description: "Revisa nombres de clases, interfaces, métodos, propiedades, parámetros y variables en código C# para detectar ambigüedades, abreviaturas y nombres poco descriptivos. Úsala cuando se solicite evaluar o mejorar la claridad de los nombres sin modificar archivos."
---

# Revisión de nombres en código C#

Analiza el archivo o fragmento de código C# indicado por el usuario y evalúa si sus nombres comunican claramente su propósito.

## Instrucciones

- Lee completamente el código proporcionado antes de emitir recomendaciones.
- Revisa nombres de namespaces, clases, interfaces, métodos, propiedades, campos, parámetros y variables locales.
- Detecta nombres ambiguos, demasiado genéricos, engañosos o que dependan de abreviaturas difíciles de entender.
- Comprueba que los nombres describan la intención y no únicamente el tipo de dato.
- Verifica que los booleanos expresen una condición clara, preferiblemente con prefijos como `Is`, `Has`, `Can` o `Should`.
- Comprueba que los métodos utilicen verbos que describan la acción realizada.
- Comprueba que las clases, propiedades y variables utilicen sustantivos descriptivos.
- Revisa que las interfaces sigan la convención de C# y comiencen por `I`.
- Ten en cuenta el contexto del dominio antes de considerar incorrecta una abreviatura conocida.
- Respeta las convenciones de nombres que ya sean consistentes en el proyecto.
- No propongas cambios meramente estéticos si el nombre actual ya es claro.
- No modifiques ningún archivo; limita la respuesta a observaciones y sugerencias.

## Criterios de revisión

Considera problemático un nombre cuando:

- No permite deducir la responsabilidad del elemento.
- Utiliza nombres genéricos como `data`, `item`, `obj`, `temp`, `value` o `manager` sin suficiente contexto.
- Contiene abreviaturas no habituales o letras aisladas fuera de bucles y expresiones breves.
- Contradice el comportamiento real del método o el contenido de la variable.
- Incluye detalles de implementación que podrían cambiar sin alterar su propósito.
- Repite innecesariamente información que ya aporta el tipo o el contexto contenedor.
- No sigue las convenciones habituales de PascalCase y camelCase de C#.

## Ejemplos

- `x` puede sustituirse por `customerCount` si representa una cantidad de clientes.
- `GetData()` puede sustituirse por `GetActiveOrders()` si devuelve pedidos activos.
- `flag` puede sustituirse por `isPaymentApproved` si indica la aprobación de un pago.
- `Process()` puede sustituirse por `GenerateMonthlyReport()` si esa es su responsabilidad.
- `usr` puede sustituirse por `user` cuando la abreviatura no aporta claridad.

## Formato de respuesta

Presenta los resultados en una lista o tabla con estos datos:

1. Nombre actual.
2. Tipo de elemento, como clase, método, propiedad, parámetro o variable.
3. Motivo por el que podría resultar confuso.
4. Nombre sugerido.
5. Justificación breve de la propuesta.

Si todos los nombres son claros, indícalo expresamente. No inventes problemas para completar el informe.
