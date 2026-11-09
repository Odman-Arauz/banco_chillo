# AGENTS.md

Reglas y lineamientos para agentes de Inteligencia Artificial y desarrolladores humanos en **Banco Chillo**.
Idioma obligatorio: **Español** en código, documentación, commits, Pull Requests y mensajes de soporte.

---

## 1. Visión del Proyecto

**Banco Chillo** es una plataforma financiera integral que comprende:
1. **Banca Web PWA (React Native / Expo Web)**: Experiencia responsive, segura y offline-first tanto para clientes/socios como para personal administrativo de agencias.
2. **Backend de Microservicios (.NET 9/10 C#)**: Arquitectura limpia (Clean Architecture), CQRS con MediatR y comunicación asíncrona orientada a eventos.
3. **Módulo Transaccional y de Cajeros**: Control operativo de ventanilla, apertura de caja, depósitos, retiros con doble validación, cuadres y arqueo de billetes/monedas con cierre de jornada.
4. **Base de Datos y Persistencia**: PostgreSQL relacional de alta fidelidad transaccional con proyección de despliegue en **Supabase** y Row Level Security (RLS).
5. **Ciclo de Desarrollo**: 17 semanas divididas en 4 fases estratégicas.

---

## 2. Stack Tecnológico Confirmado

- **Frontend PWA**: React Native (React Native for Web / Expo SDK), TypeScript, Zustand/Redux Toolkit, Tailwind/NativeWind, Service Workers para capacidades PWA.
- **Backend**: .NET 9/10 C# con Clean Architecture (`Domain`, `Application`, `Infrastructure`, `Api`) por cada microservicio, xUnit/FluentAssertions para pruebas.
- **Gateway & Resiliencia**: YARP (Yet Another Reverse Proxy) / .NET Gateway, Polly (Circuit Breaker, Retry, Timeout).
- **Mensajería & Eventos**: RabbitMQ / EventBus con Outbox Pattern para garantizar consistencia transaccional eventual.
- **Base de Datos**: PostgreSQL 16+ / Supabase con migraciones versionadas y scripts idempotentes.
- **Caché y Sesiones**: Redis para tokens temporales, rate limiting y sesiones bancarias.

---

## 3. Comandos Esenciales

```bash
# Backend (.NET)
dotnet build src/services/auth/BancoChillo.Auth.Api
dotnet test
dotnet format

# Frontend PWA (React Native / Expo)
cd src/apps/pwa-banca
npm run web           # Modo desarrollo web PWA
npm run test          # Pruebas unitarias Jest
npm run lint

# Infraestructura Local (Docker)
docker compose up -d postgres redis rabbitmq
```

---

## 4. Política de Ramas y Commits

- **`main`**: Rama de producción protegida. Solo recibe merges mediante Pull Request aprobado desde `develop` o `hotfix/*`. Push directo **estrictamente prohibido**.
- **`develop`**: Rama base de integración y desarrollo. Recibe Pull Requests desde ramas de características.
- **Nomenclatura de ramas**:
  - `feat/<issue-id>-<descripcion-corta>` (ej. `feat/10-apertura-caja-cajero`)
  - `fix/<issue-id>-<descripcion-corta>`
  - `chore/<issue-id>-<descripcion-corta>`
  - `docs/<issue-id>-<descripcion-corta>`
- **Conventional Commits**:
  - `feat: agregar endpoint de arqueo de caja`
  - `fix: corregir redondeo en tabla de amortización francesa`
  - `docs: actualizar diagrama c4 de microservicio de pagos`
  - `test: agregar pruebas unitarias de cálculo de débitos`

---

## 5. Reglas de Oro Bancarias (No Negociables)

1. **PROHIBIDO EL USO DE DATOS REALES**:
   - Ninguna cédula, cuenta bancaria, nombre, tarjeta, clave o correo real debe figurar en código, semillas, tests ni documentación.
   - Usar generadores de datos ficticios evidentes (ej. cédulas que cumplan dígito verificador pero con números de prueba).
2. **CERO SECRETOS EN EL REPOSITORIO**:
   - Jamás commitear credenciales, connection strings reales, llaves JWT privadas ni API keys. Usar variables de entorno y validar con `.gitleaks.toml`.
3. **ARQUITECTURA DE CAJERO Y ARQUEO INMUTABLE**:
   - Todo movimiento en caja (ingreso o egreso) debe registrarse en un asiento contable/transaccional inmutable (Append-only). No existe `UPDATE` ni `DELETE` sobre registros contables o transacciones ejecutadas.
4. **DECISIONES DE DISEÑO EN ADRs**:
   - Cualquier cambio en la arquitectura o nuevo patrón debe documentarse como un Architecture Decision Record en `docs/adr/`.
5. **ESPECIFICACIONES ANTES DE CODIFICAR**:
   - No implementar módulos complejos sin una especificación aprobada en `docs/specs/`.
