# Especificación: Arqueo de Caja por Denominaciones y Cierre Diario de Jornada

- **ID:** SPEC-0005
- **Módulo:** Cajeros
- **Estado:** En revisión
- **Fecha:** 2026-11-17
- **Issue relacionado:** #5
- **Reglas y decisiones relacionadas:** RN-001.4 y ADR-0003

---

## 1. Propósito y alcance

Definir el proceso funcional para cerrar una sesión de caja al finalizar la jornada mediante el conteo físico de billetes y monedas, la comparación de ese conteo con el saldo teórico registrado por el sistema y la conservación de evidencia auditable del resultado.

La especificación cubre el registro del desglose por denominación, el cálculo de totales, la clasificación de diferencias, la validación de justificación, el cierre de la sesión, la consulta del resumen y la notificación a los sistemas de auditoría y contabilidad. Esta entrega es únicamente documental: no implementa código, persistencia, endpoints funcionales ni interfaces de usuario.

### Dentro del alcance

- Capturar cantidades enteras para denominaciones monetarias admitidas.
- Calcular el total declarado usando aritmética decimal exacta.
- Determinar el saldo teórico a partir del fondo inicial y los movimientos contabilizados de la sesión.
- Calcular y clasificar la diferencia entre conteo físico y saldo teórico.
- Exigir justificación para cualquier descuadre, en cumplimiento de RN-001.4.
- Formalizar el cierre y evitar operaciones posteriores sobre la sesión cerrada.
- Mantener un registro inmutable y trazable del arqueo y sus denominaciones.
- Comunicar el evento de cierre a los consumidores de auditoría y contabilidad.
- Consultar el resumen de cuadre de una sesión.

### Fuera del alcance

- La implementación de código o migraciones.
- La generación de reportes regulatorios no descritos por las reglas vigentes.
- La corrección directa o eliminación de arqueos ya registrados.
- La definición de procesos de investigación, sanción o ajuste contable posteriores al cierre.
- La gestión de permisos y autenticación más allá de requerir un actor autorizado conforme al modelo de seguridad del servicio.

---

## 2. Historia de usuario

- **US-01 — Cajero:** Como cajero, quiero registrar el conteo físico de mi gaveta al finalizar la jornada para compararlo con el saldo que el sistema espera.
- **US-02 — Supervisor:** Como Supervisor de Agencia, quiero revisar el resultado y la justificación de un descuadre para dar seguimiento y aplicar la revisión operativa correspondiente.
- **US-03 — Auditoría/contabilidad:** Como consumidor de auditoría o contabilidad, quiero recibir un registro consistente del cierre para conservar trazabilidad de las operaciones de caja.

---

## 3. Glosario

| Término | Definición |
|---|---|
| Sesión de caja | Jornada de atención de un cajero asociada a una caja/ventanilla y a su saldo inicial y movimientos. |
| Saldo inicial | Fondo de efectivo registrado al abrir la sesión. |
| Saldo teórico | Saldo esperado en el sistema al momento del cierre, según saldo inicial y movimientos contabilizados. |
| Total físico | Suma del valor de las unidades de efectivo declaradas por denominación. |
| Diferencia | Total físico menos saldo teórico. Un resultado negativo indica faltante y uno positivo indica sobrante. |
| Arqueo | Registro del conteo físico, comparación, diferencia y resultado de cierre. |
| Denominación | Valor unitario de un billete o moneda admitido por el catálogo. |

---

## 4. Reglas de negocio aplicables

### RN-001.4 — Arqueo y cierre diario de caja

- El cajero debe contar físicamente su gaveta e ingresar las cantidades por denominación.
- Se compara el saldo físico declarado con el saldo teórico del sistema.
- Si la diferencia es cero, el cierre se clasifica como balanceado.
- Si la diferencia no es cero, se registra como sobrante o faltante, se emite una alerta a la gerencia de operaciones y se exige justificación.

### ADR-0003 — Flujo de operaciones y arqueo inmutable

