# Plan de Implementación: Arqueo de Caja por Denominaciones y Cierre Diario de Jornada

- **Especificación:** SPEC-0005
- **Issue:** #5
- **Estimación preliminar:** 3 días / 18 horas; validar con el equipo al conocer la estructura actual del servicio.
- **Alcance de este PR:** Documentación únicamente. Este plan no implica que las capas estén implementadas.

---

## 1. Objetivo del plan

Describir las etapas técnicas necesarias para implementar el arqueo y cierre de jornada siguiendo las reglas RN-001.4, la inmutabilidad definida en ADR-0003 y las convenciones existentes del repositorio. Antes de iniciar la implementación, contrastar las rutas, nombres de tablas, tecnologías y mecanismos de mensajería propuestos con el código y arquitectura actuales.

---

## 2. Arquitectura y componentes afectados

- [ ] **Domain:** Entidad `ArqueoCaja`, líneas `ArqueoDenominacion`, estados del arqueo, invariantes de cantidad/denominación y cálculo de total/diferencia.
- [ ] **Application:** Comando/handler de cierre, consulta de resumen, validadores, autorización y políticas de idempotencia.
- [ ] **Infrastructure:** Persistencia del arqueo y sus detalles, transacción atómica, control de concurrencia, mapeos y publicación confiable del evento.
- [ ] **API:** Endpoint POST de cierre, endpoint GET de resumen, contratos, documentación OpenAPI y códigos de error.
- [ ] **Frontend PWA:** Formulario de conteo, agrupación por tipo/denominación, previsualización del total y presentación del resultado; UX accesible para justificar diferencias.
- [ ] **Observabilidad:** Logs estructurados, correlación de solicitudes, métricas/alertas sin exponer datos sensibles.
- [ ] **Operación:** Configuración y monitoreo de cola/eventos de cierre, reintentos y manejo de mensajes fallidos, si aplica al patrón actual.

---

## 3. Secuencia propuesta

### Fase 0 — Confirmación de arquitectura y reglas

1. Revisar la estructura actual del servicio Cajeros, sus entidades y ciclo de vida de sesión.
2. Identificar fuentes oficiales de movimientos y cómo se calculan depósitos, retiros y otros movimientos de efectivo.
3. Confirmar política de autorización, visado de supervisión, transacciones, Outbox/eventos y respuestas HTTP.
4. Resolver las preguntas abiertas de SPEC-0005 antes de fijar contratos.

### Fase 1 — Dominio

1. Definir catálogo de denominaciones y validación de tipo/valor.
2. Modelar conteo con cantidades no negativas e integrales y montos decimales.
3. Definir reglas de total físico, diferencia y estados.
4. Definir invariantes: un cierre por sesión, justificación obligatoria para descuadres e inmutabilidad del registro confirmado.
5. Añadir pruebas unitarias puras para cálculos e invariantes.

### Fase 2 — Caso de uso y consistencia

1. Definir request de cierre y validación de formato.
2. Implementar coordinación del corte, cálculo del saldo teórico y transición de estado.
3. Garantizar que movimientos concurrentes no queden fuera del saldo calculado o registrados después del cierre.
4. Persistir cabecera y desglose de forma atómica; definir comportamiento de reintentos/idempotencia.
5. Rechazar solicitudes inválidas sin dejar cierres o detalles parciales.

### Fase 3 — Persistencia e integración

1. Crear/actualizar el mapeo de entidad y detalle, siguiendo el esquema real del servicio.
2. Añadir las restricciones necesarias para unicidad por sesión y consistencia de relaciones.
3. Incorporar migración solo después de confirmar el modelo y convenciones de migración.
4. Publicar `CajaCerradaDomainEvent` mediante la infraestructura de integración existente y asegurar recuperación tras fallos.
5. Verificar que reintentos del evento no generen duplicación en consumidores.

### Fase 4 — API y experiencia de usuario

