# SPEC-0001: Autenticación Bancaria, Gestión de Sesiones y Control de Acceso RBAC

| Campo | Valor |
|---|---|
| Issue | #16 (Parte A - Base de datos y esquema) / Épica #1 |
| Rama | `feat/16-esquema-auth` (desde `develop`) |
| Servicio | `src/services/auth/` |
| ADRs de referencia | `docs/adr/ADR-0005-despliegue-infraestructura-nube.md`, `docs/adr/ADR-0007-esquema-por-microservicio.md` |
| Estado | Aprobado |

---

## 1. Propósito y Alcance

Definir e implementar el modelo de persistencia y seguridad relacional para el microservicio de Autenticación (`auth_bc`). 
Este esquema soporta:
- Autenticación mediante credenciales con hash seguro (BCrypt).
- Gestión de tokens de actualización (*Refresh Tokens*) para persistencia de sesión segura sin exponer el secreto.
- Bitácora inmutable (append-only) de auditoría para registrar cada intento de acceso exitoso o fallido.
- Política de bloqueo temporal tras 3 intentos fallidos consecutivos de inicio de sesión.
- Asignación de roles bancarios basados en RBAC (*Role-Based Access Control*).
- Protección de integridad con Row Level Security (RLS) en Supabase / PostgreSQL.

---

## 2. Historias de Usuario Asociadas

- **US-AUTH-01:** Como **Usuario Bancario** (Socio, Cajero o Funcionario), quiero iniciar sesión de forma segura y renovar mi sesión con tokens cifrados para operar en la plataforma sin reintroducir mis credenciales constantemente.
- **US-AUTH-02:** Como **Oficial de Seguridad Bancaria**, quiero que la plataforma bloquee temporalmente cuentas con más de 3 intentos fallidos consecutivos para mitigar ataques de fuerza bruta.
- **US-AUTH-03:** Como **Auditor de Cumplimiento**, quiero un registro append-only inviolable de todos los intentos de acceso para garantizar la trazabilidad forense de quién intentó entrar, cuándo, desde qué IP y cuál fue el resultado.

---

## 3. Reglas de Negocio Aplicables

| Código | Regla |
|---|---|
| **RN-AUTH-01** | **Estados de Usuario Permitidos:** Los estados válidos para una cuenta son estrictamente `ACTIVO`, `BLOQUEADO` e `INACTIVO`. |
| **RN-AUTH-02** | **Bloqueo Temporal:** Tras 3 intentos fallidos consecutivos de ingreso de clave, el usuario pasa a estado `BLOQUEADO` y se establece la marca temporal `bloqueado_hasta`. |
| **RN-AUTH-03** | **Hash de Refresh Tokens:** Los tokens de refresco jamás se almacenan en texto plano en la base de datos; únicamente se almacena su resumen criptográfico SHA-256 (`token_hash` de hasta 128 caracteres). |
| **RN-AUTH-04** | **Inmutabilidad de Intentos de Acceso:** La tabla `auth_bc.intentos_acceso` es estrictamente append-only. Ningún usuario ni proceso puede ejecutar `UPDATE` o `DELETE` sobre sus registros. |
| **RN-AUTH-05** | **Aislamiento RLS en Supabase:** Todas las tablas del esquema `auth_bc` tienen habilitado Row Level Security (RLS). Dado que los esquemas `*_bc` no se exponen a la API pública de Supabase (PostgREST) y los microservicios se conectan mediante cadenas directas con el rol de aplicación, activar RLS sin políticas bloquea por defecto cualquier intento de consulta no autorizada vía API externa. |

---

## 4. Criterios de Aceptación (Gherkin)

```gherkin
Escenario: Inicialización limpia del esquema y semillas
  Dado un volumen de Postgres nuevo
  Cuando ejecuto docker compose up -d
  Entonces el log termina en "Base de datos de Banco Chillo inicializada" sin errores
  Y existen 5 tablas en auth_bc y 7 usuarios de prueba con su rol respectivo

Escenario: Bitácora inmutable de intentos de acceso
  Dado un registro existente en auth_bc.intentos_acceso
  Cuando intento ejecutar un UPDATE o DELETE sobre dicho registro
  Entonces PostgreSQL rechaza la operación con una excepción indicando que la tabla es inmutable

Escenario: Idempotencia de scripts
  Cuando ejecuto por segunda vez los scripts de esquema y semillas
  Entonces la ejecución no produce errores ni genera registros duplicados
```

---

## 5. Diseño del Modelo de Datos

Las tablas pertenecientes al esquema `auth_bc` son:

1. `auth_bc.roles`: Catálogo de roles del sistema financiero (`ROLE_SUPERADMIN`, `ROLE_ADMIN`, `ROLE_SUPERVISOR_CAJA`, `ROLE_CAJERO`, `ROLE_OFICIAL_CREDITO`, `ROLE_SOCIO`, `ROLE_AUDITOR`).
2. `auth_bc.usuarios`: Registro de identidades, identificador legal (Cédula/RUC), correo, teléfono, hash de contraseña, estado, contador de intentos fallidos y fecha de bloqueo.
3. `auth_bc.usuario_roles`: Asociación N:M entre usuarios y roles.
4. `auth_bc.refresh_tokens`: Tokens de refresco con hash SHA-256, expiración, revocación y metadatos del cliente (IP, User Agent).
5. `auth_bc.intentos_acceso`: Bitácora inmutable de eventos de login con usuario, IP, motivo y resultado booleano.