- El arqueo y los movimientos de caja no se modifican ni eliminan después de registrarse.
- Las correcciones deben resolverse mediante mecanismos de reverso auditados, no editando el registro original.
- Los descuadres deben conservar evidencia y someterse a la revisión correspondiente.

### Reglas derivadas para esta especificación

1. El saldo teórico se obtiene de los movimientos válidos contabilizados hasta el instante de corte: `SaldoInicial + TotalDepositos - TotalRetiros`. Otros tipos de movimiento que afecten el efectivo deberán incluirse según el catálogo aprobado del servicio; no deben omitirse ni contarse dos veces.
2. La diferencia se calcula como `TotalFisico - SaldoTeorico`.
3. Si diferencia = 0, el estado del arqueo es `CERRADA_CUADRADA` y la diferencia se almacena como `0.00`.
4. Si diferencia > 0, el estado del arqueo es `CERRADA_SOBRANTE`.
5. Si diferencia < 0, el estado del arqueo es `CERRADA_FALTANTE`.
6. Toda diferencia distinta de cero requiere justificación no vacía, conforme a RN-001.4. La obligatoriedad no se limita al faltante.
7. Solo se puede ejecutar el cierre de una sesión abierta y una sola vez. Un reintento idempotente debe devolver el resultado ya registrado si corresponde a la misma solicitud; no debe crear un segundo arqueo.
8. Una vez iniciado el cierre, depósitos y retiros de esa sesión no deben poder confirmarse después del instante de corte. El corte y el cierre deben coordinarse de forma atómica para evitar movimientos concurrentes omitidos o duplicados.
9. Las cantidades por denominación son enteros mayores o iguales a cero. No se aceptan denominaciones desconocidas, tipos incompatibles ni valores monetarios no admitidos.
10. El cálculo monetario utiliza precisión decimal exacta, nunca punto flotante binario. Los importes se expresan en USD y se presentan con dos decimales.
11. Se persiste el desglose declarado, incluso cuando una cantidad es cero si la política de captura así lo requiere; la representación debe mantenerse consistente para todas las solicitudes.
12. La ausencia de una denominación equivale a cero únicamente si el contrato de solicitud lo establece expresamente; la validación debe documentarlo y aplicarlo uniformemente.

---

## 5. Denominaciones admitidas

El catálogo inicial especificado para el conteo es:

| Tipo | Denominaciones unitarias (USD) |
|---|---|
| `BILLETE` | 100, 50, 20, 10, 5, 1 |
| `MONEDA` | 1, 0.50, 0.25, 0.10, 0.05, 0.01 |

El valor `1.00` aparece en ambas categorías en la historia del issue. La implementación deberá conservar el tipo y distinguir billete de moneda; no debe sumar una entrada dos veces. El catálogo deberá ser validado por el dominio y cualquier cambio futuro deberá quedar versionado o gestionado mediante una decisión de producto.

---

## 6. Cálculos y ejemplos de referencia

### Fórmulas

- `SubtotalDenominacion = ValorUnitario × Cantidad`
- `TotalFisico = suma de todos los SubtotalDenominacion`
- `SaldoTeorico = SaldoInicial + TotalDepositos - TotalRetiros` (incluyendo los demás movimientos de efectivo autorizados, según las reglas del módulo)
- `Diferencia = TotalFisico - SaldoTeorico`

### Ejemplos

| Saldo teórico | Total físico | Diferencia | Estado esperado |
|---:|---:|---:|---|
| 1,250.00 | 1,250.00 | 0.00 | `CERRADA_CUADRADA` |
| 1,250.00 | 1,230.00 | -20.00 | `CERRADA_FALTANTE` |
| 1,250.00 | 1,280.00 | 30.00 | `CERRADA_SOBRANTE` |

Los valores de ejemplo son referenciales. Una diferencia distinta de cero requiere justificación y debe generar la alerta definida por RN-001.4.

---

## 7. Flujo funcional de cierre

