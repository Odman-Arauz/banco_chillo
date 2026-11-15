# Tareas de Ejecución: API Gateway Centralizado con YARP y Políticas de Resiliencia

- **Especificación:** [SPEC-0002](./spec.md)
- **Plan:** [plan.md](./plan.md)
- **Issue:** #2
- **Rama:** `feat/2-api-gateway-yarp`

---

## Leyenda

- [x] Completada
- [ ] Pendiente

---

## Fase 1 — Documentación

- [x] **T1:** Crear `docs/specs/spec-0002-api-gateway/` con `spec.md`, `plan.md` y `tasks.md` a partir de la plantilla.
- [x] **T2:** Ampliar la especificación con contexto, glosario, requisitos, diagramas, seguridad, riesgos y definición de terminado.
- [ ] **T3:** Obtener la revisión y aprobación de la especificación por parte del Tech Lead.
- [ ] **T4:** Resolver con el Tech Lead las decisiones pendientes de la sección 14 de `spec.md`:
  - [ ] Puertos definitivos de los microservicios.
  - [ ] URL del frontend PWA (desarrollo y producción).
  - [ ] Uso de Redis para rate limiting distribuido.
  - [ ] Límite especial para `/api/v1/auth/login`.

## Fase 2 — Configuración base del Gateway (`src/gateway/BancoChillo.Gateway/`)

- [ ] **T5:** Agregar paquetes NuGet `Yarp.ReverseProxy` y `Microsoft.Extensions.Http.Resilience`.
  - *Criterio de terminado:* `dotnet build` sin errores.
- [ ] **T6:** Definir en `appsettings.json` las 5 rutas (`auth`, `cajeros`, `cuentas`, `transacciones`, `creditos`) y sus clusters con `ActivityTimeout` de 30 s.
  - *Criterio de terminado:* JSON válido y secciones existentes conservadas.
- [ ] **T7:** Registrar YARP en `Program.cs` con `AddReverseProxy().LoadFromConfig(...)` y `MapReverseProxy()`; eliminar el endpoint de ejemplo.
  - *Criterio de terminado:* `/api/v1/auth/...` llega al servicio de Auth.
  - *Commit:* `feat: configurar yarp reverse proxy`

## Fase 3 — Políticas de seguridad y resiliencia

- [ ] **T8:** Agregar limitador global por IP de 100 peticiones/minuto con respuesta `429`.
  - *Criterio de terminado:* la petición 101 en un minuto recibe `429`.
  - *Commit:* `feat: agregar rate limiting en gateway`
- [ ] **T9:** Configurar CORS restrictivo (`PwaPolicy`) leyendo `Cors:FrontendOrigin` de configuración.
  - *Criterio de terminado:* solo el origen de la PWA recibe `Access-Control-Allow-Origin`.
- [ ] **T10:** Habilitar `UseHttpsRedirection` (terminación TLS).
  - *Criterio de terminado:* HTTP redirige a HTTPS.
- [ ] **T11:** Agregar middleware de registro de peticiones con tiempo de respuesta (sin cuerpos ni tokens).
  - *Criterio de terminado:* cada petición genera una línea `MÉTODO RUTA -> ESTADO en X ms`.
- [ ] **T12:** Verificar el orden de middlewares según la sección 9.4 de `spec.md`.
  - *Commit:* `feat: agregar cors, https y registro de peticiones en gateway`

## Fase 4 — Pruebas

- [ ] **T13:** Crear el proyecto `tests/BancoChillo.Gateway.Tests/` (xUnit + FluentAssertions + `WebApplicationFactory`) y agregarlo a `BancoChillo.sln`.
- [ ] **T14:** Implementar las pruebas de integración PI-01 a PI-07 de `plan.md` (sección 3.1).
  - *Commit:* `test: agregar pruebas de integracion del gateway`
- [ ] **T15:** Ejecutar las pruebas manuales de `plan.md` (sección 3.2) y anotar los resultados en el PR.

## Fase 5 — Calidad y cierre

- [ ] **T16:** Ejecutar `dotnet build`, `dotnet test` y `dotnet format --verify-no-changes`.
- [ ] **T17:** Verificar que no hay secretos ni datos reales (Gitleaks en CI).
- [ ] **T18:** Abrir Pull Request hacia `develop` con `Closes #2` y completar el checklist de la plantilla de PR.
- [ ] **T19:** Atender los comentarios de la revisión.
- [ ] **T20:** Marcar como completo el checklist de la **Definición de Terminado** (`spec.md`, sección 16).

---

## Resumen de avance

| Fase | Tareas | Completadas |
|------|--------|-------------|
| 1. Documentación | 4 | 2 |
| 2. Configuración base | 3 | 0 |
| 3. Seguridad y resiliencia | 5 | 0 |
| 4. Pruebas | 3 | 0 |
| 5. Calidad y cierre | 5 | 0 |
| **Total** | **20** | **2** |
