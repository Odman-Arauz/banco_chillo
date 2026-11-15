# Plan de Implementación: API Gateway Centralizado con YARP y Políticas de Resiliencia

- **Especificación:** [SPEC-0002](./spec.md)
- **Issue:** #2
- **Rama:** `feat/2-api-gateway-yarp` (creada desde `develop`)
- **Estimación:** 1–2 días (ver desglose en la sección 4)

---

## 1. Arquitectura y Capas Afectadas

El Gateway no es un microservicio de dominio: no contiene reglas de negocio bancarias, por lo que **no tiene** capas `Domain`, `Application` ni `Infrastructure`. Solo se modifica el proyecto `src/gateway/BancoChillo.Gateway/`, que cumple el rol de capa de entrada (`Api`) de todo el sistema.

- [ ] `Domain`: No aplica.
- [ ] `Application`: No aplica.
- [ ] `Infrastructure`: No aplica.
- [x] `Api` (Gateway): configuración de YARP, middlewares de rate limiting, CORS, HTTPS y registro de peticiones.
- [ ] `Frontend PWA`: No aplica en esta especificación (la PWA solo deberá apuntar su URL base al Gateway en una tarea posterior).

### 1.1 Archivos a modificar

| Archivo | Cambio |
|---------|--------|
| `src/gateway/BancoChillo.Gateway/BancoChillo.Gateway.csproj` | Agregar paquetes `Yarp.ReverseProxy` y `Microsoft.Extensions.Http.Resilience`. |
| `src/gateway/BancoChillo.Gateway/appsettings.json` | Agregar secciones `ReverseProxy` (5 rutas y 5 clusters con `ActivityTimeout`) y `Cors:FrontendOrigin`. Conservar `Logging` y `AllowedHosts`. |
| `src/gateway/BancoChillo.Gateway/appsettings.Development.json` | (Opcional) Valores de desarrollo: origen local de la PWA y puertos locales. |
| `src/gateway/BancoChillo.Gateway/Program.cs` | Reemplazar el `MapGet("/")` de ejemplo por: `AddReverseProxy`, `AddRateLimiter`, `AddCors`, `UseHttpsRedirection`, middleware de tiempos, `UseCors`, `UseRateLimiter`, `MapReverseProxy`. |
| `tests/BancoChillo.Gateway.Tests/` (nuevo) | Proyecto de pruebas de integración con xUnit, FluentAssertions y `WebApplicationFactory`. |

### 1.2 Archivos que NO se modifican

- Ningún microservicio (`src/services/**`) se modifica en esta especificación, salvo que el Tech Lead decida alinear los puertos de `launchSettings.json` (ver `spec.md`, sección 14).
- No se modifica `docker-compose.yml`.

---

## 2. Pasos de Implementación

### Paso 1 — Preparar la rama
```bash
git checkout develop
git pull origin develop
git checkout feat/2-api-gateway-yarp
git merge develop          # traer cambios recientes de develop, si los hay
```

### Paso 2 — Instalar paquetes NuGet
```bash
dotnet add src/gateway/BancoChillo.Gateway package Yarp.ReverseProxy
dotnet add src/gateway/BancoChillo.Gateway package Microsoft.Extensions.Http.Resilience
dotnet build src/gateway/BancoChillo.Gateway
```
**Resultado esperado:** el proyecto compila sin errores.

### Paso 3 — Configurar rutas y clusters
En `appsettings.json` agregar la sección `ReverseProxy` con las 5 rutas y 5 clusters de la tabla 9.1 de `spec.md`, y la sección `Cors:FrontendOrigin`.
**Resultado esperado:** el archivo es JSON válido y conserva las secciones `Logging` y `AllowedHosts` existentes.

### Paso 4 — Registrar YARP en `Program.cs`
```csharp
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
// ...
app.MapReverseProxy();
```
**Resultado esperado:** una petición a `/api/v1/auth/...` en el Gateway llega al servicio de Auth.
**Commit:** `feat: configurar yarp reverse proxy`

### Paso 5 — Agregar rate limiting
Limitador global de ventana fija particionado por IP: `PermitLimit = 100`, `Window = 1 minuto`, `QueueLimit = 0`, `RejectionStatusCode = 429`. Registrar `app.UseRateLimiter()`.
**Resultado esperado:** la petición 101 en un minuto desde la misma IP recibe `429`.
**Commit:** `feat: agregar rate limiting en gateway`

### Paso 6 — Agregar CORS, HTTPS y registro de peticiones
- Política `PwaPolicy` con el origen leído de `Cors:FrontendOrigin`.
- `app.UseHttpsRedirection()`.
- Middleware con `Stopwatch` que registre método, ruta, estado y milisegundos.
- Respetar el orden de middlewares de la sección 9.4 de `spec.md`.
**Commit:** `feat: agregar cors, https y registro de peticiones en gateway`

### Paso 7 — Pruebas de integración
Crear `tests/BancoChillo.Gateway.Tests/` y agregar las pruebas de la sección 3.
**Commit:** `test: agregar pruebas de integracion del gateway`

