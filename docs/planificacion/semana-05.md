# Semana 5 — 23 al 29 de noviembre de 2026

**Objetivo de la semana:** que cada estudiante tenga **su base de datos** y **su primer CRUD/API funcionando**
sobre la base del proyecto, con mockups del grupo, CI en verde y evidencias para las Partes 1, 3, 4 y 5 del informe.

Milestone en GitHub: **Semana 5**. Etiqueta: `semana-5`.

## Calendario

| Día | Hito |
|---|---|
| Lun 23 | Clase: presentación de la base, Stitch y flujo Git. Todos completan el **Paso 0** (entorno + `verificar-entorno.ps1` en verde) |
| Mié 25 | **PR del Issue A (base de datos)** abierto, con CI en verde |
| Vie 27 | PNG de mockups de cada módulo entregados a la responsable de la PWA |
| Dom 29, 23:59 | **PR del Issue B (API/PWA)** abierto con CI en verde + revisión cruzada hecha + informe parcial |

## Asignación

| Estudiante | Épica | Issue A — Base de datos / diseño | Issue B — API / app |
|---|---|---|---|
| @Odman-Arauz | #1 Auth | #16 Esquema `auth_bc` completo + semillas ficticias + Supabase | #17 API Auth: registro y login con JWT |
| @Ronaldtipan | #2 Gateway | #18 Gateway: completar spec-0002 sobre la base | #19 DevOps: colección Postman, Docker y Render |
| @jostin1224jp | #3, #8 | #20 `cajeros_bc` (ventanillas, apertura) + `creditos_bc` | #21 CRUD de ventanillas (.NET) |
| @josuebj72-ai | #4, #9 | #22 Operaciones de ventanilla append-only + `auditoria_bc` | #23 Microservicio Auditoría (.NET): registro con hash encadenado y consultas |
| @MateoPupiales37 | #5, #10 | #24 Arqueo y cierre inmutables + `notificaciones_bc` | #25 Notificaciones (**Node.js + Express**): preferencias, push y envío simulado |
| @MathiasAlex2005 | #6 | #26 `cuentas_bc` + `transacciones_bc` | #27 API Cuentas: CRUD de socios y saldos |
| @S0yRai | #7 PWA | #28 Mockups en Stitch: guía de estilo y consolidación | #29 PWA: login, navegación por rol y Vercel |

## Propiedad de archivos SQL (para evitar conflictos de merge)

| Archivo | Dueño |
|---|---|
| `database/schema/01-auth/*` y `database/seeds/02-auth-*.sql` | Odman |
| `database/schema/02-cuentas/*`, `04-transacciones/*` y `database/seeds/03-cuentas-*.sql` | Mathias |
| `database/schema/03-cajeros/01-cajeros-schema.sql`, `02-ventanillas-apertura.sql` y `05-creditos/*` | Jostin |
| `database/schema/03-cajeros/03-operaciones-ventanilla.sql` y `06-auditoria/*` | Josué |
| `database/schema/03-cajeros/04-arqueo-cierre.sql` y `07-notificaciones/02-*.sql` | Mateo |
| `database/seeds/01-catalogos-financieros.sql` | Cada uno agrega **solo su bloque**, al final, con un comentario `-- <módulo>` |

Si necesitas cambiar un archivo que no es tuyo, coméntalo en el Issue de su dueño.

## Revisión cruzada de Pull Requests (cada uno revisa al siguiente)

Odman → Ronald → Jostin → Josué → Mateo → Mathias → S0yRai → Odman

## Entregable grupal

Ver el Issue #30 "Entregable grupal Semana 5": mockups consolidados, diagramas ER, `docker compose --profile apps`
funcionando en cada computadora e informe con las Partes 1, 3, 4 y 5 ([guía 12](../guias/12-informe-practica.md)).

## Base entregada por el docente (antes de empezar)

- Microservicio de referencia **.NET**: `src/services/cajeros` (CRUD de agencias, JWT, validación, errores, pruebas, Docker).
- Microservicio **Node.js + Express** (el único del sistema, ADR-0004): `src/services/notificaciones` con CRUD de plantillas, JWT, zod, pruebas y Docker. Lo continúa Mateo.
- Gateway YARP con una ruta por servicio, BuildingBlocks compartidos, token de desarrollo en Auth.
- PWA Expo SDK 58 compilando (lint, tipos, Jest, export web, manifest y Service Worker).
- App de escritorio WPF que genera `BancoChillo.Cajero.exe`.
- `docker-compose.yml` que inicializa todos los esquemas, pipelines de CI/CD, `render.yaml`, `vercel.json`.
- ADR-0004 a ADR-0007 y guías `docs/guias/01` a `12`.