1. El actor autorizado solicita el cierre de una sesión abierta e incluye el conteo por denominación y, cuando corresponda, la justificación.
2. El sistema verifica que la sesión exista y continúe abierta.
3. El sistema establece un punto de corte consistente para impedir que se confirmen operaciones durante o después del cierre.
4. El sistema determina los movimientos válidos hasta ese punto y calcula el saldo teórico.
5. El sistema valida tipos, denominaciones, cantidades y formato de los importes.
6. El sistema calcula subtotales, total físico y diferencia.
7. Si la diferencia no es cero, valida que la justificación tenga contenido significativo. Si falta, rechaza la solicitud sin cerrar la sesión.
8. El sistema registra el arqueo y sus líneas de denominación de forma atómica, actualiza el estado y los saldos de cierre de la sesión y conserva quién/cuándo efectuó la operación.
9. El sistema registra/publica el evento de cierre mediante el mecanismo confiable de mensajería aprobado por el servicio, sin perder el registro si el broker no está disponible.
10. El sistema devuelve el resultado del cierre. Los consumidores reciben la alerta pertinente cuando hay sobrante o faltante.

---

## 8. Criterios de aceptación (Gherkin)

```gherkin
Característica: Arqueo y cierre diario de caja

  Escenario: Cerrar una caja cuadrada
    Dado que una sesión de caja abierta tiene un saldo teórico de 1250.00 USD
    Y el cajero declara denominaciones cuyo total físico es 1250.00 USD
    Cuando solicita el cierre de la sesión
    Entonces el arqueo queda en estado "CERRADA_CUADRADA"
    Y la diferencia registrada es 0.00 USD
    Y el desglose y el cierre quedan asociados a la sesión
    Y la sesión no acepta nuevas operaciones de caja
    Y se registra el evento de cierre para auditoría y contabilidad

  Escenario: Cerrar una caja con faltante y justificación
    Dado que una sesión de caja abierta tiene un saldo teórico de 1250.00 USD
    Y el cajero declara un total físico de 1230.00 USD
    Y proporciona una justificación no vacía
    Cuando solicita el cierre de la sesión
    Entonces el estado es "CERRADA_FALTANTE"
    Y la diferencia es -20.00 USD
    Y se conserva la justificación
    Y se genera la alerta de descuadre para revisión
    Y la sesión no acepta nuevas operaciones

  Escenario: Rechazar faltante sin justificación
    Dado que una sesión abierta tiene un saldo teórico de 1250.00 USD
    Y el cajero declara un total físico de 1230.00 USD
    Y no proporciona justificación
    Cuando solicita el cierre
    Entonces la solicitud se rechaza por validación
    Y la sesión permanece abierta
    Y no se registra un arqueo parcial

  Escenario: Cerrar una caja con sobrante y justificación
    Dado que una sesión de caja abierta tiene un saldo teórico de 1250.00 USD
    Y el cajero declara un total físico de 1280.00 USD
    Y proporciona una justificación no vacía
    Cuando solicita el cierre de la sesión
    Entonces el estado es "CERRADA_SOBRANTE"
    Y la diferencia es 30.00 USD
    Y se genera la alerta de descuadre para revisión

  Escenario: Rechazar sobrante sin justificación
    Dado que una sesión abierta tiene un saldo teórico de 1250.00 USD
    Y el cajero declara un total físico de 1280.00 USD
    Y no proporciona justificación
    Cuando solicita el cierre
    Entonces la solicitud se rechaza por validación
    Y la sesión permanece abierta

  Escenario: Rechazar una denominación no admitida
    Dado que una sesión de caja está abierta
    Cuando el cajero envía una denominación que no pertenece al catálogo
    Entonces la solicitud se rechaza con un error de validación
    Y la sesión no cambia de estado

  Escenario: Rechazar una cantidad negativa o fraccionaria
    Dado que una sesión de caja está abierta
    Cuando el conteo contiene una cantidad negativa o no entera
    Entonces la solicitud se rechaza con un error de validación
    Y no se registra el arqueo

  Escenario: No permitir operaciones después del cierre
    Dado que la sesión tiene un arqueo de cierre confirmado
    Cuando se intenta confirmar un depósito o retiro en esa sesión
    Entonces la operación se rechaza por conflicto de estado

  Escenario: Evitar un segundo arqueo para la misma sesión
    Dado que la sesión ya tiene un cierre confirmado
    Cuando se envía otra solicitud de cierre
    Entonces el sistema no crea un segundo arqueo
    Y devuelve el resultado existente o un conflicto según la política de idempotencia

  Escenario: Evitar pérdida del evento ante indisponibilidad del broker
    Dado que los datos del cierre se guardan correctamente
    Y el broker no está disponible temporalmente
    Cuando se confirma el cierre
    Entonces el arqueo permanece persistido
    Y el evento queda pendiente para publicación confiable
```

