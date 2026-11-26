# Arquitectura de Banco Chillo

- **Responsable:** @Ronaldtipan (Issue #30, entregable 3)
- **Nivel:** C4 nivel 2 (contenedores), dibujado en Mermaid
- **Decisiones de referencia:** ADR-0001, ADR-0004, ADR-0005, ADR-0006, ADR-0007 y spec-0002 (gateway)
- **Fecha:** 2026-11-25

GitHub dibuja los diagramas Mermaid automáticamente al abrir este archivo.

---

## 1. Contenedores (C4 nivel 2)

```mermaid
flowchart TB
    socio(["👤 Socio<br/>navegador o celular"])
    cajero(["👤 Cajero<br/>ventanilla de agencia"])

    subgraph clientes["Aplicaciones cliente"]
        pwa["<b>PWA Banca</b><br/>React Native · Expo Web<br/><i>Vercel</i>"]
        escritorio["<b>App de escritorio</b><br/>WPF · .NET 10<br/><i>BancoChillo.Cajero.exe</i>"]
    end

    subgraph backend["Backend · Render (Docker)"]
        gateway["<b>API Gateway</b><br/>.NET 10 · YARP<br/>rutas · rate limit · CORS · timeouts · health"]

        subgraph nucleo["Núcleo bancario · .NET 10 · Clean Architecture + CQRS"]
            auth["<b>Auth</b><br/>JWT · roles"]
            cuentas["<b>Cuentas</b><br/>socios · saldos"]
            transacciones["<b>Transacciones</b><br/>motor con idempotencia"]
            cajeros["<b>Cajeros</b><br/>ventanillas · arqueo"]
            creditos["<b>Créditos</b><br/>simulador · préstamos"]
            auditoria["<b>Auditoría</b><br/>hash encadenado"]
        end

        notificaciones["<b>Notificaciones</b><br/>Node.js · Express"]
    end

    subgraph datos["Datos e infraestructura"]
        postgres[("<b>PostgreSQL 16</b><br/><i>Supabase</i><br/>un esquema por servicio")]
        redis[("<b>Redis</b><br/>caché · <i>fase 3</i>")]
        rabbit[["<b>RabbitMQ</b><br/>eventos · Outbox<br/><i>fase 3</i>"]]
    end

    socio -- HTTPS --> pwa
    cajero -- usa --> escritorio
    pwa -- "HTTPS · JSON · JWT" --> gateway
    escritorio -- "HTTPS · JSON · JWT" --> gateway

    gateway -- "/api/v1/auth" --> auth
    gateway -- "/api/v1/cuentas" --> cuentas
    gateway -- "/api/v1/transacciones" --> transacciones
    gateway -- "/api/v1/cajeros" --> cajeros
    gateway -- "/api/v1/creditos" --> creditos
    gateway -- "/api/v1/auditoria" --> auditoria
    gateway -- "/api/v1/notificaciones" --> notificaciones

    auth & cuentas & transacciones & cajeros & creditos & auditoria & notificaciones --> postgres
    transacciones & cajeros -.->|publica eventos| rabbit
    rabbit -.->|consume| auditoria & notificaciones
    gateway -.->|trabajo futuro| redis

    classDef cliente fill:#E8F1FB,stroke:#2F6FB3,color:#0B2540
    classDef borde fill:#FFF4E0,stroke:#C77700,color:#3D2600
    classDef net fill:#EEF7EE,stroke:#2E7D32,color:#0F2E10
    classDef nodejs fill:#F3EEFA,stroke:#6A3FA0,color:#24133A
    classDef dato fill:#F2F2F2,stroke:#666,color:#222
    class pwa,escritorio cliente
    class gateway borde
    class auth,cuentas,transacciones,cajeros,creditos,auditoria net
    class notificaciones nodejs
    class postgres,redis,rabbit dato
```

**Lectura rápida**

- Los clientes (PWA y escritorio) **solo** conocen la URL del gateway. Ningún microservicio se expone directo (spec-0002, RG-01).
- El gateway **no** valida el JWT: lo reenvía y cada microservicio lo valida (defensa en profundidad).
- Siete microservicios: seis en .NET y uno en Node.js (ADR-0004), todos con el mismo JWT emitido por Auth.
- Una sola base PostgreSQL en Supabase con **un esquema por servicio** y sin consultas entre esquemas (ADR-0007).
- Líneas punteadas = piezas planificadas para la fase 3 (Redis y RabbitMQ con Outbox); hoy existen en Docker pero aún no se usan.

---

## 2. Responsabilidades de cada contenedor

| Contenedor | Tecnología | Puerto local | Esquema BD | Responsabilidad | Responsable S5 |
|---|---|---|---|---|---|
| PWA Banca | React Native · Expo Web | 8081 | — | Interfaz del socio y navegación por rol | S0yRai |
| App de escritorio | WPF · .NET 10 | — | — | Operación de ventanilla del cajero | — |
| API Gateway | .NET 10 · YARP | 5000 | — | Punto único de entrada, rate limit, CORS, timeouts, salud | Ronald |
| Auth | .NET 10 | 5001 | `auth_bc` | Registro, login, emisión de JWT, roles | Odman |
| Cajeros | .NET 10 | 5002 | `cajeros_bc` | Agencias, ventanillas, apertura, operaciones, arqueo y cierre | Jostin · Josué · Mateo |
| Cuentas | .NET 10 | 5003 | `cuentas_bc` | Socios, cuentas y saldos | Mathias |
| Transacciones | .NET 10 | 5004 | `transacciones_bc` | Depósitos, retiros y transferencias con idempotencia | Mathias |
| Créditos | .NET 10 | 5005 | `creditos_bc` | Simulación y gestión de préstamos | Jostin |
| Auditoría | .NET 10 | 5006 | `auditoria_bc` | Registro inmutable con hash encadenado | Josué |
| Notificaciones | Node.js · Express | 5007 | `notificaciones_bc` | Plantillas, preferencias y envíos (correo/SMS/push simulados) | Mateo |

---

## 3. Detalle del API Gateway (spec-0002)

```mermaid
flowchart LR
    cliente(["PWA / escritorio"]) --> fh

    subgraph gw["API Gateway · YARP"]
        direction LR
        fh["1 · ForwardedHeaders<br/>IP real detrás de Render"]
        log["2 · Registro de peticiones<br/>método · ruta · estado · ms"]
        cors["3 · CORS<br/>solo origen de la PWA"]
        rl{"4 · Rate limit<br/>global 100/min por IP<br/>login 10/min por IP"}
        proxy["5 · YARP<br/>ruta → cluster<br/>timeout 30 s"]
        hc["Health checks activos<br/>cada 30 s → /health"]
        fh --> log --> cors --> rl
        rl -- "dentro del límite" --> proxy
    end

    rl -- "excede" --> r429["429 Too Many Requests<br/>Retry-After: 60"]
    proxy -- "servicio caído" --> r502["502 Bad Gateway"]
    proxy -- "servicio lento" --> r504["504 Gateway Timeout"]
    proxy --> servicios["7 microservicios"]
    hc -.-> servicios
```

Endpoints propios del gateway: `GET /health` (lo usa Render), `GET /health/servicios` (estado de los 7 servicios, 3 s máximo por servicio) y `GET /` (lista de rutas).

---

## 4. Flujo de una operación de ventanilla (ejemplo)

```mermaid
sequenceDiagram
    autonumber
    actor C as Cajero
    participant E as App escritorio
    participant G as Gateway
    participant A as Auth
    participant CJ as Cajeros
    participant T as Transacciones
    participant AU as Auditoría

    C->>E: Inicia sesión
    E->>G: POST /api/v1/auth/login
    G->>A: reenvía (límite 10/min por IP)
    A-->>E: 200 · JWT con ROLE_CAJERO
    C->>E: Registra un depósito
    E->>G: POST /api/v1/cajeros/... (Bearer JWT)
    G->>CJ: reenvía
    CJ->>CJ: valida JWT y rol
    CJ->>T: registra el movimiento (idempotente)
    T-->>CJ: confirmado
    CJ-)AU: evento de la operación (fase 3: RabbitMQ)
    CJ-->>E: 201 · comprobante
```

---

## 5. Despliegue (ADR-0005)

```mermaid
flowchart LR
    dev["Desarrollador<br/>rama feat/... → PR"] --> gh["GitHub<br/>develop → main"]
    gh -- "CI: build + test + Gitleaks" --> gh
    gh -- "push a main" --> cd["GitHub Actions<br/>cd-despliegue.yml"]
    cd -- "docker push" --> ghcr[("ghcr.io<br/>imágenes")]
    cd -- "Deploy Hook" --> render["Render<br/>gateway + servicios"]
    gh -- "integración Git" --> vercel["Vercel<br/>PWA"]
    gh -- "tag escritorio-v*" --> rel["GitHub Releases<br/>.exe"]
    render --> supa[("Supabase<br/>PostgreSQL")]
    vercel -- "EXPO_PUBLIC_API_URL" --> render
```

| Pieza | Plataforma | Cómo llega |
|---|---|---|
| Gateway y microservicios | Render (Docker, plan free) | `render.yaml` + Deploy Hooks desde `cd-despliegue.yml` |
| Imágenes Docker | GitHub Container Registry | `cd-despliegue.yml` al llegar a `main` |
| Base de datos | Supabase | Workflow manual `db-supabase.yml` |
| PWA | Vercel | Integración Git (vista previa por PR) |
| App de escritorio | GitHub Releases | `cd-escritorio.yml` con el tag `escritorio-v*` |

En local todo corre con `docker compose --profile apps up -d --build` (gateway + 7 servicios + PostgreSQL, Redis y RabbitMQ).
