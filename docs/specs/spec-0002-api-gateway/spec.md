# Especificación: API Gateway Centralizado con YARP y Políticas de Resiliencia

- **ID:** SPEC-0002
- **Módulo:** Gateway (`src/gateway/BancoChillo.Gateway`)
- **Estado:** En Revisión
- **Fecha:** 2026-11-12
- **Issue:** #2
- **ADR de referencia:** [ADR-0001 — Arquitectura de Microservicios con Clean Architecture y CQRS](../../adr/ADR-0001-arquitectura-microservicios-clean-cqrs.md)

---

## 1. Propósito y Alcance

Banco Chillo expone varios microservicios internos (Auth, Cajeros, Cuentas, Transacciones y Créditos), cada uno en su propio puerto. El frontend PWA no debe conocer ni conectarse directamente a esos servicios.

Esta especificación define un **API Gateway** como **punto único de entrada seguro** que:

1. Encamina el tráfico del frontend PWA hacia el microservicio correspondiente según la ruta (`/api/v1/<modulo>/...`), sin exponer puertos ni direcciones internas.
2. Aplica **limitación de velocidad (rate limiting)**: máximo 100 peticiones por minuto por dirección IP, para mitigar ataques de fuerza bruta y abuso.
3. Aplica **tiempos de espera (timeouts)** hacia los servicios internos para que un servicio lento no bloquee al Gateway.
4. Realiza la **terminación TLS**: el tráfico externo llega por HTTPS y el Gateway lo descifra antes de reenviarlo a la red interna.
5. Restringe **CORS** para aceptar únicamente el origen del frontend PWA.
6. Registra cada petición con su **tiempo de respuesta** para observabilidad.

### Dentro del alcance
- Configuración de YARP (rutas y clusters) en `appsettings.json`.
- Middlewares de rate limiting, CORS, HTTPS y registro de peticiones en `Program.cs`.
- Política de timeout por cluster.

### Fuera del alcance
- Autenticación/validación de JWT en el Gateway (se abordará en una especificación posterior junto al servicio de Auth).
- Rate limiting distribuido con Redis (ver sección 6, decisiones pendientes).
- Balanceo de carga entre múltiples instancias de un mismo servicio.
- Exposición del servicio de Auditoría (es interno y no lo consume la PWA).

---

## 2. Historias de Usuario Asociadas

- **US-01:** Como sistema bancario, quiero un punto único de entrada seguro (API Gateway) para encaminar el tráfico del frontend PWA hacia los microservicios internos, aplicando políticas de limitación de velocidad, tiempo de espera y terminación TLS.

---

## 3. Reglas de Negocio Aplicables

- **RG-01 — Punto único de entrada:** el frontend PWA solo se comunica con el Gateway. Ningún microservicio interno se expone directamente al cliente.
- **RG-02 — Límite de peticiones:** cada dirección IP puede realizar como máximo **100 peticiones en una ventana de 1 minuto**. Al superarlo, el Gateway responde `429 Too Many Requests` sin reenviar la petición.
- **RG-03 — Origen permitido:** solo el origen configurado del frontend PWA puede realizar peticiones desde navegador (CORS).
- **RG-04 — Cero secretos:** las URLs, orígenes y límites se leen de configuración / variables de entorno; no se incluyen credenciales en el repositorio (regla de oro N.º 2 de `AGENTS.md`).
- **RG-05 — Sin datos reales:** los registros (logs) del Gateway no deben incluir cuerpos de petición, contraseñas ni tokens; solo método, ruta, código de estado y tiempo de respuesta.

---

## 4. Criterios de Aceptación (Gherkin)

```gherkin
Característica: API Gateway centralizado

  Escenario: Redirección transparente al microservicio de Auth
    Dado una petición dirigida a "/api/v1/auth/login" recibida en el Gateway
    Cuando se procesa la ruta
    Entonces es redirigida transparentemente al microservicio de Auth
    Y se devuelve la respuesta sin exponer puertos internos

  Escenario: Límite de peticiones superado
    Dado un cliente que envía más de 100 peticiones en menos de un minuto
    Cuando supera el límite
    Entonces el Gateway responde con HTTP 429 Too Many Requests

  Escenario: Ruta no registrada
    Dado una petición dirigida a "/api/v1/inexistente/recurso"
    Cuando se procesa la ruta
    Entonces el Gateway responde con HTTP 404 Not Found

  Escenario: Microservicio no disponible o lento
    Dado que el microservicio de Cuentas no responde dentro del tiempo de espera configurado
    Cuando el Gateway reenvía una petición a "/api/v1/cuentas/..."
    Entonces el Gateway responde con HTTP 504 Gateway Timeout (o 502 Bad Gateway si el servicio está caído)
    Y no queda bloqueado para las demás rutas

  Escenario: Origen no permitido (CORS)
    Dado una petición de navegador cuyo origen no es el frontend PWA
    Cuando llega al Gateway
    Entonces la respuesta no incluye la cabecera "Access-Control-Allow-Origin"
    Y el navegador bloquea la respuesta
```

