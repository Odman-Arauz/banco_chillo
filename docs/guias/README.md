# 📚 Guías del equipo — Banco Chillo

Léelas **en orden** la primera vez. Después úsalas como consulta.

| # | Guía | Cuándo usarla |
|---|---|---|
| 01 | [Instalación del entorno](01-instalacion-entorno.md) | Primer día: instalar Git, .NET, Node, Docker, VS Code |
| 02 | [Conectar GitHub por comando](02-conectar-github.md) | Primer día: identidad, `gh auth login`, SSH, clonar |
| 03 | [Levantar el proyecto en local](03-levantar-proyecto-local.md) | Cada vez que empiezas a trabajar |
| 04 | [Errores frecuentes y soluciones](04-errores-frecuentes.md) | Cuando algo falla. **Busca aquí antes de preguntar** |
| 05 | [Flujo Git: ramas, commits y Pull Requests](05-flujo-git-commits-pr.md) | Cada vez que subes código |
| 06 | [Cómo escribir Specs y ADRs](06-specs-y-adrs.md) | Antes de programar un módulo |
| 07 | [Cómo trabajar con IA sin que divague](07-uso-de-ia.md) | Si usas Claude, Copilot, ChatGPT, Gemini… |
| 08 | [Mockups con Google Stitch](08-mockups-google-stitch.md) | Semana 5: diseño de pantallas |
| 09 | [Base de datos: local y Supabase](09-base-de-datos-supabase.md) | Semana 5: tus tablas |
| 10 | [Despliegue y CI/CD](10-despliegue-ci-cd.md) | Para entender el pipeline y publicar |
| 11 | [App de escritorio (.exe)](11-escritorio-exe.md) | Si trabajas en la app WPF del cajero |
| 12 | [Informe de la práctica (PDF)](12-informe-practica.md) | Para armar la entrega de cada semana |

## Mapa rápido del repositorio

```
banco_chillo/
├── AGENTS.md                     ← Reglas del proyecto (léelas; la IA también)
├── docker-compose.yml            ← Postgres, Redis, RabbitMQ (+ perfil "apps" con todos los servicios)
├── render.yaml                   ← Despliegue en Render
├── database/
│   ├── schema/0X-<modulo>/*.sql  ← Tablas de cada microservicio (un esquema por servicio)
│   └── seeds/*.sql               ← Datos ficticios de prueba
├── docs/
│   ├── adr/                      ← Decisiones de arquitectura
│   ├── specs/                    ← Especificaciones por Issue
│   ├── guias/                    ← Estas guías
│   └── planificacion/            ← Qué se entrega cada semana
├── scripts/                      ← verificar-entorno.ps1, publicar-escritorio.ps1
└── src/
    ├── building-blocks/          ← Código .NET compartido (JWT, errores, validación)
    ├── gateway/                  ← API Gateway (YARP) — puerto 5000
    ├── services/
    │   ├── auth/                 ← .NET — 5001
    │   ├── cuentas/              ← .NET — 5003
    │   ├── transacciones/        ← .NET — 5004
    │   ├── cajeros/              ← .NET — 5002   ⭐ EJEMPLO DE REFERENCIA .NET
    │   ├── creditos/             ← .NET — 5005
    │   ├── auditoria/            ← .NET — 5006
    │   └── notificaciones/       ← Node.js — 5007 ⭐ EJEMPLO DE REFERENCIA NODE
    └── apps/
        ├── pwa-banca/            ← React Native / Expo Web (PWA) — 8081
        └── escritorio-cajero/    ← WPF .NET (.exe)
```

## Regla de oro del equipo

> **Copia el patrón del ejemplo de referencia.** Si tu microservicio es .NET, abre `src/services/cajeros`
> y replica la misma estructura para tu entidad. Si es Node.js, abre `src/services/notificaciones`.
> Si algo de tu Issue no encaja con el ejemplo, pregunta en el Issue **antes** de inventar otra estructura.
