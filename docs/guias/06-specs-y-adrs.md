# 06 · Cómo escribir Specs y ADRs

## ¿Spec o ADR?

| Documento | Responde a | Dónde | ¿Cuándo? |
|---|---|---|---|
| **Spec** | **Qué** hace una funcionalidad y cómo se prueba | `docs/specs/spec-00XX-<nombre>/` | **Siempre** antes de programar tu Issue |
| **ADR** | **Por qué** elegimos una tecnología o patrón | `docs/adr/ADR-00XX-<nombre>.md` | Solo si introduces algo nuevo (librería, patrón, cambio de comunicación entre servicios) |

## Spec en 4 pasos

```powershell
# El número de la spec = número de tu Issue
Copy-Item -Recurse docs/specs/_plantilla docs/specs/spec-0012-esquema-auth
```

Llena los 3 archivos:

1. **`spec.md`** — propósito, historias de usuario, reglas de negocio y criterios **Gherkin**:
   ```gherkin
   Escenario: Crear agencia con código duplicado
     Dado que existe la agencia "AG-001"
     Cuando un administrador envía POST /api/v1/cajeros/agencias con código "AG-001"
     Entonces recibe HTTP 409
     Y el mensaje indica que el código ya existe
   ```
2. **`plan.md`** — qué archivos vas a crear o modificar, capa por capa (Domain, Application, Infrastructure, Api), y las tablas SQL.
3. **`tasks.md`** — checklist de tareas (`- [ ]`) que irás marcando (`- [x]`).

Sube la spec en tu **primer commit** (`docs(cuentas): agregar spec-0013 de crud de socios`) para que el Tech Lead la revise
en tu PR. Ejemplo completo para copiar: el CRUD de agencias en `src/services/cajeros`.

## ADR en 4 pasos

```powershell
Copy-Item docs/adr/ADR-0000-plantilla.md docs/adr/ADR-0008-idempotencia-transferencias.md
```

1. **Contexto:** qué problema hay (2–4 líneas).
2. **Opciones consideradas:** al menos 2, con pros y contras.
3. **Decisión:** cuál y por qué.
4. **Consecuencias:** qué ganamos y qué costo asumimos.

Estado inicial: `Propuesto`. Cuando el Tech Lead aprueba el PR, cambia a `Aceptado`.

ADRs vigentes que **debes leer**:

| ADR | Tema |
|---|---|
| 0001 | Microservicios con Clean Architecture y CQRS |
| 0002 | Frontend PWA con React Native / Expo |
| 0003 | Flujo de cajeros y arqueo inmutable |
| 0004 | Arquitectura híbrida .NET + Node.js |
| 0005 | Despliegue y CI/CD (Supabase, Render, Vercel) |
| 0006 | App de escritorio WPF |
| 0007 | Un esquema PostgreSQL por microservicio |

## Ejemplo: ¿necesito un ADR?

| Situación | ¿ADR? |
|---|---|
| Agrego un CRUD siguiendo el ejemplo de Cajeros | No |
| Quiero usar `Dapper` en lugar de EF Core | **Sí** |
| Voy a publicar eventos en RabbitMQ por primera vez | **Sí** |
| Agrego una tabla a mi propio esquema | No (va en la spec) |
| Quiero que mi servicio lea tablas de otro esquema | **No se permite** (ADR-0007) |
