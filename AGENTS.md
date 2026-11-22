# AGENTS.md

Reglas y lineamientos para agentes de Inteligencia Artificial y desarrolladores humanos en **Banco Chillo**.
Idioma obligatorio: **Español** en código, documentación, commits, Pull Requests y mensajes de soporte.

---

## 1. Visión del Proyecto

**Banco Chillo** es una plataforma financiera integral que comprende:
1. **Banca Web PWA (React Native / Expo Web)**: Experiencia responsive, segura y offline-first tanto para clientes/socios como para personal administrativo de agencias.
2. **Backend de Microservicios (.NET 9/10 C#)**: Arquitectura limpia (Clean Architecture), CQRS con MediatR y comunicación asíncrona orientada a eventos.
3. **Módulo Transaccional y de Cajeros**: Control operativo de ventanilla, apertura de caja, depósitos, retiros con doble validación, cuadres y arqueo de billetes/monedas con cierre de jornada.
4. **Base de Datos y Persistencia**: PostgreSQL relacional de alta fidelidad transaccional con proyección de despliegue en **Supabase** y Row Level Security (RLS).
5. **Ciclo de Desarrollo**: 17 semanas divididas en 4 fases estratégicas.

---

## 2. Stack Tecnológico Confirmado

- **Frontend PWA**: React Native (React Native for Web / Expo SDK), TypeScript, Zustand/Redux Toolkit, Tailwind/NativeWind, Service Workers para capacidades PWA.
- **Backend núcleo**: .NET 10 C# con Clean Architecture (`Domain`, `Application`, `Infrastructure`, `Api`) por cada microservicio, MediatR 12 (licencia Apache), FluentValidation, EF Core + Npgsql, xUnit + FluentAssertions 7 para pruebas.
- **Backend periférico (ADR-0004)**: Node.js 22 + Express 5 solo para Notificaciones (zod, jsonwebtoken, pg, helmet; pruebas con `node --test` + supertest).
- **App de escritorio (ADR-0006)**: WPF .NET 10 + CommunityToolkit.Mvvm, publicada como un único `.exe`.
- **Despliegue (ADR-0005)**: Supabase (PostgreSQL), Render (Docker), Vercel (PWA), GitHub Container Registry y GitHub Actions.
- **Gateway & Resiliencia**: YARP (Yet Another Reverse Proxy) / .NET Gateway, Polly (Circuit Breaker, Retry, Timeout).
- **Mensajería & Eventos**: RabbitMQ / EventBus con Outbox Pattern para garantizar consistencia transaccional eventual.
- **Base de Datos**: PostgreSQL 16+ / Supabase con migraciones versionadas y scripts idempotentes.
- **Caché y Sesiones**: Redis para tokens temporales, rate limiting y sesiones bancarias.

---

## 3. Comandos Esenciales

```bash
# Backend (.NET)
dotnet build src/services/auth/BancoChillo.Auth.Api
dotnet test
dotnet format

# Frontend PWA (React Native / Expo)
cd src/apps/pwa-banca
npm run web           # Modo desarrollo web PWA
npm run test          # Pruebas unitarias Jest
npm run lint

# Microservicios Node.js (ej. notificaciones)
cd src/services/notificaciones
npm run dev
npm test

# Infraestructura Local (Docker)
docker compose up -d                          # Postgres, Redis, RabbitMQ
docker compose --profile apps up -d --build   # + todos los microservicios

# Verificar entorno del desarrollador
.\scripts\verificar-entorno.ps1
```

---

## 4. Política de Ramas y Commits

- **`main`**: Rama de producción protegida. Solo recibe merges mediante Pull Request aprobado desde `develop` o `hotfix/*`. Push directo **estrictamente prohibido**.
- **`develop`**: Rama base de integración y desarrollo. Recibe Pull Requests desde ramas de características.
- **Solo `main` y `develop` son permanentes.** Toda otra rama se borra al fusionar su Pull Request (workflow `.github/workflows/limpiar-ramas.yml`); para cambios posteriores se crea una rama nueva desde `develop`.
- **Nomenclatura de ramas**:
  - `feat/<issue-id>-<descripcion-corta>` (ej. `feat/10-apertura-caja-cajero`)
  - `fix/<issue-id>-<descripcion-corta>`
  - `chore/<issue-id>-<descripcion-corta>`
  - `docs/<issue-id>-<descripcion-corta>`
- **Conventional Commits**:
  - `feat: agregar endpoint de arqueo de caja`
  - `fix: corregir redondeo en tabla de amortización francesa`
  - `docs: actualizar diagrama c4 de microservicio de pagos`
  - `test: agregar pruebas unitarias de cálculo de débitos`

---

## 5. Reglas de Oro Bancarias (No Negociables)

1. **PROHIBIDO EL USO DE DATOS REALES**:
   - Ninguna cédula, cuenta bancaria, nombre, tarjeta, clave o correo real debe figurar en código, semillas, tests ni documentación.
   - Usar generadores de datos ficticios evidentes (ej. cédulas que cumplan dígito verificador pero con números de prueba).
2. **CERO SECRETOS EN EL REPOSITORIO**:
   - Jamás commitear credenciales, connection strings reales, llaves JWT privadas ni API keys. Usar variables de entorno y validar con `.gitleaks.toml`.
3. **ARQUITECTURA DE CAJERO Y ARQUEO INMUTABLE**:
   - Todo movimiento en caja (ingreso o egreso) debe registrarse en un asiento contable/transaccional inmutable (Append-only). No existe `UPDATE` ni `DELETE` sobre registros contables o transacciones ejecutadas.
4. **DECISIONES DE DISEÑO EN ADRs**:
   - Cualquier cambio en la arquitectura o nuevo patrón debe documentarse como un Architecture Decision Record en `docs/adr/`.
5. **ESPECIFICACIONES ANTES DE CODIFICAR**:
   - No implementar módulos complejos sin una especificación aprobada en `docs/specs/`.

---

## 6. Convenciones Técnicas Obligatorias

| Tema | Convención |
|---|---|
| Rutas REST | `/api/v1/<servicio>/<recurso>` (ej. `/api/v1/cajeros/agencias`, `/api/v1/cuentas/socios`) |
| Puertos locales | Gateway 5000 · Auth 5001 · Cajeros 5002 · Cuentas 5003 · Transacciones 5004 · Créditos 5005 · Auditoría 5006 · Notificaciones 5007 · PWA 8081 |
| Puerto en contenedor | 8080 para todos los servicios |
| JWT | HS256, emisor `BancoChillo.Auth`, audiencia `BancoChillo.Clientes`, roles en el claim `role` (códigos de `auth_bc.roles`) |
| Errores HTTP | RFC 7807 ProblemDetails: 400 validación, 401, 403, 404, 409 duplicado, 422 regla de negocio |
| Base de datos | Un esquema `<modulo>_bc` por microservicio, sin FK entre esquemas, scripts idempotentes, dinero `NUMERIC(14,2)` (ADR-0007) |
| Bajas | Lógicas (estado `INACTIVA` / `activa = false`), nunca `DELETE` físico de datos de negocio |
| Health check | `GET /health` público en todos los servicios |

**Ejemplos de referencia (copiar su estructura, no inventar otra):**
- .NET: `src/services/cajeros` (CRUD de agencias: Domain → Application → Infrastructure → Api + pruebas).
- Node.js: `src/services/notificaciones` (CRUD de plantillas: rutas → esquemas zod → repositorio SQL + pruebas).

Guías paso a paso para el equipo: `docs/guias/README.md`.

---

## 7. Instrucciones para Asistentes de IA (Claude, Copilot, Codex, Gemini, etc.)

Antes de proponer o escribir código:
1. Lee este archivo, `docs/guias/README.md`, el **Issue** asignado y su **spec** en `docs/specs/`.
2. Abre el ejemplo de referencia del stack correspondiente y replica su estructura, nombres y estilo.
3. Indica primero, en pocas líneas, qué archivos vas a crear o modificar. Limítate al alcance del Issue.

Reglas:
- Español en nombres, comentarios, mensajes de error, commits y PRs.
- No agregar dependencias nuevas ni cambiar puertos, rutas del Gateway, workflows de `.github/`, `docker-compose.yml`,
  `Directory.Build.props` o `global.json` salvo que el Issue lo pida explícitamente.
- No tocar módulos de otros microservicios. No leer ni escribir tablas de otro esquema.
- Datos siempre ficticios; nunca secretos reales (usar `appsettings.Development.json` / `.env.example` como modelo).
- Todo cambio debe compilar y pasar pruebas: `dotnet build && dotnet test`, o `npm run lint && npm test`.
- Si algo del Issue es ambiguo o contradice este archivo, pregunta en lugar de suponer.
- Los commits y Pull Requests no incluyen firmas ni coautoría de herramientas de IA.
