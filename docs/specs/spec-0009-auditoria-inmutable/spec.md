# SPEC-0009: Microservicio de Auditoría Inmutable y Registro de Eventos Financieros

| Campo | Valor |
|---|---|
| Issue | #9 |
| Rama | `feat/9-auditoria-inmutable` (desde `develop`) |
| Servicio | `src/services/auditoria/` |
| ADR de referencia | `docs/adr/ADR-0003-flujo-cajeros-y-arqueo-inmutable.md` |
| Estado | Borrador para revisión |

## 1. Historia de usuario

Como **Auditor de Cumplimiento y Seguridad Bancaria**, quiero registrar de manera inmutable (append-only) cada evento financiero y administrativo con su usuario, IP, timestamp y payload previo y posterior, para fines legales y regulatorios.

## 2. Alcance

**Incluye**
- Consumo de eventos publicados por los demás microservicios vía RabbitMQ.
- Almacenamiento append-only de cada evento en PostgreSQL (`auditoria_bc.bitacora_operaciones`).
- Hash de integridad SHA-256 encadenado entre registros consecutivos.
- API de consulta de solo lectura con filtros (fecha, usuario, tipo de operación).
- Control de acceso por roles (`ROLE_SUPERADMIN`, `ROLE_AUDITOR`).

**No incluye**
- Modificación o eliminación de registros bajo ninguna circunstancia.
- Generación de reportes regulatorios (PDF/Excel).
- Alertas en tiempo real ni detección de fraude.
- Cambios en los microservicios emisores, más allá de publicar sus eventos.

## 3. Actores

| Actor | Descripción |
|---|---|
| Servicios emisores | Cajeros, Transacciones y Seguridad/Autenticación: publican eventos en RabbitMQ. |
| Servicio de Auditoría | Consume, valida, encadena y persiste los eventos. |
| Auditor / Superadmin | Consulta los registros mediante la API. |

## 4. Eventos consumidos y tipo de operación

| Evento | Emisor | `TipoOperacion` |
|---|---|---|
| `CajaAbiertaEvent` | Cajeros | `APERTURA_CAJA` |
| `TransaccionCompletadaEvent` | Transacciones | `DEPOSITO`, `RETIRO` o `TRANSFERENCIA` (según el contenido del evento) |
| `SesionIniciadaEvent` | Seguridad | `LOGIN` |

## 5. Modelo de datos: `RegistroAuditoria`

| Propiedad | Tipo | Descripción |
|---|---|---|
| `Id` | Guid | Identificador único del registro. |
| `UsuarioId` | Guid | Usuario que ejecutó la operación. |
| `TipoOperacion` | enum | `APERTURA_CAJA`, `DEPOSITO`, `RETIRO`, `TRANSFERENCIA`, `LOGIN`. |
| `DireccionIp` | string | IP de origen. |
| `UserAgent` | string | Agente de usuario del cliente. |
| `FechaTimestamp` | DateTime (UTC) | Momento en que ocurrió el evento. |
| `DatosAnterioresJson` | jsonb | Estado previo (nulo si no aplica, p. ej. en `LOGIN`). |
| `DatosNuevosJson` | jsonb | Estado posterior o payload completo del evento. |
| `HashIntegridad` | string (64 hex) | SHA-256 encadenado con el registro anterior. |

> **Propuesta a validar:** agregar `HashAnterior` (hash del registro previo) para poder verificar la cadena de forma independiente. El issue solo lista `HashIntegridad`.

## 6. Reglas de negocio e inmutabilidad

| ID | Regla |
|---|---|
| RA-01 | La entidad no expone métodos `Modificar()` ni `Eliminar()`: es de solo lectura y solo inserción. |
| RA-02 | Cada registro guarda un hash SHA-256 calculado con el hash del registro anterior, de modo que alterar un registro invalida toda la cadena posterior. |
| RA-03 | El primer registro usa un hash "génesis" fijo (64 ceros). |
| RA-04 | Un mismo evento no se registra dos veces: se deduplica por identificador del evento (idempotencia ante reentregas de RabbitMQ). |
| RA-05 | La API no ofrece ninguna operación de escritura: `PUT`, `PATCH`, `DELETE` (y `POST`) responden 405. |
| RA-06 | Solo `ROLE_SUPERADMIN` y `ROLE_AUDITOR` pueden consultar. |
| RA-07 | Todos los timestamps se guardan en UTC. |

## 7. Cálculo del hash encadenado

