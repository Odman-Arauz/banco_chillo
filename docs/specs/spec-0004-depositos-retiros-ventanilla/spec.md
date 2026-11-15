# Especificación: Registro de Depósitos y Retiros en Ventanilla

- **ID:** SPEC-0004
- **Módulo:** Cajeros / Transacciones
- **Estado:** Borrador — pendiente de revisión y aprobación
- **Fecha:** 2026-11-15
- **Issue asociado:** [#4](https://github.com/Odman-Arauz/banco_chillo/issues/4)
- **Reglas relacionadas:** `RN-001.1`, `RN-001.2`, `RN-001.3` y `RN-001.4`
- **ADR de referencia:** `ADR-0001` (arquitectura de microservicios y Clean Architecture), `ADR-0003` (flujo de cajeros y arqueo inmutable)

---

## 1. Propósito y Alcance

Esta especificación define los requisitos funcionales y técnicos para que un cajero registre depósitos y retiros de efectivo desde una sesión de caja. La operación debe validar el estado de la caja, las condiciones de la cuenta y la disponibilidad de efectivo; debe aplicar el control de autorización de supervisor para retiros de alto valor y dejar evidencia verificable para auditoría.

El objetivo de negocio es procesar operaciones de ventanilla de manera segura, mantener consistentes los saldos de la cuenta y de la gaveta, y contar con un comprobante único que permita rastrear cada operación.

### Dentro del alcance

- Registrar depósitos en efectivo y solicitar la acreditación inmediata a la cuenta destino.
- Registrar retiros y solicitar el débito correspondiente a la cuenta del titular.
- Verificar que la sesión de caja identificada exista y permanezca `ABIERTA` durante el procesamiento.
- Validar monto positivo, datos requeridos, saldo disponible de la cuenta y efectivo disponible en gaveta, según aplique.
- Exigir autorización digital del supervisor para retiros **iguales o superiores a USD 2,000.00**, conforme al criterio explícito del issue #4.
- Registrar el movimiento de caja de forma auditable e inmutable y producir un comprobante único.
- Devolver errores diferenciados para datos inválidos, falta de autorización, saldo insuficiente, caja cerrada o indisponibilidad de dependencias.

### Fuera del alcance de esta especificación

- Apertura, cierre y arqueo de caja, salvo las validaciones de estado y los saldos necesarios para procesar la operación.
- Transferencias entre cuentas, pagos de servicios, reversos, anulaciones o modificaciones de operaciones completadas.
- Diseño de pantallas de la PWA.
- Definición de límites diarios por cliente o agencia, listas de riesgo, reglas antifraude adicionales o gestión de bóveda, si no se aprueban como requisitos aparte.
- Implementación de persistencia, autenticación, integración entre servicios o endpoints. Este documento describe lo esperado, no afirma que ya esté implementado.

---

## 2. Historias de Usuario Asociadas

- **US-01 — Depósito:** Como cajero de ventanilla, quiero registrar un depósito en efectivo para acreditar de inmediato el monto a la cuenta del socio y dejar constancia en la caja.
- **US-02 — Retiro:** Como cajero de ventanilla, quiero registrar un retiro validando identidad, saldo de cuenta y efectivo en gaveta para entregar dinero de forma segura.
- **US-03 — Autorización:** Como supervisor de agencia, quiero autorizar digitalmente los retiros iguales o superiores a USD 2,000.00 para controlar operaciones de alto valor.
- **US-04 — Trazabilidad:** Como responsable de operaciones o auditor, quiero consultar el comprobante y los datos esenciales de un movimiento para poder verificar su trazabilidad sin alterar el asiento original.

---

## 3. Actores y Responsabilidades

| Actor / componente | Responsabilidad |
|---|---|
| Cajero (`ROLE_CAJERO`) | Solicitar depósitos y retiros de una sesión de caja asignada; confirmar datos antes del envío. |
| Supervisor de agencia | Emitir o validar la autorización digital de un retiro que alcance el umbral. La identidad del autorizador debe quedar asociada al movimiento. |
| Servicio de Cajeros | Validar la sesión de caja, aplicar las reglas de ventanilla y coordinar el registro del movimiento. |
| Servicio de Transacciones | Validar y ejecutar el crédito o débito en la cuenta con resultado inequívoco. |
| Persistencia / registro contable | Conservar el movimiento y el comprobante como evidencia append-only, de acuerdo con `ADR-0003`. |

La presencia de un PIN en una solicitud no demuestra por sí sola que la autorización sea válida. La autenticación, autorización, expiración y asociación del supervisor deben verificarse con el mecanismo seguro definido para el sistema; no se deben guardar ni exponer PINes en texto claro.

---

## 4. Reglas de Negocio Aplicables

- **RN-001.1 — Caja abierta:** no se procesa ninguna operación si la sesión de caja no está en estado `ABIERTA`.
- **RN-001.2 — Retiros:** validar identidad del titular; no permitir retirar más que el saldo disponible de la cuenta ni más que el efectivo disponible en la gaveta. El issue #4 define autorización de supervisor para retiros de USD 2,000.00 o más. Esta especificación adopta `monto >= 2000.00` para resolver la diferencia entre el texto general de la regla («supera») y el criterio del issue.
- **RN-001.3 — Depósitos:** acreditar de manera inmediata el saldo contable y disponible de la cuenta destino. Depósitos **superiores a USD 5,000.00** requieren registrar declaración de origen lícito de fondos, de acuerdo con la regla vigente.
- **RN-001.4 — Arqueo y cierre:** los movimientos deben permitir calcular el saldo teórico de caja y compararlo con el saldo físico declarado durante el cierre. La operación no reemplaza el proceso formal de arqueo.
- **Inmutabilidad:** no actualizar ni eliminar un movimiento financiero ya confirmado. Los ajustes, si se definen en otro alcance, deben realizarse mediante una operación de reverso auditada.
- **Monto:** debe ser mayor que cero y respetar la precisión monetaria definida para USD. Se recomienda rechazar montos con más de dos decimales, en lugar de redondearlos silenciosamente.
- **Atomicidad lógica:** no se debe informar éxito si el débito/crédito de la cuenta no fue confirmado y el movimiento de caja no tiene una forma confiable de quedar registrado. La estrategia de consistencia entre microservicios —por ejemplo, idempotencia, outbox o compensación— debe ajustarse a `ADR-0001` y `ADR-0003` antes de implementar.

---

## 5. Flujos Funcionales

### 5.1 Depósito

1. El cajero autenticado envía la sesión de caja, la cuenta, el monto y el concepto.
2. El sistema valida autorización del cajero, formato de entrada y que la sesión exista y esté `ABIERTA`.
3. Si el monto supera USD 5,000.00, el sistema exige los datos de la declaración de origen lícito que se definan en el contrato final. Si faltan, se rechaza antes de acreditar.
4. El servicio responsable solicita al servicio transaccional acreditar la cuenta.
5. Solo ante confirmación del crédito se registra el ingreso de efectivo en la sesión de caja y se emite el comprobante.
6. Si una dependencia falla o el resultado del crédito es desconocido, la respuesta debe identificar que la operación no se confirmó o que su estado requiere verificación; no se debe permitir duplicar el depósito por reintento.

### 5.2 Retiro

1. El cajero autenticado envía la sesión, cuenta, monto y cédula del titular; si corresponde, se incluye el contexto de autorización digital del supervisor.
2. El sistema valida rol, datos, estado `ABIERTA` de la caja, identidad del titular y disponibilidad de la cuenta y de la gaveta.
3. Si el monto es igual o superior a USD 2,000.00, verifica la autorización digital y obtiene la identidad del supervisor. Si falta o no es válida, rechaza con HTTP 403.
4. El servicio transaccional intenta debitar la cuenta. Si no confirma el débito, no se entrega efectivo ni se registra el egreso como completado.
5. Tras confirmación, se registra el egreso de gaveta, el movimiento inmutable y el comprobante único.

### 5.3 Resultado incierto o reintento

Cada solicitud de operación debe contar con un identificador de idempotencia o mecanismo equivalente. Un reintento con la misma clave no debe generar un segundo débito, crédito, movimiento de caja ni comprobante. Se debe conservar una forma de consultar el resultado de una solicitud cuyo procesamiento haya quedado incierto.

---

## 6. Criterios de Aceptación (Gherkin)

```gherkin
Característica: Operaciones de depósito y retiro en ventanilla

  Escenario: Retiro normal con saldo de cuenta y efectivo suficiente
    Dado un cajero autorizado con una sesión de caja ABIERTA
    Y el titular tiene saldo disponible de USD 3,000.00
    Y la gaveta dispone de al menos USD 500.00
    Cuando registra un retiro de USD 500.00 con identidad válida
    Entonces el servicio de transacciones confirma el débito de USD 500.00
    Y la gaveta registra un egreso de USD 500.00
    Y se almacena un movimiento inmutable
    Y se devuelve un número de comprobante único

  Escenario: Rechazo de retiro de alto valor sin autorización
    Dado un cajero autorizado con una sesión de caja ABIERTA
    Cuando solicita retirar USD 2,500.00 sin autorización digital válida del supervisor
    Entonces el sistema responde HTTP 403 Forbidden
    Y el mensaje indica "Se requiere autorización del Supervisor para retiros de $2,000 USD o superiores"
    Y no se debita la cuenta ni se entrega efectivo
    Y no se registra un retiro completado

  Escenario: El umbral incluye exactamente USD 2,000.00
    Dado un cajero autorizado con una sesión de caja ABIERTA
    Cuando solicita retirar exactamente USD 2,000.00
    Entonces se exige autorización digital válida del supervisor antes del débito

  Escenario: Retiro supera saldo disponible
    Dado una sesión de caja ABIERTA
    Y la cuenta dispone de USD 300.00
    Cuando se solicita retirar USD 500.00
    Entonces la operación se rechaza por saldo insuficiente
    Y no se modifica la cuenta ni la gaveta

  Escenario: Retiro supera efectivo disponible en gaveta
    Dado una sesión de caja ABIERTA
    Y la gaveta dispone de USD 200.00
    Cuando se solicita retirar USD 250.00
    Entonces la operación se rechaza por efectivo insuficiente en gaveta
    Y no se debita la cuenta

  Escenario: Caja cerrada
    Dado una sesión de caja en estado CERRADA
    Cuando se solicita un depósito o retiro para esa sesión
    Entonces la operación se rechaza
    Y la cuenta y los saldos de caja permanecen sin cambios

  Escenario: Depósito exitoso
    Dado un cajero autorizado con una sesión de caja ABIERTA
    Cuando registra un depósito de USD 1,200.00 con cuenta y concepto válidos
    Entonces el servicio de transacciones confirma la acreditación
    Y se registra el ingreso en la sesión de caja
    Y se devuelve un comprobante único

  Escenario: Depósito superior a USD 5,000.00 sin declaración
    Dado una sesión de caja ABIERTA
    Cuando se intenta depositar USD 5,000.01 sin la declaración de origen lícito requerida
    Entonces el sistema rechaza la solicitud antes de acreditar la cuenta

  Escenario: Solicitud repetida con la misma clave de idempotencia
    Dado que una operación ya fue procesada con una clave de idempotencia
    Cuando el cliente reenvía la solicitud con la misma clave
    Entonces no se crea un segundo movimiento ni se repite el débito o crédito
    Y se devuelve el resultado previamente asociado a esa solicitud
```

---

## 7. Diseño Técnico y Contratos Propuestos

> Los contratos siguientes son propuestas documentales. Los nombres finales, el mecanismo de autorización y la compatibilidad con los servicios existentes quedan sujetos a revisión técnica.

### Endpoints

- `POST /api/v1/cajeros/operaciones/deposito`
- `POST /api/v1/cajeros/operaciones/retiro`
- Ambos requieren autenticación y rol `ROLE_CAJERO`.

### Request propuesto: depósito

```json
{
  "sesionCajaId": "<uuid-sesion>",
  "numeroCuenta": "<cuenta-de-prueba>",
  "monto": 1200.00,
  "concepto": "Depósito en ventanilla",
  "declaracionOrigenFondos": null
}
```

Para montos superiores a USD 5,000.00, `declaracionOrigenFondos` debe contener el registro exigido por RN-001.3. Los campos específicos deben revisarse con cumplimiento normativo; no se deben inventar datos del depositante ni incluir información personal real en ejemplos.

### Request propuesto: retiro

```json
{
  "sesionCajaId": "<uuid-sesion>",
  "numeroCuenta": "<cuenta-de-prueba>",
  "monto": 500.00,
  "cedula": "<identificacion-sintetica-de-prueba>",
  "autorizacionSupervisor": null
}
```

La autorización debe preferir un token o referencia verificable de autorización frente al envío o persistencia de un PIN en claro. La firma final del contrato es una decisión pendiente de diseño y seguridad.

### Response propuesto: operación confirmada

```json
{
  "movimientoId": "<uuid-movimiento>",
  "tipo": "RETIRO",
  "sesionCajaId": "<uuid-sesion>",
  "numeroCuenta": "<cuenta-enmascarada>",
  "monto": 500.00,
  "fecha": "<fecha-hora-utc>",
  "numeroComprobante": "<comprobante-unico>",
  "estado": "CONFIRMADA"
}
```

### Códigos HTTP esperados

| Código | Uso esperado |
|---|---|
| `200 OK` | Operación confirmada y comprobante disponible. |
| `400 Bad Request` | Campos faltantes o formato/monto inválido. |
| `401 Unauthorized` | No hay autenticación válida. |
| `403 Forbidden` | Rol no autorizado o falta/invalidación de autorización de supervisor. |
| `404 Not Found` | Sesión o recurso solicitado inexistente, de acuerdo con la política de API. |
| `409 Conflict` | Caja no operable, saldo/gaveta insuficiente o conflicto de idempotencia, según el contrato de errores que se apruebe. |
| `422 Unprocessable Entity` | Regla de negocio incumplida, si se adopta esta convención para errores de dominio. |
| `500 Internal Server Error` | Error inesperado; no debe revelar secretos ni detalles internos. |
| `503 Service Unavailable` | Servicio de transacciones no disponible o resultado no confirmable. |

La distinción entre `403`, `409` y `422` debe alinearse con el estándar de errores del proyecto. El requisito explícito del issue para falta de autorización del supervisor es `403`.

---

## 8. Datos y Trazabilidad del Movimiento

Cada movimiento confirmado debería conservar, como mínimo:

| Campo | Descripción / consideración |
|---|---|
| `Id` | Identificador único del movimiento. |
| `SesionCajaId` | Sesión en la que se atendió la operación. |
| `Tipo` | `DEPOSITO` o `RETIRO`. |
| `NumeroCuenta` | Referencia a la cuenta; restringir exposición en respuestas y logs. |
| `Monto` | Importe en USD, positivo y con precisión monetaria validada. |
| `CedulaCliente` | Necesaria para el retiro conforme a RN-001.2; tratar como dato personal protegido. |
| `Fecha` | Fecha/hora de registro normalizada a UTC. |
| `NumeroComprobante` | Identificador único e inmutable presentado al cliente. |
| `RequiereAprobacionSupervisor` | Indicador derivado de tipo y monto; no sustituye la evidencia de autorización. |
| `AprobadoPorSupervisorId` | Identidad del supervisor verificada, cuando aplique. |
| `ReferenciaTransaccion` | Identificador correlacionable de la operación en el servicio transaccional. |
| `ClaveIdempotencia` | Referencia no secreta para evitar el procesamiento duplicado. |
| `Estado` | Estado operativo explícito, por ejemplo pendiente, confirmada o fallida, según diseño aprobado. |

Los registros financieros no se borran ni se alteran para corregir errores. Deben limitarse los datos personales en logs, métricas, mensajes de error y comprobantes.

---

## 9. Seguridad, Privacidad y Auditoría

- Validar la identidad autenticada y el permiso `ROLE_CAJERO` en ambos endpoints; no confiar en un rol declarado por el cliente.
- Verificar que el cajero pueda operar la sesión de caja enviada.
- Para retiros de alto valor, validar autorización digital del supervisor y conservar identidad, fecha y referencia de autorización. El PIN o secreto no debe persistirse ni escribirse en logs.
- Enmascarar número de cuenta y proteger la cédula; aplicar el principio de mínimo privilegio a consultas y auditoría.
- Usar HTTPS en el transporte y evitar incluir credenciales o información sensible en URL, telemetría y trazas.
- Registrar quién solicitó la operación, quién la autorizó, el resultado y las referencias de correlación necesarias para investigar fallos, sin registrar secretos.
- Mantener movimientos confirmados append-only. Toda política de reverso requiere movimiento compensatorio trazable, no edición del asiento original.
- Aplicar idempotencia y protección contra solicitudes duplicadas, especialmente en reintentos de red o timeouts.

---

## 10. Casos de Error y Comportamiento Esperado

- **Sesión inexistente:** responder con error de recurso inexistente; no invocar el servicio transaccional.
- **Sesión cerrada:** rechazar antes de invocar el débito/crédito; no modificar cuenta ni caja.
- **Cuenta inexistente o inactiva:** informar rechazo de negocio sin divulgar información de terceros.
- **Saldo de cuenta insuficiente:** no completar el retiro ni alterar gaveta.
- **Efectivo insuficiente:** no debitar la cuenta.
- **Autorización de supervisor inválida, vencida o de otro contexto:** responder 403 y no ejecutar el débito.
- **Falla confirmada antes del débito/crédito:** responder como operación no completada y permitir un reintento seguro.
- **Timeout con resultado desconocido:** no afirmar éxito ni fracaso definitivo; permitir consulta/reconciliación usando el identificador de idempotencia.
- **Falla al registrar el asiento de caja después de confirmar transacción:** activar el mecanismo de consistencia acordado (por ejemplo, outbox/reconciliación/compensación). La estrategia requiere aprobación de arquitectura antes de implementación.

---

## 11. Decisiones Pendientes de Aprobación

1. Confirmar que el umbral de supervisor se aplica a `monto >= USD 2,000.00`; este documento sigue el criterio del issue #4.
2. Definir el mecanismo de autorización digital del supervisor y reemplazar, si corresponde, la propuesta inicial de PIN por una credencial/token verificable.
3. Definir formato, campos, resguardo y consulta de la declaración de origen lícito para depósitos superiores a USD 5,000.00.
4. Aprobar el formato uniforme de errores y la asignación definitiva de HTTP 409 frente a 422 para reglas de negocio.
5. Definir la estrategia de consistencia distribuida entre la caja y el servicio de transacciones, incluidas idempotencia, estados intermedios y reconciliación.
6. Confirmar formato de identificación y algoritmo aplicable para Ecuador, así como el servicio responsable de la validación del titular.
7. Definir retención, permisos y auditoría de datos personales y comprobantes conforme a las políticas del producto.

---

## 12. Criterio de Aprobación de la Especificación

La especificación podrá pasar a estado **Aprobado** cuando el responsable funcional y el responsable técnico revisen las decisiones pendientes que afectan contratos, autorización y consistencia financiera. Aprobar este documento no implica que la funcionalidad esté implementada ni desplegada.