### Paso 8 — Verificación final
```bash
dotnet build
dotnet test
dotnet format --verify-no-changes
```

### Paso 9 — Pull Request
- Subir la rama: `git push origin feat/2-api-gateway-yarp`.
- Abrir PR hacia **`develop`**, completar la plantilla y escribir `Closes #2`.
- Solicitar revisión al Tech Lead. **No fusionar sin aprobación.**

---

## 3. Estrategia de Pruebas

### 3.1 Pruebas de integración automatizadas (xUnit + `WebApplicationFactory`)

| ID | Caso de prueba | Datos de entrada | Resultado esperado |
|----|----------------|------------------|--------------------|
| PI-01 | Reenvío a Auth | `POST /api/v1/auth/login` con destino simulado | La petición llega al destino simulado y se devuelve su respuesta. |
| PI-02 | Reenvío a cada servicio | Una petición por cada ruta de la tabla 9.1 | Cada una llega a su cluster correcto. |
| PI-03 | Límite de peticiones | 101 peticiones seguidas desde la misma IP | Las 100 primeras no son `429`; la 101 es `429`. |
| PI-04 | Ruta inexistente | `GET /api/v1/inexistente` | `404 Not Found`. |
| PI-05 | CORS permitido | Cabecera `Origin` igual al origen de la PWA | La respuesta incluye `Access-Control-Allow-Origin`. |
| PI-06 | CORS rechazado | Cabecera `Origin: https://sitio-no-permitido.test` | La respuesta no incluye `Access-Control-Allow-Origin`. |
| PI-07 | Servicio caído | Destino configurado a un puerto sin servicio | `502 Bad Gateway`. |

> Los datos de prueba son ficticios (usuarios y claves de ejemplo), conforme a la regla de oro N.º 1 de `AGENTS.md`.

### 3.2 Pruebas manuales

1. **Reenvío:** levantar Auth y Gateway; llamar a `https://localhost:<puerto-gateway>/api/v1/auth/login` y verificar que responde Auth.
2. **Rate limiting:** enviar 105 peticiones seguidas y verificar que desde la 101 se recibe `429`:
   ```bash
   for i in $(seq 1 105); do curl -k -s -o /dev/null -w "%{http_code} " https://localhost:<puerto-gateway>/api/v1/auth/login; done
   ```
   En PowerShell:
   ```powershell
   1..105 | ForEach-Object { (Invoke-WebRequest -Uri "https://localhost:<puerto-gateway>/api/v1/auth/login" -SkipCertificateCheck -SkipHttpErrorCheck).StatusCode }
   ```
3. **Reinicio del límite:** esperar un minuto y comprobar que se vuelve a aceptar peticiones.
4. **Servicio caído:** detener un microservicio y verificar `502`; las demás rutas siguen funcionando.
5. **Servicio lento:** simular una demora mayor a 30 s y verificar `504`.
6. **Logs:** revisar en la consola que cada petición registra método, ruta, código y milisegundos, sin cuerpos ni tokens.

### 3.3 Pruebas E2E / UI en frontend
No aplica en esta especificación.

---

## 4. Estimación por Tarea

| Tarea | Estimación |
|-------|------------|
| Instalar paquetes y configurar `appsettings.json` | 1 h |
| Registrar YARP y validar reenvío | 1 h |
| Rate limiting | 1 h |
| CORS, HTTPS y logs | 1–2 h |
| Pruebas de integración | 3–4 h |
| Pruebas manuales y correcciones | 2 h |
| PR y ajustes por revisión | 1–2 h |
| **Total** | **10–13 h (≈ 1,5 días)** |

---

## 5. Dependencias y Riesgos

- **Puertos internos:** resolver la discrepancia de puertos descrita en la sección 14 de `spec.md` antes de implementar.
- **Origen de la PWA:** se necesita la URL de la PWA para configurar CORS.
- **Rate limiting en memoria:** válido para una instancia; con varias instancias se requiere Redis.
- **CORS:** si el origen de la PWA está mal configurado, el frontend recibirá errores de CORS en el navegador.
- **Microservicios sin endpoints:** los servicios actuales son plantillas; para probar el reenvío puede usarse un destino simulado o un endpoint temporal de prueba.

---

## 6. Plan de Reversión (Rollback)

Si tras la integración en `develop` se detecta un problema grave:

1. Revertir el merge del PR desde GitHub (botón **Revert**) o con `git revert <commit-del-merge>`.
2. El Gateway vuelve a su estado anterior (endpoint de ejemplo), sin afectar a los microservicios, porque estos no se modifican en esta especificación.
3. Registrar el problema en un issue nuevo y corregirlo en una rama `fix/2-...`.

---

## 7. Criterios para Considerar el Plan Completado

- Todos los pasos de la sección 2 ejecutados.
- Todas las pruebas de la sección 3.1 en verde.
- Checklist de la **Definición de Terminado** (`spec.md`, sección 16) completo.