```
HashIntegridad(n) = SHA256( HashIntegridad(n-1)
                            | Id | UsuarioId | TipoOperacion
                            | DireccionIp | UserAgent
                            | FechaTimestamp (ISO-8601 UTC)
                            | DatosAnterioresJson | DatosNuevosJson )
```

- El separador `|` y el orden de los campos son fijos y se documentan para que la verificación sea reproducible.
- El JSON se serializa de forma canónica (mismo orden de propiedades, sin espacios sobrantes) para que el mismo contenido dé siempre el mismo hash.

## 8. Contrato de la API

### `GET /api/v1/auditoria/consultas`

| Parámetro | Tipo | Obligatorio | Descripción |
|---|---|---|---|
| `fechaDesde` | fecha | No | Inicio del rango (UTC). |
| `fechaHasta` | fecha | No | Fin del rango (UTC). |
| `usuarioId` | Guid | No | Filtra por usuario. |
| `tipoOperacion` | enum | No | Filtra por tipo de operación. |
| `pagina`, `tamanoPagina` | int | No | Paginación (tamaño máximo a definir). |

**Respuestas**

| Código | Situación |
|---|---|
| 200 | Lista paginada de registros. |
| 400 | Filtros inválidos (p. ej. `fechaDesde` > `fechaHasta`). |
| 401 | Sin autenticación. |
| 403 | Rol sin permiso. |
| 405 | Cualquier método distinto de `GET` sobre los registros. |

## 9. Criterios de aceptación (Gherkin)

```gherkin
Escenario: Registro de una transacción exitosa
  Dado una transacción realizada con éxito en el sistema
  Cuando se publica el evento en RabbitMQ
  Entonces el servicio de auditoría lo consume
  Y lo almacena con su payload completo en formato JSON en menos de 1 segundo

Escenario: Intento de modificar o eliminar registros
  Dado un intento de llamada HTTP DELETE o PUT sobre los registros de auditoría
  Cuando llega al servidor
  Entonces la API responde HTTP 405 Method Not Allowed

Escenario: Encadenamiento de hashes
  Dado un registro de auditoría existente con su hash
  Cuando se almacena un nuevo evento
  Entonces su HashIntegridad se calcula incluyendo el hash del registro anterior

Escenario: Consulta con rol no autorizado
  Dado un usuario autenticado sin rol ROLE_AUDITOR ni ROLE_SUPERADMIN
  Cuando solicita GET /api/v1/auditoria/consultas
  Entonces la API responde HTTP 403 Forbidden

Escenario: Evento duplicado
  Dado un evento ya registrado
  Cuando RabbitMQ lo vuelve a entregar
  Entonces el servicio no crea un registro adicional
```

## 10. Requisitos no funcionales

- **Rendimiento:** persistir cada evento en menos de 1 segundo desde su consumo.
- **Disponibilidad:** si el servicio está caído, los eventos permanecen en la cola y se procesan al recuperarse (cola durable, confirmaciones manuales).
- **Seguridad:** el rol de base de datos del servicio solo tiene `INSERT` y `SELECT` sobre la tabla; sin `UPDATE` ni `DELETE`. Los payloads no deben incluir datos sensibles sin enmascarar (PIN, contraseñas, números completos de tarjeta).
- **Escalabilidad:** particionamiento mensual por fecha para consultas rápidas con alto volumen.
- **Trazabilidad:** conservar los registros según la política de retención regulatoria (por definir).

## 11. Riesgos

| Riesgo | Mitigación |
|---|---|
| Concurrencia: dos eventos simultáneos toman el mismo "hash anterior" y rompen la cadena. | Serializar el cálculo (un único consumidor por cola o bloqueo transaccional al leer el último hash). |
| Reentrega de mensajes duplicados. | Deduplicación por ID de evento (RA-04). |
| Alteración manual en la base de datos. | Permisos restringidos y verificación periódica de la cadena de hashes. |
| Datos sensibles en el payload. | Enmascarar campos sensibles antes de publicar el evento. |
| Particionamiento y cadena de hashes. | La cadena es lógica (por orden de inserción) y no depende de la partición; se documenta cómo se verifica entre particiones. |

## 12. Puntos abiertos

- Confirmar en `ADR-0003` el alcance de "arqueo inmutable" y si impone restricciones adicionales.
- Definir política de retención y tamaño máximo de página.
- Confirmar si se agrega `HashAnterior` a la entidad.
