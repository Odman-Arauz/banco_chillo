# PLAN: SPEC-0009 Auditoría Inmutable

Plan técnico de implementación del servicio `src/services/auditoria/`. Este documento describe **cómo** se construirá lo definido en `spec.md`.

## 1. Arquitectura general

```
[Cajeros] [Transacciones] [Seguridad]
      \          |           /
       \         |          /
        ---> RabbitMQ (exchange de eventos) ---> cola durable de auditoría
                                                        |
                                                        v
                                         BancoChillo.Auditoria (consumidor)
                                                        |
                                    calcula hash SHA-256 encadenado
                                                        |
                                                        v
                              PostgreSQL: auditoria_bc.bitacora_operaciones
                                                        ^
                                                        |
                          BancoChillo.Auditoria.Api (GET /api/v1/auditoria/consultas)
```

El servicio sigue la misma estructura en capas que el resto del proyecto: Domain, Application, Infrastructure y Api.

## 2. Capas

### 2.1 `BancoChillo.Auditoria.Domain`
- Entidad `RegistroAuditoria` con las propiedades definidas en `spec.md`.
- Enum `TipoOperacion`: `APERTURA_CAJA`, `DEPOSITO`, `RETIRO`, `TRANSFERENCIA`, `LOGIN`.
- Constructor/fábrica de creación; **sin** métodos `Modificar()` ni `Eliminar()`; propiedades con `private set`.
- Servicio de dominio `CalculadorHashIntegridad` (SHA-256 encadenado, serialización canónica).

### 2.2 `BancoChillo.Auditoria.Application`
- Handlers/consumidores para `CajaAbiertaEvent`, `TransaccionCompletadaEvent` y `SesionIniciadaEvent`.
- Mapeo evento → `RegistroAuditoria` (incluye determinar el `TipoOperacion`).
- Caso de uso de consulta con filtros (fecha, usuario, tipo de operación) y paginación.
- Interfaces: `IRegistroAuditoriaRepository` (solo `Agregar` y consultas; sin actualizar ni borrar).

### 2.3 `BancoChillo.Auditoria.Infrastructure`
- Configuración EF Core para PostgreSQL: esquema `auditoria_bc`, tabla `bitacora_operaciones`.
- Columnas JSON como `jsonb`.
- Particionamiento por rango de fecha (mensual) sobre `FechaTimestamp`; la clave primaria incluye la columna de partición.
- Índices por `FechaTimestamp`, `UsuarioId` y `TipoOperacion`.
- Consumidor RabbitMQ con cola durable y confirmación manual (`ack` solo tras persistir).
- Restricciones de base de datos: el rol del servicio con permisos solo de `INSERT` y `SELECT`, y un trigger que rechace `UPDATE`/`DELETE`.

### 2.4 `BancoChillo.Auditoria.Api`
- Controlador con un único `GET /api/v1/auditoria/consultas`.
- Autorización `ROLE_SUPERADMIN` o `ROLE_AUDITOR`.
- Respuesta 405 para `PUT`, `PATCH`, `DELETE` y `POST` sobre la ruta.
- Documentación del contrato en Swagger/OpenAPI.

## 3. Flujo de procesamiento de un evento

1. Un microservicio publica el evento en RabbitMQ.
2. El consumidor recibe el mensaje y valida que traiga los campos mínimos.
3. Se verifica que el evento no esté ya registrado (idempotencia por ID de evento).
4. En una transacción de base de datos: se lee el último `HashIntegridad`, se calcula el nuevo hash y se inserta el registro.
5. Se confirma el mensaje (`ack`). Si falla, se reintenta; tras N intentos pasa a una cola de mensajes fallidos (dead-letter) para revisión.

## 4. Decisiones de diseño

| Tema | Decisión | Motivo |
|---|---|---|
| Inmutabilidad | Doble capa: dominio sin métodos de cambio y permisos de BD restringidos | Un solo control no basta si alguien accede directo a la base. |
| Orden de la cadena | Cálculo serializado (bloqueo al leer el último hash) | Evita que dos eventos usen el mismo hash previo. |
| Hash | SHA-256 con serialización canónica | Reproducible y verificable. |
| Duplicados | Deduplicación por ID de evento | RabbitMQ garantiza entrega "al menos una vez". |
| Fallos | Cola dead-letter tras reintentos | No se pierden eventos ni se bloquea la cola. |
| Particionamiento | Mensual por fecha | Consultas rápidas y mantenimiento sencillo. |

## 5. Estrategia de pruebas

- **Unitarias (obligatorias según el issue):** cálculo del hash encadenado SHA-256: mismo contenido da mismo hash; un cambio en cualquier campo cambia el hash; el hash depende del anterior; el primer registro usa el hash génesis.
- **Unitarias:** mapeo de cada evento a su `TipoOperacion`.
- **Integración:** consumo de un evento y persistencia en menos de 1 segundo; evento duplicado no genera un segundo registro.
- **API:** `PUT` y `DELETE` responden 405; rol sin permiso responde 403; filtros de consulta.

## 6. Verificación de integridad

Se prevé un proceso de verificación que recorre los registros en orden, recalcula cada hash y compara con el almacenado. Si hay diferencia, indica el primer registro alterado. Queda como mejora futura y se anota en `spec.md` como punto abierto.

## 7. Dependencias y supuestos

- Los demás microservicios ya publican (o publicarán) los eventos con el contrato acordado: ID de evento, usuario, IP, user agent, timestamp, datos previos y nuevos.
- Existe infraestructura de RabbitMQ y PostgreSQL en el entorno.
- La autenticación y los roles los provee el servicio de seguridad existente.
- Se consultó `docs/adr/ADR-0003-flujo-cajeros-y-arqueo-inmutable.md` para alinear el diseño.

## 8. Flujo Git

- Rama: `feat/9-auditoria-inmutable`, creada desde `develop`.
- Commits pequeños por tipo de cambio.
- PR hacia `develop` con `Closes #9` en la descripción.