---

## 5. Diseño Técnico y Endpoints

### 5.1 Tabla de enrutamiento

| Ruta pública (Gateway)       | Ruta YARP                                  | Cluster                 | Destino (según issue #2) |
|------------------------------|--------------------------------------------|-------------------------|--------------------------|
| `/api/v1/auth/**`            | `/api/v1/auth/{**catch-all}`               | `auth-service`          | `http://localhost:5001`  |
| `/api/v1/cajeros/**`         | `/api/v1/cajeros/{**catch-all}`            | `cajeros-service`       | `http://localhost:5002`  |
| `/api/v1/cuentas/**`         | `/api/v1/cuentas/{**catch-all}`            | `cuentas-service`       | `http://localhost:5003`  |
| `/api/v1/transacciones/**`   | `/api/v1/transacciones/{**catch-all}`      | `transacciones-service` | `http://localhost:5004`  |
| `/api/v1/creditos/**`        | `/api/v1/creditos/{**catch-all}`           | `creditos-service`      | `http://localhost:5005`  |

La ruta se reenvía **sin transformar** (el microservicio recibe la misma ruta `/api/v1/...`).

### 5.2 Componentes

- **Reverse proxy:** `Yarp.ReverseProxy`, cargado con
  `builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))`.
- **Resiliencia / timeout:** `HttpRequest.ActivityTimeout` por cluster en YARP (valor propuesto: 30 s). Se agrega `Microsoft.Extensions.Http.Resilience` (Polly) como dependencia según el stack de `AGENTS.md`, para políticas adicionales en futuras iteraciones.
- **Rate limiting:** `Microsoft.AspNetCore.RateLimiting` (incluido en ASP.NET Core), limitador global de **ventana fija** particionado por IP remota: `PermitLimit = 100`, `Window = 1 min`, `QueueLimit = 0`, `RejectionStatusCode = 429`.
- **CORS:** política `PwaPolicy` con `WithOrigins(<origen PWA desde configuración>)`.
- **TLS:** `UseHttpsRedirection()` y perfil HTTPS de Kestrel; los servicios internos se consumen por HTTP dentro de la red privada.
- **Observabilidad:** middleware que mide con `Stopwatch` y registra `método, ruta, código de estado, milisegundos`.

### 5.3 Orden de middlewares

1. `UseHttpsRedirection`
2. Registro de peticiones con tiempo de respuesta (primero, para que también registre los 429)
3. `UseCors("PwaPolicy")` (antes del rate limiter, para que la PWA pueda leer el 429)
4. `UseRateLimiter`
5. `MapReverseProxy`

### 5.4 Códigos de estado HTTP

| Código | Origen |
|--------|--------|
| 2xx / 4xx / 5xx | Respuesta del microservicio, devuelta tal cual |
| 404 | Ruta no registrada en el Gateway |
| 429 | Límite de 100 peticiones/minuto por IP superado |
| 502 | Microservicio caído o inalcanzable |
| 504 | Microservicio no respondió dentro del timeout |

### 5.5 Configuración (claves en `appsettings.json`)

- `ReverseProxy:Routes` y `ReverseProxy:Clusters` — tabla 5.1.
- `Cors:FrontendOrigin` — URL del frontend PWA.

---

## 6. Decisiones Pendientes / Preguntas para el Tech Lead

1. **Puertos de los microservicios:** el issue #2 indica `5001`–`5005`, pero los `launchSettings.json` actuales usan otros puertos (Auth `5011`, Cajeros `5134`, Cuentas `5018`, Transacciones `5050`, Créditos `5195`). ¿Se ajustan los `launchSettings.json` a `5001`–`5005` o el Gateway apunta a los puertos actuales?
2. **Origen del frontend PWA:** ¿cuál es la URL/puerto oficial de la PWA en desarrollo (por ejemplo, el puerto por defecto de Expo Web) y en producción?
3. **Rate limiting distribuido:** el limitador en memoria funciona por instancia. `AGENTS.md` menciona Redis para rate limiting; ¿se migra a Redis cuando haya más de una instancia del Gateway?
4. **IP detrás de proxy/balanceador:** si el Gateway se despliega detrás de otro proxy, habrá que configurar `ForwardedHeaders` para obtener la IP real del cliente.

---

## 7. ADR

YARP y Polly ya forman parte del stack aprobado en `AGENTS.md` y siguen la arquitectura de ADR-0001, por lo que **no se requiere un ADR nuevo**.
