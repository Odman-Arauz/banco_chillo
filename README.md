# Banco Chillo — Banca Web PWA y Plataforma de Microservicios

[![CI Backend](https://github.com/Odman-Arauz/banco_chillo/actions/workflows/ci-backend.yml/badge.svg)](https://github.com/Odman-Arauz/banco_chillo/actions)
[![CI Frontend PWA](https://github.com/Odman-Arauz/banco_chillo/actions/workflows/ci-frontend-pwa.yml/badge.svg)](https://github.com/Odman-Arauz/banco_chillo/actions)
[![CI Node](https://github.com/Odman-Arauz/banco_chillo/actions/workflows/ci-servicios-node.yml/badge.svg)](https://github.com/Odman-Arauz/banco_chillo/actions)
[![Gitleaks Scan](https://github.com/Odman-Arauz/banco_chillo/actions/workflows/gitleaks.yml/badge.svg)](https://github.com/Odman-Arauz/banco_chillo/actions)

Plataforma bancaria modular orientada a **microservicios** con backend en **.NET**, frontend **PWA con React Native**, módulo operativo de **cajeros y personal administrativo**, y persistencia en **PostgreSQL / Supabase**.

---

## 🏛️ Componentes del Sistema

1. **Banca Web PWA (React Native / Expo Web)**:
   - Portal unificado responsive para clientes/socios (consulta de posición consolidada, transferencias directas e interbancarias, solicitudes de crédito).
   - Portal operativo para Personal Administrativo (ventanilla de cajeros, oficiales de crédito y supervisores).
   - Capacidades PWA: instalación en dispositivos móviles y de escritorio, soporte offline para consultas estáticas.
2. **Ecosistema de Microservicios (híbrido, ver ADR-0004)**: .NET 10 + Clean Architecture + CQRS para el núcleo, Node.js + Express para Notificaciones:
   - `BancoChillo.Gateway`: Enrutador seguro con YARP, rate limiting y terminación TLS.
   - `BancoChillo.Auth`: Servicio de Identidad, OAuth2, JWT de corta duración y autenticación multifactor (MFA/OTP).
   - `BancoChillo.Cuentas`: Gestión de socios, cuentas de ahorro, cuentas corrientes e inversiones.
   - `BancoChillo.Cajeros`: Flujo completo de ventanilla (apertura de caja con fondo fijo, depósitos, retiros con validación, arqueo de billetes/monedas y cierre de jornada).
   - `BancoChillo.Transacciones`: Motor transaccional con idempotencia, débitos/créditos atómicos y transferencias interbancarias.
   - `BancoChillo.Creditos`: Simulador y gestión de préstamos (amortizaciones francesa y alemana).
   - `BancoChillo.Auditoria`: Trazabilidad inmutable de operaciones y eventos de cumplimiento financiero.
   - `notificaciones` (Node.js + Express): Plantillas y envío de notificaciones (correo, SMS, Web Push).
   - `BancoChillo.Escritorio` (WPF): Aplicación `.exe` para estaciones de ventanilla (ver ADR-0006).
3. **Persistencia e Infraestructura**:
   - PostgreSQL 16+ con soporte para despliegue en **Supabase** y políticas de Row Level Security (RLS).
   - Redis para caché de sesión y control de concurrencia.
   - RabbitMQ para mensajería asíncrona y consistencia eventual mediante Outbox Pattern.

---

## 📅 Hoja de Ruta de 17 Semanas

```mermaid
timeline
    title Cronograma de Implementación — Banco Chillo (17 Semanas)
    Fase 1 : Semanas 1 a 4 : Fundación y Arquitectura : Configuración CI/CD, DB Postgres/Supabase, Auth MFA y Gateway
    Fase 2 : Semanas 5 a 8 : Cuentas y Módulo de Cajeros : Gestión de socios, apertura de cuentas, ventanilla de caja y arqueo diario
    Fase 3 : Semanas 9 a 12 : Transacciones y PWA React Native : Transferencias directas/interbancarias, créditos y UI PWA accesible
    Fase 4 : Semanas 13 a 17 : Auditoría, Seguridad y Despliegue : Logs inmutables, notificaciones, pruebas E2E y despliegue a producción
```

| Fase | Semanas | Enfoque Principal | Entregables Clave |
|---|---|---|---|
| **Fase 1** | Semanas 1 - 4 | Fundación, Arquitectura y Auth | Esqueleto monorepo, CI/CD, base de datos PostgreSQL, IAM/Auth con MFA y Gateway YARP |
| **Fase 2** | Semanas 5 - 8 | Cuentas, Socios y Cajeros | Gestión de socios, cuentas de ahorro/corriente, apertura/cierre de cajas, depósitos, retiros y arqueo |
| **Fase 3** | Semanas 9 - 12 | Motor Transaccional y PWA | Transferencias internas y SPI, simulador de créditos, amortizaciones y Frontend PWA en React Native |
| **Fase 4** | Semanas 13 - 17 | Auditoría, Hardening y Despliegue | Microservicio de auditoría inmutable, notificaciones, pruebas de carga/seguridad y despliegue en Supabase/Cloud |

---

## 🚀 Inicio Rápido en Desarrollo Local

> ¿Primera vez? Sigue las guías **en orden**: [docs/guias/README.md](docs/guias/README.md)
> (instalación, conexión con GitHub, levantar el proyecto, errores frecuentes, flujo Git, uso de IA, despliegue).

```powershell
# 1. Verificar herramientas (Git, gh, .NET 10, Node 22, Docker)
.\scripts\verificar-entorno.ps1

# 2. Infraestructura (PostgreSQL con esquemas y semillas, Redis, RabbitMQ)
docker compose up -d

# 3. Backend .NET
dotnet build
dotnet test
dotnet run --project src/services/auth/BancoChillo.Auth.Api        # :5001
dotnet run --project src/services/cajeros/BancoChillo.Cajeros.Api  # :5002  (ejemplo de referencia)
dotnet run --project src/gateway/BancoChillo.Gateway               # :5000

# 4. Microservicio Node.js (ejemplo de referencia)
cd src/services/notificaciones; Copy-Item .env.example .env; npm install; npm run dev   # :5007

# 5. PWA
cd src/apps/pwa-banca; npm install; npm run web                     # :8081
```

Todo el sistema en contenedores: `docker compose --profile apps up -d --build`.

---

## 📚 Documentación

- [docs/guias/](docs/guias/README.md): **Guías paso a paso del equipo** (empieza aquí).
- [docs/planificacion/](docs/planificacion/): Entregables y responsables por semana.
- [docs/adr/](docs/adr/): Decisiones de Arquitectura de Software (ADR-0001 a ADR-0007).
- [docs/business-rules/](docs/business-rules/): Catálogo formal de Reglas de Negocio (RN-00X).
- [docs/specs/](docs/specs/): Especificaciones funcionales y planes de ejecución.
- [AGENTS.md](AGENTS.md): Reglas del proyecto para personas y asistentes de IA.
- [CONTRIBUTING.md](CONTRIBUTING.md): Flujo de ramas y estándares de código.
- [SECURITY.md](SECURITY.md): Políticas de reporte de vulnerabilidades.