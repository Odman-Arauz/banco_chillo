# 07 · Cómo trabajar con IA sin que divague

La IA (Claude, Copilot, ChatGPT, Gemini…) **está permitida y recomendada**, pero tú eres responsable de cada línea que subes:
debes poder explicarla en la revisión y en la defensa.

## Por qué la IA "divaga"

Porque no conoce el proyecto. Sin contexto inventa su propia arquitectura, otras carpetas, otras librerías,
y mezcla inglés y español. La solución es **darle contexto y limitarle el alcance**.

## Archivos que la IA DEBE leer primero

Este repositorio ya trae instrucciones que muchas herramientas leen solas:

| Herramienta | Archivo que lee automáticamente |
|---|---|
| Claude Code | `CLAUDE.md` → apunta a `AGENTS.md` |
| GitHub Copilot (VS Code) | `.github/copilot-instructions.md` |
| Cursor, Codex, otros agentes | `AGENTS.md` |
| ChatGPT / Gemini web | **Ninguno**: copia y pega el bloque de contexto de abajo |

## Plantilla de prompt (cópiala y llénala)

```text
Trabajo en el proyecto Banco Chillo (monorepo). Reglas obligatorias:
- Lee primero AGENTS.md, docs/guias/README.md y el ejemplo de referencia
  [src/services/cajeros  |  src/services/notificaciones].
- Todo en español: nombres de clases, métodos, variables, comentarios y mensajes.
- Copia EXACTAMENTE la estructura y el estilo del ejemplo de referencia. No inventes carpetas ni capas nuevas.
- No agregues librerías que no estén ya en el proyecto sin decírmelo antes.
- Datos de prueba SIEMPRE ficticios. Nunca secretos reales.
- Montos con decimal / NUMERIC(14,2). Tablas de transacciones append-only (sin UPDATE ni DELETE).

Mi tarea es el Issue #[número]: [pega aquí el texto del Issue o su sección "Qué debes hacer"].
Hazlo SOLO para [la parte concreta, ej. "el script SQL de cuentas_bc"]. No toques otros módulos.
Antes de escribir código, dime en 5 líneas qué archivos vas a crear o modificar.
```

## Buenas prácticas

1. **Una tarea pequeña por conversación.** "Haz el CRUD de socios" es mejor que "haz el microservicio de cuentas".
2. **Pide el plan primero** ("dime qué archivos vas a tocar") y corrígelo antes de que escriba código.
3. **Ejecuta y prueba todo lo que te da.** `dotnet build`, `dotnet test`, `npm test`. Si falla, pégale el error **completo**.
4. **Pídele que te explique** lo que no entiendas: "explícame línea por línea este handler".
5. **Revisa el diff antes de hacer commit** (`git diff`). La IA a veces modifica archivos que no pediste.
6. **Pide pruebas:** "escribe 3 pruebas xUnit para este validador siguiendo `CrearAgenciaTests.cs`".

## Lo que NO debes hacer

- ❌ Pegar en la IA secretos, contraseñas, tokens o tu llave SSH privada.
- ❌ Aceptar que la IA cambie `AGENTS.md`, los workflows de `.github/`, `docker-compose.yml` o archivos de otros
  módulos si tu Issue no lo pide.
- ❌ Subir código que no compila "porque la IA dijo que funcionaba".
- ❌ Dejar que la IA genere datos que parezcan reales (nombres y cédulas de personas reales).

## Commits con ayuda de IA

Puedes pedirle el mensaje de commit:

```text
Dame un mensaje Conventional Commit en español para este diff (máximo 72 caracteres):
[pega la salida de git diff --staged]
```

El commit lo haces tú, con tu cuenta. Los commits y PRs del proyecto no llevan firmas de herramientas de IA.
