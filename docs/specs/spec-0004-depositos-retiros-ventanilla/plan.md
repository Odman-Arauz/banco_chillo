# Plan de Implementación: Registro de Depósitos y Retiros en Ventanilla

- **Especificación:** `SPEC-0004`
- **Issue:** #4 — registro de depósitos y retiros con verificación de límites
- **Estimación inicial:** 3 días / 12 horas (sujeta a refinamiento tras revisar dependencias y decisiones pendientes)
- **Estado del plan:** Propuesta documental; no se ha iniciado implementación
- **Rama prevista:** `feat/4-depositos-retiros-cajero`
- **Base de integración indicada por el proyecto:** `develop`

---

## 1. Objetivo de Implementación

Implementar el flujo de depósitos y retiros descrito en `spec.md`, respetando Clean Architecture, los límites entre microservicios y la inmutabilidad de los movimientos financieros. La cuenta y la sesión de caja deben quedar consistentes; no se debe comunicar una operación como exitosa hasta que exista confirmación transaccional y registro auditable conforme al mecanismo que se apruebe.

Este plan es una guía para trabajo futuro. No certifica que las capas, contratos, integraciones o pruebas estén implementados.

---

## 2. Alcance Técnico por Capa

### Domain — `BancoChillo.Cajeros.Domain`

- Definir el tipo de movimiento (`DEPOSITO`, `RETIRO`) y el estado de operación/caja requerido, evitando duplicar estados existentes.
- Modelar el movimiento con identificador, sesión, tipo, cuenta, monto, fecha, referencia de transacción, comprobante y evidencia de autorización cuando aplique.
- Encapsular reglas: monto positivo, umbral de supervisor para retiro `>= USD 2,000.00`, sesión abierta y límites sobre la gaveta.
- Definir errores de dominio precisos, sin acoplar la capa a HTTP ni a infraestructura.
- Representar movimientos confirmados de manera append-only; modelar reversos como eventos compensatorios solo si se aprueba su alcance.

### Application — `BancoChillo.Cajeros.Application`

- Definir `RegistrarDepositoVentanillaCommand` y `RegistrarRetiroVentanillaCommand` (o contratos equivalentes que sigan CQRS si se adopta).
- Crear handlers que primero comprueben que la sesión existe y está abierta, y que validen la autorización y disponibilidad requerida antes de solicitar el débito/crédito.
- Definir interfaces para obtener/guardar la sesión y coordinar la operación transaccional, autorización digital y registro de movimiento.
- Asegurar que un fallo o timeout de un servicio externo no produzca un comprobante de éxito ni doble procesamiento.
- Aplicar clave de idempotencia y guardar/recuperar el resultado asociado de forma consistente.
- Para depósitos mayores a USD 5,000.00, validar los datos de declaración requeridos por RN-001.3 antes de la acreditación.

### Infrastructure — `BancoChillo.Cajeros.Infrastructure` y adaptadores transaccionales

- Persistir sesión y movimientos con restricciones que impidan duplicados de comprobante e idempotencia.
- Implementar adaptador de comunicación al servicio de Transacciones con timeout, correlación y tratamiento de errores definido.
- Usar transacción local/outbox o el patrón de consistencia aprobado para publicar/confirmar movimiento.
- Proteger credenciales de servicios mediante configuración segura, sin secretos en el repositorio.
- Crear migración de base de datos versionada e idempotente, si la tecnología de persistencia existente lo requiere.
- Incorporar consulta de estado o reconciliación de operaciones inciertas cuando aplique.

### API — `BancoChillo.Cajeros.Api`

- Exponer `POST /api/v1/cajeros/operaciones/deposito` y `POST /api/v1/cajeros/operaciones/retiro`.
- Aplicar autenticación y autorización de rol `ROLE_CAJERO` y control de acceso a la sesión de caja.
- Validar el contrato de request, manejar formatos de monto e idempotencia, y mapear errores de aplicación a HTTP sin filtrar información interna.
- Documentar DTOs, ejemplos sintéticos, respuestas y códigos en OpenAPI/Swagger.
- Asegurar que la autorización del supervisor sea una evidencia verificable y nunca un PIN guardado en texto claro.

### Frontend PWA

- No es entregable de esta historia salvo aprobación expresa de alcance. Si se agrega posteriormente, diseñar formularios diferenciados de depósito y retiro, validaciones de monto, advertencia de umbral, confirmación y visualización del comprobante sin exponer datos innecesarios.

