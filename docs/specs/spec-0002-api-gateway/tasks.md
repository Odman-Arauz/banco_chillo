# Tareas de Ejecución: API Gateway Centralizado con YARP y Políticas de Resiliencia

- **Especificación:** SPEC-0002
- **Issue:** #2

---

## Tareas

### Documentación
- [x] **T1:** Crear `docs/specs/spec-0002-api-gateway/` con `spec.md`, `plan.md` y `tasks.md` a partir de la plantilla.
- [ ] **T2:** Resolver con el Tech Lead las decisiones pendientes (puertos internos, origen de la PWA, Redis) de la sección 6 de `spec.md`.

### Implementación (`src/gateway/BancoChillo.Gateway/`)
- [ ] **T3:** Agregar paquetes NuGet `Yarp.ReverseProxy` y `Microsoft.Extensions.Http.Resilience`.
- [ ] **T4:** Definir en `appsettings.json` las 5 rutas (`auth`, `cajeros`, `cuentas`, `transacciones`, `creditos`) y sus clusters con `ActivityTimeout`.
- [ ] **T5:** Registrar YARP en `Program.cs` con `AddReverseProxy().LoadFromConfig(...)` y `MapReverseProxy()`. Commit `feat: configurar yarp reverse proxy`.
- [ ] **T6:** Agregar limitador global por IP de 100 peticiones/minuto con respuesta `429`. Commit `feat: agregar rate limiting en gateway`.
- [ ] **T7:** Configurar CORS restrictivo (`PwaPolicy`) leyendo `Cors:FrontendOrigin` de configuración.
- [ ] **T8:** Habilitar `UseHttpsRedirection` (terminación TLS).
- [ ] **T9:** Agregar middleware de registro de peticiones con tiempo de respuesta (sin cuerpos ni tokens).

### Pruebas y cierre
- [ ] **T10:** Escribir pruebas de integración (reenvío a Auth, 429 en la petición 101, 404 en ruta inexistente).
- [ ] **T11:** Verificación manual de los criterios de aceptación de `spec.md`.
- [ ] **T12:** Ejecutar `dotnet build`, `dotnet test` y `dotnet format`.
- [ ] **T13:** Abrir Pull Request hacia `develop` con `Closes #2` y completar el checklist de la plantilla de PR.
