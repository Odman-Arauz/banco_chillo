# Tareas de Ejecución: Registro de Depósitos y Retiros en Ventanilla

- **Especificación:** `SPEC-0004`
- **Issue:** #4
- **Estado:** Pendiente — checklist de trabajo futuro. La documentación no representa implementación concluida.

---

## Fase 0 — Revisión documental y decisiones

- [x] **T0.1:** Consultar `RN-001.2` sobre retiros y `RN-001.3` sobre depósitos.
- [x] **T0.2:** Consultar la guía de desarrollo y las plantillas oficiales de especificación.
- [x] **T0.3:** Redactar la especificación funcional inicial con escenarios Gherkin.
- [x] **T0.4:** Documentar el plan, las tareas, riesgos y estrategia de pruebas.
- [ ] **T0.5:** Revisar el umbral: confirmar formalmente que todo retiro con monto `>= USD 2,000.00` requiere autorización.
- [ ] **T0.6:** Definir con Seguridad el mecanismo de autorización digital del supervisor; no aceptar ni almacenar PIN en texto claro.
- [ ] **T0.7:** Definir con Cumplimiento los datos y la evidencia de origen lícito requeridos para depósitos superiores a USD 5,000.00.
- [ ] **T0.8:** Aprobar el contrato de errores HTTP y la estrategia de consistencia entre microservicios.
- [ ] **T0.9:** Revisar modelos, API y persistencia existentes antes de decidir los archivos finales de implementación.

## Fase 1 — Dominio

- [ ] **T1.1:** Definir o reutilizar el modelo de sesión de caja y estados, evitando estados duplicados.
- [ ] **T1.2:** Definir entidad/registro append-only para movimientos de ventanilla.
- [ ] **T1.3:** Modelar tipo de movimiento, monto, cuenta, sesión, fecha, comprobante y referencia de transacción.
- [ ] **T1.4:** Implementar validación de monto positivo y precisión monetaria acordada.
- [ ] **T1.5:** Implementar la regla del umbral de supervisor para retiros iguales o superiores a USD 2,000.00.
- [ ] **T1.6:** Implementar validación de saldo físico de gaveta y actualización consistente de ingreso/egreso.
- [ ] **T1.7:** Definir errores de dominio para caja cerrada, identidad no válida, autorización ausente y saldos insuficientes.

## Fase 2 — Casos de uso y contratos

- [ ] **T2.1:** Definir command de registro de depósito con sesión, cuenta, monto, concepto e idempotencia.
- [ ] **T2.2:** Definir command de retiro con sesión, cuenta, monto, identidad e información verificable de autorización.
- [ ] **T2.3:** Definir interfaces para repositorio de caja, registro de movimiento, servicio transaccional y autorización.
- [ ] **T2.4:** Validar estado `ABIERTA` antes de invocar efectos en cuenta.
- [ ] **T2.5:** Validar datos UAFE antes de acreditar depósitos sobre USD 5,000.00.
- [ ] **T2.6:** Validar identidad, saldo disponible y efectivo de gaveta para retiros.
- [ ] **T2.7:** Asegurar que monto >= USD 2,000.00 no se procese sin autorización de supervisor confirmada.
- [ ] **T2.8:** Emitir comprobante único solo para operación confirmada.
- [ ] **T2.9:** Implementar idempotencia y comportamiento seguro de reintentos.
- [ ] **T2.10:** Definir tratamiento de timeouts y estados de resultado incierto.

## Fase 3 — Persistencia e integración

- [ ] **T3.1:** Implementar repositorios para sesiones y movimientos sin permitir edición/borrado financiero.
- [ ] **T3.2:** Implementar el adaptador de integración con el servicio de Transacciones.
- [ ] **T3.3:** Definir correlación entre movimiento de ventanilla y transacción de cuenta.
- [ ] **T3.4:** Definir y configurar outbox, reconciliación u otro patrón aprobado para fallos parciales.
- [ ] **T3.5:** Agregar restricciones para unicidad de comprobante y clave idempotente.
- [ ] **T3.6:** Agregar migración versionada si corresponde a la persistencia existente.
- [ ] **T3.7:** Configurar timeout, telemetría segura y manejo de indisponibilidad sin exponer secretos.

## Fase 4 — API y seguridad

- [ ] **T4.1:** Exponer `POST /api/v1/cajeros/operaciones/deposito`.
- [ ] **T4.2:** Exponer `POST /api/v1/cajeros/operaciones/retiro`.
- [ ] **T4.3:** Restringir ambas operaciones al rol autenticado `ROLE_CAJERO`.
- [ ] **T4.4:** Validar que el cajero tenga acceso a la sesión de caja indicada.
- [ ] **T4.5:** Mapear errores a HTTP; la falta de autorización del supervisor debe responder 403 según el issue.
- [ ] **T4.6:** Documentar DTOs, respuestas, idempotencia, códigos de error y ejemplos sintéticos en OpenAPI.
- [ ] **T4.7:** Enmascarar datos de cuenta e identidad en respuesta, trazas y logs.

## Fase 5 — Pruebas y verificación

- [ ] **T5.1:** Agregar pruebas unitarias para el dominio y los límites monetarios.
- [ ] **T5.2:** Probar retiro de USD 500.00 con saldo y gaveta suficientes.
- [ ] **T5.3:** Probar retiro de USD 2,000.00 y superiores sin autorización (rechazo) y con autorización válida.
- [ ] **T5.4:** Probar retiro con saldo de cuenta insuficiente y con saldo de gaveta insuficiente.
- [ ] **T5.5:** Probar depósito normal y depósito superior a USD 5,000.00 sin declaración.
- [ ] **T5.6:** Probar caja cerrada y ausencia de cambios en cuenta/gaveta.
- [ ] **T5.7:** Probar llamadas repetidas, idempotencia, timeout y recuperación de estado incierto.
- [ ] **T5.8:** Probar autorización API: usuario no autenticado, rol incorrecto y cajero autorizado.
- [ ] **T5.9:** Verificar que no se registren PINes, secretos ni datos reales en logs o tests.
- [ ] **T5.10:** Ejecutar `dotnet test`, build y análisis de secretos; documentar resultados reales.

## Fase 6 — Revisión y entrega

- [ ] **T6.1:** Completar revisión funcional, técnica, de seguridad y de cumplimiento.
- [ ] **T6.2:** Confirmar que documentación, API y comportamiento no contradicen `RN-001` ni ADRs aplicables.
- [ ] **T6.3:** Abrir/actualizar PR a `develop` con resumen, pruebas ejecutadas y riesgos conocidos.
- [ ] **T6.4:** Incluir `Closes #4` solamente cuando la implementación haya sido revisada y el PR esté listo para merge.
- [ ] **T6.5:** Cerrar la historia después de integrar el cambio, no por la sola creación de esta documentación.

---

## Evidencia de avance documental

Los únicos elementos marcados como completados son tareas de documentación y análisis preliminar. Todas las tareas de código, integración, pruebas ejecutables y despliegue permanecen pendientes.
