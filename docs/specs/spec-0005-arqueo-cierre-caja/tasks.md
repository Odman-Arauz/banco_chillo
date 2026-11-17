# Tareas de Ejecución: Arqueo de Caja por Denominaciones y Cierre Diario de Jornada

- **Especificación:** SPEC-0005
- **Issue:** #5
- **Estado de esta lista:** Planificación documental; las tareas no se consideran implementadas por este PR.

---

## A. Análisis previo

- [ ] **A1:** Revisar el ciclo actual de sesión de caja, movimientos y estados existentes.
- [ ] **A2:** Confirmar cómo se calcula el saldo teórico e identificar todos los movimientos que afectan efectivo.
- [ ] **A3:** Acordar permisos, revisión/visado de supervisor y alerta de descuadre con Operaciones.
- [ ] **A4:** Confirmar catálogo de denominaciones, política de datos omitidos/cero e idempotencia del API.
- [ ] **A5:** Confirmar patrón de persistencia y mensajería (Outbox/eventos) utilizado por el servicio.

## B. Dominio

- [ ] **T1:** Modelar `ArqueoCaja` y `ArqueoDenominacion` según las convenciones del proyecto.
- [ ] **T2:** Definir catálogo y validación de tipos y denominaciones admitidos.
- [ ] **T3:** Implementar cálculo de subtotal, total físico, saldo teórico y diferencia con precisión decimal.
- [ ] **T4:** Definir estados `CERRADA_CUADRADA`, `CERRADA_SOBRANTE` y `CERRADA_FALTANTE`.
- [ ] **T5:** Aplicar invariantes: cantidades enteras no negativas, justificación obligatoria ante descuadre e inmutabilidad del cierre confirmado.
- [ ] **T6:** Añadir pruebas unitarias de cálculo, límites y transiciones del dominio.

## C. Aplicación y consistencia

- [ ] **T7:** Definir `RealizarArqueoCierreCommand` y contratos de entrada/salida.
- [ ] **T8:** Implementar validación de sesión existente, abierta y autorizada.
- [ ] **T9:** Implementar corte consistente de movimientos y cálculo de saldo teórico.
- [ ] **T10:** Coordinar bloqueo de operaciones concurrentes con el cierre.
- [ ] **T11:** Implementar idempotencia/reintentos para evitar cierres duplicados.
- [ ] **T12:** Añadir consulta de resumen sin efectos secundarios.
- [ ] **T13:** Añadir pruebas de handler para éxito, faltantes, sobrantes, solicitudes inválidas y conflictos.

## D. Infraestructura e integración

- [ ] **T14:** Configurar persistencia de cabecera y desglose conforme al esquema real.
- [ ] **T15:** Añadir migración y restricciones de integridad/unicidad aprobadas.
- [ ] **T16:** Garantizar persistencia atómica del cierre y líneas de denominación.
- [ ] **T17:** Emitir `CajaCerradaDomainEvent` mediante el mecanismo confiable existente.
- [ ] **T18:** Implementar reintento/consumo idempotente y observabilidad de fallos de publicación.
- [ ] **T19:** Añadir pruebas de integración para persistencia, concurrencia y mensajería.

## E. API y experiencia de usuario

- [ ] **T20:** Exponer `POST /api/v1/cajeros/sesiones/{id}/arqueo-cierre`.
- [ ] **T21:** Exponer `GET /api/v1/cajeros/sesiones/{id}/resumen-cuadre`.
- [ ] **T22:** Documentar OpenAPI, permisos, respuestas y códigos de error.
- [ ] **T23:** Construir formulario de conteo por denominación, si está dentro del alcance del producto.
- [ ] **T24:** Mostrar totales y diferencia y exigir justificación en todo descuadre.
- [ ] **T25:** Añadir pruebas de API/UI y verificar accesibilidad/usabilidad con los roles previstos.

## F. Aceptación y liberación

- [ ] **T26:** Verificar escenarios Gherkin de SPEC-0005 con Producto/Operaciones.
- [ ] **T27:** Ejecutar suite de pruebas, análisis y validaciones requeridas por el repositorio.
- [ ] **T28:** Confirmar que no se pueden registrar movimientos después del cierre.
- [ ] **T29:** Revisar trazabilidad, inmutabilidad, privacidad y alertas.
- [ ] **T30:** Actualizar documentación con decisiones y resultados verificados.
- [ ] **T31:** Preparar PR de implementación futura con evidencia y referencia al issue.

---

## Definición de terminado para la implementación futura

- Los criterios de aceptación funcionales están cubiertos por pruebas.
- El cierre, el desglose y la diferencia se persisten de forma consistente y auditable.
- No existe camino de mutación/eliminación directa del arqueo confirmado.
- El sistema bloquea operaciones posteriores y maneja correctamente concurrencia y reintentos.
- Los descuadres requieren justificación y generan alerta conforme a RN-001.4.
- El evento de cierre se entrega con semántica confiable y consumidores idempotentes.
- La API y cualquier UI muestran valores coherentes con el cálculo del servidor.
- Las verificaciones de seguridad, integración y regresión pasan.
