# Tareas de Ejecución: SPEC-0001 (Esquema auth_bc, RLS y Semillas)

- **Issue:** #16
- **Rama:** `feat/16-esquema-auth`

---

## Tareas

- [x] **T1: Especificación y Documentación de Arquitectura**
  - Crear carpeta `docs/specs/spec-0001-autenticacion-mfa/`.
  - Redactar `spec.md` con historias de usuario, reglas de negocio y criterios de aceptación.
  - Diseñar diagrama ER Mermaid y documentar justificación de RLS en `plan.md`.

- [x] **T2: Completar Esquema DDL en `01-auth-schema.sql`**
  - Modificar `auth_bc.usuarios` incorporando `CHECK (estado IN ('ACTIVO', 'BLOQUEADO', 'INACTIVO'))` y columna `bloqueado_hasta`.
  - Crear tabla `auth_bc.refresh_tokens` con clave primaria UUID, `usuario_id`, `token_hash` único y expiración.
  - Crear tabla `auth_bc.intentos_acceso` con clave secuencial `BIGSERIAL`, `usuario_id`, identificación ingresada, estado booleano y motivo.
  - Crear trigger e inmutabilidad append-only (`auth_bc.impedir_modificacion()`) para rechazar `UPDATE` y `DELETE`.
  - Crear índices para `intentos_acceso(usuario_id, creado_en DESC)` y `refresh_tokens(usuario_id)`.

- [x] **T3: Seguridad a Nivel de Fila (RLS) en `02-auth-rls.sql`**
  - Habilitar RLS en las 5 tablas del esquema `auth_bc`.

- [x] **T4: Actualización de Catálogos y Constantes de Seguridad**
  - Agregar `ROLE_AUDITOR` en `database/seeds/01-catalogos-financieros.sql`.
  - Agregar constante `Auditor = "ROLE_AUDITOR"` en `RolesBancarios.cs`.

- [x] **T5: Semillas de Usuarios Demo en `02-auth-usuarios-demo.sql`**
  - Generar 7 usuarios ficticios (uno por rol) con cédulas módulo 10 válidas de provincia 30.
  - Generar hash BCrypt de la contraseña `Demo#2026`.
  - Asociar roles en `auth_bc.usuario_roles` mediante subconsultas idempotentes.

- [x] **T6: Validación y Pruebas Locales (PostgreSQL 16 + .NET)**
  - Inicializar PostgreSQL 16 local y verificar idempotencia en doble ejecución.
  - Ejecutar prueba de fallo en `DELETE FROM auth_bc.intentos_acceso;`.
  - Ejecutar `dotnet build` y `dotnet test`.

- [ ] **T7: Configuración de Proyecto Supabase y Apertura de PR**
  - Crear proyecto `banco-chillo` en Supabase y copiar Session Pooler URI.
  - Registrar secreto `SUPABASE_DB_URL` en GitHub Actions.
  - Subir rama `feat/16-esquema-auth` y abrir Pull Request hacia `develop`.