---

## 9. Diseño técnico propuesto

### Dominio

- `ArqueoCaja`: representa el cierre, saldo teórico, total físico, diferencia, estado, justificación, actor y fecha de corte.
- `ArqueoDenominacion`: representa tipo, valor unitario, cantidad y subtotal de una línea del conteo.
- Estados esperados: `CERRADA_CUADRADA`, `CERRADA_SOBRANTE`, `CERRADA_FALTANTE`.
- Operaciones de dominio conceptuales: calcular total físico y comparar contra el saldo teórico.
- Las invariantes del arqueo deben expresarse en el dominio y no depender de detalles HTTP o de base de datos.

### Aplicación

- Caso de uso: `RealizarArqueoCierreCommand`, con `sesionCajaId`, lista de denominaciones y justificación opcional en el transporte.
- El handler coordina sesión, corte de operaciones, cálculo de saldo teórico, validación, persistencia y evento.
- Una consulta separada devuelve el resumen del cuadre sin mutar el estado.
- La autorización de actor y la verificación de pertenencia a la sesión deben seguir las políticas del servicio.

### Persistencia e integración

- Persistir cabecera y desglose en una misma transacción; la tabla de detalle propuesta por el issue es `cajeros_bc.arqueo_denominaciones`.
- Actualizar los campos de cierre de `sesiones_caja` conforme al modelo existente.
- Aplicar restricciones que eviten más de un cierre confirmado por sesión.
- Guardar evidencia suficiente para reproducir el resultado: importes, movimientos considerados o referencia de corte, fecha/hora, actor y versión/regla aplicada.
- La publicación de `CajaCerradaDomainEvent` debe ser confiable (por ejemplo, patrón Outbox si el servicio ya lo utiliza). El evento no reemplaza el registro persistido.

### API propuesta

- `POST /api/v1/cajeros/sesiones/{id}/arqueo-cierre`: solicita el cierre y recibe el conteo.
- `GET /api/v1/cajeros/sesiones/{id}/resumen-cuadre`: consulta el saldo teórico, el total físico, la diferencia, estado, desglose y datos de auditoría permitidos.

### Códigos HTTP orientativos

- `201 Created`: arqueo creado y cierre confirmado.
- `200 OK`: consulta del resumen o repetición idempotente que devuelve el resultado existente.
- `400 Bad Request`: formato o contenido inválido.
- `401 Unauthorized` / `403 Forbidden`: actor sin autenticación o permisos.
- `404 Not Found`: sesión inexistente o no visible para el actor.
- `409 Conflict`: sesión ya cerrada, operación concurrente o conflicto de estado.
- `422 Unprocessable Entity`: reglas de negocio incumplidas, incluida justificación ausente para diferencia distinta de cero.
- `500 Internal Server Error`: error no esperado, sin exponer datos sensibles.

Los códigos definitivos deben alinearse con el middleware y convenciones existentes del API.

---

## 10. Contratos de datos ilustrativos

Los ejemplos siguientes son ilustrativos y no constituyen una implementación ni un contrato final de serialización. Los importes deben manejarse con precisión decimal.