1. Exponer `POST /api/v1/cajeros/sesiones/{id}/arqueo-cierre`.
2. Exponer `GET /api/v1/cajeros/sesiones/{id}/resumen-cuadre`.
3. Documentar contratos, errores, seguridad e idempotencia en OpenAPI.
4. Implementar captura del conteo y presentación clara de saldo teórico, total físico y diferencia.
5. Exigir y mostrar la justificación cuando la diferencia no sea cero.

### Fase 5 — Verificación y liberación

1. Ejecutar pruebas unitarias, integración y API.
2. Probar concurrencia, reintentos, broker no disponible y rechazo de operaciones posterior al cierre.
3. Revisar trazabilidad, acceso autorizado y exposición de datos.
4. Validar escenarios con Operaciones/Supervisión y actualizar documentación si hay decisiones nuevas.
5. Completar checklist de despliegue y monitoreo antes de liberar.

---

## 4. Estrategia de pruebas

- **Dominio:** Cálculo de subtotales y total; diferencia positiva, negativa y cero; catálogo inválido; cantidad negativa/fraccionaria; justificación requerida; estados terminales.
- **Application:** Saldo teórico obtenido de los movimientos correctos; orden y atomicidad de operaciones; solicitud inválida sin mutación; sesión inexistente/no abierta; segunda solicitud de cierre; reintento idempotente.
- **Integración/persistencia:** Cabecera y detalle atómicos; unicidad de cierre; relaciones y precision/scale monetaria; concurrencia con depósitos y retiros; rollback ante fallo.
- **Mensajería:** Evento generado una vez lógicamente, reintentos seguros, Outbox recuperable y consumidor idempotente.
- **API:** Validación de request, autenticación/autorización, códigos HTTP, serialización decimal, respuesta de resumen y errores sin información sensible.
- **End-to-end/UI:** Conteo de todas las denominaciones, suma visible, justificación condicional, confirmación de cierre y bloqueo de nuevas operaciones.
- **Regresión:** Apertura y operaciones existentes en sesiones ABIERTAS no deben cambiar de comportamiento.

---

## 5. Riesgos y mitigaciones

| Riesgo | Mitigación propuesta |
|---|---|
| Movimiento confirmado simultáneamente al cierre | Definir serialización/corte transaccional y probar concurrencia. |
| Diferencia entre cálculo del cliente y servidor | Calcular siempre subtotales y total en servidor; cliente solo presenta estimación. |
| Pérdida o duplicado de evento | Usar mecanismo confiable existente, identificador estable y consumidores idempotentes. |
| Edición accidental de arqueo | Restricciones de dominio/persistencia y correcciones por reverso auditado. |
| Diferencias de centavos por representación numérica | Usar decimal exacto, precisión de base de datos adecuada y pruebas de redondeo. |
| Catálogo distinto entre API y dominio | Centralizar/validar catálogo y documentar su versión. |
| Especificación asumida distinta al esquema actual | Realizar Fase 0 y ajustar el diseño antes de migrar o codificar. |

---

## 6. Dependencias y decisiones requeridas

- Disponibilidad del modelo actual de `SesionCaja` y de los movimientos de efectivo.
- Confirmación de permisos de cajero y supervisor para cerrar y revisar.
- Aprobación del contrato de monedas/billetes, política de idempotencia y semántica de errores.
- Confirmación de soporte para Outbox/eventos y nombre/schema de tablas conforme a la infraestructura real.
- Confirmación de si el visado del supervisor bloquea el cierre o representa una revisión posterior.

---

## 7. Entregables de implementación futura

- Dominio y pruebas de cálculo.
- Caso de uso de cierre y consulta.
- Persistencia/migración revisada.
- Endpoints y documentación OpenAPI.
- Evento auditable y pruebas de confiabilidad.
- Experiencia de captura/revisión, si forma parte del alcance de producto.
- Resultados de pruebas y evidencia de aceptación operacional.
