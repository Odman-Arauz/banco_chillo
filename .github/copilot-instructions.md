# Instrucciones para GitHub Copilot — Banco Chillo

Sigue las reglas de `AGENTS.md` (raíz del repositorio). Resumen:

- Responde y escribe código en **español** (clases, métodos, variables, comentarios, mensajes de error, commits).
- Microservicios .NET 10: Clean Architecture (Domain → Application → Infrastructure → Api), MediatR 12, FluentValidation,
  EF Core + Npgsql con `UseSnakeCaseNamingConvention`. Copia la estructura de `src/services/cajeros`.
- Microservicios Node.js 22 + Express 5 (ESM): rutas, esquemas zod, repositorio con SQL parametrizado, errores ProblemDetails.
  Copia la estructura de `src/services/notificaciones`.
- Rutas REST `/api/v1/<servicio>/<recurso>`. JWT HS256 con roles en el claim `role`.
- PostgreSQL: un esquema `<modulo>_bc` por servicio, scripts idempotentes, dinero `NUMERIC(14,2)`,
  tablas de movimientos append-only (sin UPDATE ni DELETE).
- Nunca generes datos personales reales ni secretos. No modifiques archivos fuera del módulo del Issue.
- Pruebas: xUnit + FluentAssertions (.NET), `node --test` + supertest (Node), Jest (PWA).
