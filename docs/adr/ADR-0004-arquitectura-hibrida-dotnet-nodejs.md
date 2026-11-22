# ADR-0004: Arquitectura híbrida — microservicios .NET y Node.js/Express

- **Estado:** Aceptado
- **Fecha:** 2026-11-22
- **Autores:** Docente / Tech Lead
- **Decisores:** Tech Lead

---

## 1. Contexto y Planteamiento del Problema

La Práctica Experimental N° 1 de Sistemas Web (S4P_SISW_PE-01) exige que los estudiantes:

- desarrollen microservicios y APIs REST con **Node.js y Express** (Partes 2 a 15),
- implementen un CRUD completo (crear, leer, actualizar, eliminar),
- protejan la API con **JWT**, validación y middleware de seguridad,
- empaqueten los servicios con **Docker** y automaticen con **CI/CD**.

Banco Chillo ya definió (ADR-0001) un backend en **.NET con Clean Architecture y CQRS**, que es más adecuado
para el núcleo transaccional (tipado fuerte, decimal exacto, EF Core) pero no cubre el requisito de Node.js.

---

## 2. Opciones Consideradas

- **Opción 1 — Todo .NET:** se justifica la equivalencia con Express. Riesgo: la rúbrica evalúa Node.js de forma explícita.
- **Opción 2 — Todo Node.js:** cumple la rúbrica, pero se pierde el trabajo ya hecho y la solidez del núcleo bancario.
- **Opción 3 — Híbrida:** núcleo bancario en .NET y **un** microservicio periférico en Node.js + Express, todos detrás del mismo Gateway y con el mismo JWT.

---

## 3. Decisión

Elegimos la **Opción 3 (híbrida)**:

| Microservicio | Tecnología | Motivo |
|---|---|---|
| Gateway | .NET + YARP | Punto único de entrada, rate limiting |
| Auth | .NET | Emite los JWT que usan todos |
| Cuentas, Transacciones, Cajeros, Créditos | .NET (Clean Architecture + CQRS) | Núcleo financiero: precisión decimal, transacciones ACID |
| Auditoría | .NET (Clean Architecture) | Su spec-0009 ya fue aprobada en .NET; comparte EF Core y el manejo de JSONB con el núcleo |
| **Notificaciones** | **Node.js + Express** | Servicio de integración (correo/SMS/push), poco acoplado al núcleo; cubre las Partes 2 y 11–15 del práctico |

Reglas de interoperabilidad (obligatorias para que el sistema funcione como uno solo):

1. **Mismo JWT:** HS256, emisor `BancoChillo.Auth`, audiencia `BancoChillo.Clientes`, roles en el claim `role`.
   .NET lo lee de `Jwt__Secreto` y Node.js de `JWT_SECRETO` (el mismo valor).
2. **Mismo formato de error:** RFC 7807 (ProblemDetails: `status`, `title`, `detail`, `instance`, `errores`).
3. **Mismas convenciones REST:** rutas `/api/v1/<servicio>/<recurso>`, `GET /health` público, baja lógica en lugar de `DELETE` físico.
4. **Cada servicio con su propio esquema** de PostgreSQL (ver ADR-0007).

Ejemplos de referencia que todos deben copiar:

- .NET: `src/services/cajeros` (CRUD de agencias).
- Node.js: `src/services/notificaciones` (CRUD de plantillas).

---

## 4. Consecuencias

### Positivas
- Se cumple literalmente la práctica (Node.js + Express + JWT + Docker + CI/CD) sin abandonar .NET.
- Los estudiantes aprenden a integrar servicios heterogéneos, que es lo habitual en la industria.

### Negativas / Trade-offs
- Dos stacks que mantener (dos pipelines de CI, dos estilos de pruebas). Se limita a **un solo** servicio Node para acotar ese costo.
- Solo un estudiante programa en Node; el resto evidencia las Partes 2 y 11 levantando y consumiendo Notificaciones (guía 12).
- Mitigación: plantillas de referencia, convenciones comunes y guías en `docs/guias/`.
