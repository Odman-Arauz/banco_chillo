# TASKS: SPEC-0009 Auditoría Inmutable

Lista de tareas derivada de `plan.md`. Marca cada casilla al completarla.

## Fase 0: Documentación (entrega actual)

- [ ] T-01 Crear la carpeta `docs/specs/spec-0009-auditoria-inmutable/` a partir de `docs/specs/_plantilla/`
- [ ] T-02 Revisar `docs/adr/ADR-0003-flujo-cajeros-y-arqueo-inmutable.md` y alinear la especificación
- [ ] T-03 Revisar `docs/procedimientos/guia-desarrollo-juniors.md`
- [ ] T-04 Redactar `spec.md` (historia, alcance, reglas, contrato API, Gherkin, riesgos)
- [ ] T-05 Redactar `plan.md` (arquitectura, capas, decisiones, pruebas)
- [ ] T-06 Redactar `tasks.md`
- [ ] T-07 Resolver o registrar los puntos abiertos de `spec.md`
- [ ] T-08 Abrir PR de tipo `docs` hacia `develop` con `Closes #9`

## Fase 1: Domain (implementación futura)

- [ ] T-10 Crear el enum `TipoOperacion`
- [ ] T-11 Crear la entidad `RegistroAuditoria` (sin `Modificar()` ni `Eliminar()`)
- [ ] T-12 Implementar `CalculadorHashIntegridad` (SHA-256 encadenado, serialización canónica)
- [ ] T-13 Pruebas unitarias del cálculo del hash encadenado

## Fase 2: Application

- [ ] T-20 Definir la interfaz `IRegistroAuditoriaRepository` (solo agregar y consultar)
- [ ] T-21 Handler de `CajaAbiertaEvent`
- [ ] T-22 Handler de `TransaccionCompletadaEvent`
- [ ] T-23 Handler de `SesionIniciadaEvent`
- [ ] T-24 Caso de uso de consulta con filtros y paginación
- [ ] T-25 Pruebas unitarias de mapeo evento → registro

## Fase 3: Infrastructure

- [ ] T-30 Configuración EF Core y mapeo a `auditoria_bc.bitacora_operaciones` (columnas `jsonb`)
- [ ] T-31 Migración con particionamiento mensual por `FechaTimestamp` e índices
- [ ] T-32 Rol de base de datos solo con `INSERT`/`SELECT` y trigger que bloquee `UPDATE`/`DELETE`
- [ ] T-33 Consumidor RabbitMQ (cola durable, `ack` manual, reintentos y dead-letter)
- [ ] T-34 Deduplicación por ID de evento
- [ ] T-35 Serialización del cálculo del hash (control de concurrencia)

## Fase 4: Api

- [ ] T-40 Endpoint `GET /api/v1/auditoria/consultas` con filtros
- [ ] T-41 Autorización `ROLE_SUPERADMIN` / `ROLE_AUDITOR`
- [ ] T-42 Respuesta 405 para `PUT`, `PATCH`, `DELETE` y `POST`
- [ ] T-43 Documentación OpenAPI

## Fase 5: Pruebas y cierre

- [ ] T-50 Prueba de integración: evento consumido y persistido en menos de 1 segundo
- [ ] T-51 Prueba: evento duplicado no genera un segundo registro
- [ ] T-52 Pruebas de API: 405, 403 y filtros
- [ ] T-53 (Opcional) Proceso de verificación de integridad de la cadena
- [ ] T-54 Revisión de código y PR con `Closes #9`

> **Nota:** como la entrega actual es solo documentación, las fases 1 a 5 quedan planificadas y se ejecutarán en una etapa posterior.
