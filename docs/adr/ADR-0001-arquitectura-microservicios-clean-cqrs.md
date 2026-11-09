# ADR-0001: Arquitectura de Microservicios con Clean Architecture y CQRS

- **Estado:** Aceptado
- **Fecha:** 2026-11-09
- **Autores:** Equipo de Arquitectura Banco Chillo
- **Decisores:** Tech Lead, Arquitecto de Software

---

## 1. Contexto y Planteamiento del Problema

Banco Chillo requiere una plataforma bancaria robusta con separación de dominios para operaciones de ventanilla (cajeros), transacciones financieras de alta concurrencia, gestión de cuentas de socios, evaluación de créditos y trazabilidad de auditoría. Una arquitectura monolítica representaría un cuello de botella para escalar componentes transaccionales críticos de manera independiente y aumentaría el riesgo de acoplamiento.

---

## 2. Opciones Consideradas

- **Opción A: Monolito modular en .NET**: Fácil de desplegar inicialmente, pero acopla el escalamiento de ventanilla con transacciones masivas y reportes pesados.
- **Opción B: Microservicios en .NET con Clean Architecture y CQRS**: Separación estricta de dominios, escalabilidad independiente, separación de modelos de lectura y escritura, e interoperabilidad asíncrona mediante eventos de dominio.

---

## 3. Decisión

Se adopta la **Opción B**: Microservicios independientes en **.NET 9/10**, donde cada servicio implementa:
1. **Clean Architecture**:
   - `Domain`: Entidades, Value Objects, eventos de dominio y reglas de negocio puras.
   - `Application`: Commands, Queries, Handlers (MediatR), validaciones con FluentValidation y contratos.
   - `Infrastructure`: Persistencia con EF Core / Dapper, clientes de mensajería y adaptadores externos.
   - `Api`: Controllers o Minimal APIs, autenticación y middlewares.
2. **Patrón CQRS**: Segregación de operaciones de mutación (Commands) y consultas optimizadas (Queries).
3. **Outbox Pattern**: Para publicación confiable de eventos transaccionales al Event Bus (RabbitMQ).

---

## 4. Consecuencias

### Positivas
- Aislamiento de fallos: un incidente en el módulo de créditos no interrumpe la operatividad de los cajeros ni de las transferencias.
- Despliegue granular y optimización de recursos en la nube.
- Adherencia total a los principios SOLID y alta cobertura de pruebas automatizadas.

### Trade-offs y Mitigaciones
- Mayor complejidad en despliegue y orquestación local: mitigada con `docker-compose.yml` y contratos compartidos en `BancoChillo.BuildingBlocks`.