---

## 3. Orden Sugerido de Trabajo

1. Revisar la especificación y cerrar las decisiones pendientes sobre autorización, UAFE, idempotencia y consistencia.
2. Inspeccionar los modelos de sesión de caja, transacciones, autenticación y persistencia ya existentes; evitar crear modelos o estados duplicados.
3. Definir contratos internos entre Cajeros y Transacciones y acordar el manejo de errores/timeout.
4. Implementar y probar reglas de dominio aisladas.
5. Implementar casos de uso y adaptadores, primero con dobles de prueba y luego con integración controlada.
6. Implementar persistencia y restricciones de idempotencia/inmutabilidad.
7. Exponer API, autorización y OpenAPI.
8. Añadir pruebas de integración y escenarios de fallos/reintentos.
9. Ejecutar build, pruebas, análisis de secretos y revisión de seguridad.
10. Abrir o actualizar PR hacia `develop`, describir limitaciones conocidas y enlazar el issue con `Closes #4` únicamente cuando el trabajo de implementación esté completo y revisado.

---

## 4. Estrategia de Pruebas

### Pruebas unitarias de dominio

- monto cero, negativo y con precisión superior a dos decimales
- retiro bajo el umbral sin autorización y retiro en el umbral exacto
- retiro mayor al umbral sin autorización, con autorización inválida y con autorización válida
- caja abierta y caja cerrada
- gaveta suficiente e insuficiente
- intento de cambiar/eliminar un movimiento confirmado (debe impedirse por diseño)

### Pruebas de Application

- orden de validación: caja y autorización se verifican antes del efecto transaccional
- cuenta rechaza débito/crédito y no se registra operación exitosa
- fallo del adaptador transaccional
- timeout con resultado incierto y reintento idempotente
- solicitud duplicada con misma clave y solicitudes distintas con claves diferentes
- depósito sobre USD 5,000.00 sin declaración
- manejo de estado de caja que cambia durante la solicitud, según garantías de concurrencia disponibles

### Pruebas de API / integración

- `ROLE_CAJERO` puede invocar operaciones autorizadas
- usuario no autenticado obtiene 401 y usuario sin rol obtiene 403
- contratos, serialización, validaciones y códigos HTTP
- comprobante único y respuesta consistente con el estado persistido
- integración controlada con servicio de Transacciones, usando entorno de pruebas y cuentas ficticias
- no exponer PIN, cédula completa, número de cuenta sin máscara ni credenciales en logs

### Pruebas de regresión y operación

- reintentos y duplicados ante timeout
- reconciliación de resultados pendientes
- cálculo de saldos teóricos para el arqueo de caja
- ejecución repetida de migraciones cuando corresponda

---

## 5. Riesgos y Mitigaciones

| Riesgo | Mitigación propuesta |
|---|---|
| Débito/crédito confirmado sin asiento de caja o asiento de caja sin transacción | Definir estrategia de consistencia distribuida, outbox y reconciliación antes de desarrollar. |
| Duplicación por reintentos de red | Clave de idempotencia persistente, restricciones únicas y prueba de timeout/reintento. |
| Uso de PIN de supervisor como texto plano | Validación en servicio de autorización; almacenar solo identidad y referencia verificable, nunca el secreto. |
| Descuadre entre gaveta y saldo teórico | Actualizar la sesión solo al confirmar la operación; integrar con reglas de arqueo y auditar cada movimiento. |
| Exposición de datos personales/financieros | Enmascarar, minimizar logs, limitar permisos y utilizar datos sintéticos en tests/documentación. |
| Diferencia entre “supera” y “>=” USD 2,000 | Usar `>=` por criterio explícito del issue y registrar esta decisión en la spec. |
| Falta de campos de declaración UAFE en request inicial | Definir contrato con cumplimiento antes de aceptar depósitos por encima de USD 5,000. |

---

## 6. Criterios de Listo para Implementación

- [ ] Spec revisada y aprobada por responsables funcional y técnico.
- [ ] Decisiones de supervisor, identificación del titular, UAFE y códigos HTTP resueltas.
- [ ] Contrato de integración Cajeros–Transacciones y garantía de idempotencia definidos.
- [ ] Estrategia para fallos parciales, timeout y reconciliación documentada.
- [ ] Modelos existentes de caja y transacciones revisados para evitar duplicidad.
- [ ] Datos y ambiente de pruebas sintéticos disponibles.
