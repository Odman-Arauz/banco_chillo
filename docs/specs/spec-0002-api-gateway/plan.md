# Plan de Implementación: API Gateway Centralizado con YARP y Políticas de Resiliencia

- **Especificación:** SPEC-0002
- **Issue:** #2
- **Estimación:** 1–2 días

---

## 1. Arquitectura y Capas Afectadas

El Gateway no es un microservicio de dominio: no tiene capas `Domain`, `Application` ni `Infrastructure`. Solo se modifica el proyecto `src/gateway/BancoChillo.Gateway/`.

- [ ] `Domain`: No aplica.
- [ ] `Application`: No aplica.
- [ ] `Infrastructure`: No aplica.
- [x] `Api` (Gateway): configuración de YARP, middlewares de rate limiting, CORS, HTTPS y registro de peticiones.
- [ ] `Frontend PWA`: No aplica en esta especificación (la PWA solo deberá apuntar su URL base al Gateway).

### 1.1 Archivos a modificar

| Archivo | Cambio |
|---------|--------|
| `src/gateway/BancoChillo.Gateway/BancoChillo.Gateway.csproj` | Agregar paquetes `Yarp.ReverseProxy` y `Microsoft.Extensions.Http.Resilience`. |
| `src/gateway/BancoChillo.Gateway/appsettings.json` | Agregar secciones `ReverseProxy` (5 rutas y 5 clusters con `ActivityTimeout`) y `Cors:FrontendOrigin`. Conservar `Logging` y `AllowedHosts`. |
| `src/gateway/BancoChillo.Gateway/appsettings.Development.json` | (Opcional) Valores de desarrollo: origen local de la PWA y puertos locales. |
| `src/gateway/BancoChillo.Gateway/Program.cs` | Reemplazar el `MapGet("/")` de ejemplo por: `AddReverseProxy`, `AddRateLimiter`, `AddCors`, `UseHttpsRedirection`, middleware de tiempos, `UseCors`, `UseRateLimiter`, `MapReverseProxy`. |

### 1.2 Pasos de implementación

1. Crear la rama `feat/2-api-gateway-yarp` desde `develop`.
2. Instalar paquetes NuGet:
   ```bash
   dotnet add src/gateway/BancoChillo.Gateway package Yarp.ReverseProxy
   dotnet add src/gateway/BancoChillo.Gateway package Microsoft.Extensions.Http.Resilience
   ```
3. Configurar rutas y clusters en `appsettings.json` (ver tabla 5.1 de `spec.md`).
4. Registrar YARP en `Program.cs` y validar el reenvío.
   Commit: `feat: configurar yarp reverse proxy`
5. Agregar el limitador global por IP (100 peticiones/minuto, respuesta 429), CORS restrictivo y registro de tiempos de respuesta.
   Commit: `feat: agregar rate limiting en gateway`
6. Ejecutar `dotnet build` y `dotnet format`.
7. Abrir Pull Request hacia `develop` con `Closes #2`.

### 1.3 Dependencias y riesgos

- **Puertos internos:** resolver la discrepancia de puertos descrita en la sección 6 de `spec.md` antes de implementar.
- **Rate limiting en memoria:** válido para una instancia; con varias instancias se requiere Redis.
- **CORS:** si el origen de la PWA está mal configurado, el frontend recibirá errores de CORS en el navegador.

---

## 2. Estrategia de Pruebas

- **Pruebas de integración** (xUnit + `WebApplicationFactory`):
  - Una petición a `/api/v1/auth/login` se reenvía al cluster `auth-service` (usando un servicio de destino simulado).
  - La petición número 101 de la misma IP dentro de un minuto recibe `429`.
  - Una ruta no registrada recibe `404`.
- **Pruebas manuales:**
  - Levantar Auth y Gateway; llamar a `https://localhost:<puerto-gateway>/api/v1/auth/login` y verificar la respuesta de Auth.
  - Enviar 105 peticiones seguidas y verificar que desde la 101 se recibe `429`:
    ```bash
    for i in $(seq 1 105); do curl -k -s -o /dev/null -w "%{http_code} " https://localhost:<puerto-gateway>/api/v1/auth/login; done
    ```
  - Detener un microservicio y verificar `502`; simular lentitud y verificar `504`.
  - Revisar en la consola que cada petición registra método, ruta, código y milisegundos, sin cuerpos ni tokens.
- **Pruebas E2E / UI en frontend:** No aplica en esta especificación.