### Solicitud de cierre

```json
{
  "conteoFisico": [
    { "tipo": "BILLETE", "denominacion": 100.00, "cantidad": 10 },
    { "tipo": "BILLETE", "denominacion": 50.00, "cantidad": 4 },
    { "tipo": "MONEDA", "denominacion": 1.00, "cantidad": 20 },
    { "tipo": "MONEDA", "denominacion": 0.25, "cantidad": 10 }
  ],
  "justificacion": null
}
```

La justificación puede ser nula únicamente cuando el arqueo resulte cuadrado. Si existe diferencia, debe contener una explicación no vacía. La validación no debe confiar en el total enviado por el cliente: los subtotales y el total se calculan en el servidor.

### Respuesta conceptual

```json
{
  "sesionCajaId": "<id-de-sesion>",
  "fechaCorte": "<fecha-hora-iso-8601>",
  "saldoInicial": 0.00,
  "totalDepositos": 0.00,
  "totalRetiros": 0.00,
  "saldoTeoricoSistema": 1250.00,
  "totalFisico": 1230.00,
  "diferencia": -20.00,
  "estado": "CERRADA_FALTANTE",
  "justificacion": "Justificación registrada por el cajero",
  "desglose": [
    { "tipo": "BILLETE", "denominacion": 100.00, "cantidad": 10, "subtotal": 1000.00 }
  ]
}
```

---

## 11. Evento de dominio propuesto

`CajaCerradaDomainEvent` deberá permitir a consumidores autorizados identificar, como mínimo:

- Identificador de sesión y de arqueo.
- Identificador del actor que realizó el cierre.
- Fecha/hora de corte y de registro.
- Saldo teórico, total físico, diferencia y estado.
- Referencia a la incidencia/justificación cuando corresponda, según las políticas de exposición de datos.
- Identificador de correlación para rastrear el flujo.

No se debe incluir información sensible innecesaria. El evento debe ser versionable y los consumidores deben tolerar reintentos sin contabilizar dos veces el mismo cierre.

---

## 12. Seguridad, auditoría y privacidad

- Validar autenticación y autorización del cajero/supervisor conforme a los permisos del servicio.
- Registrar identidad del actor y correlación de solicitud en la auditoría.
- No aceptar del cliente el saldo teórico como fuente de verdad.
- Evitar revelar información de otras sesiones en errores o consultas.
- Conservar la evidencia financiera según las políticas de retención del banco.
- No editar ni eliminar el arqueo; todo ajuste debe quedar documentado como reverso o evento compensatorio auditado.

---

## 13. Riesgos y preguntas para validación

- Confirmar qué otros movimientos (pagos, reversos, ajustes autorizados) forman parte del saldo teórico además de depósitos y retiros.
- Confirmar si las monedas y billetes deben representarse en una única lista diferenciada por tipo o si el contrato usa catálogos separados.
- Acordar qué comportamiento se espera para cierre concurrente, reintentos y clave de idempotencia con las convenciones del API.
- Confirmar política de visado/firma del supervisor mencionada en ADR-0003 y cuándo debe bloquear la finalización frente a permitir cierre sujeto a revisión.
- Confirmar convención de formato monetario, fecha/hora y respuesta de errores existente en el repositorio.

---

## 14. Trazabilidad

| Requisito | Referencia en esta especificación |
|---|---|
| Conteo por denominación | Secciones 5, 7 y 8 |
| Comparación de saldo físico y teórico | Secciones 4, 6 y 8 |
| Clasificación de cuadrado/sobrante/faltante | Secciones 4, 6 y 8 |
| Justificación de descuadre | Secciones 4 y 8 |
| Bloqueo de operaciones tras cierre | Secciones 4, 7 y 8 |
| Persistencia inmutable y trazabilidad | Secciones 7, 9 y 12 |
| Evento para auditoría/contabilidad | Secciones 7, 9 y 11 |
| Consulta de resumen de cuadre | Secciones 9 y 10 |
