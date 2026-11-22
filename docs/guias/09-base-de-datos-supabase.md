# 09 · Base de datos: local (Docker) y Supabase

## Convenciones (ADR-0007) — léelas antes de escribir SQL

1. **Un esquema por microservicio**; tus tablas van en tu carpeta:
   `database/schema/0X-<modulo>/01-<modulo>-schema.sql` (y `02-...sql` si necesitas más archivos).
2. **Idempotente:** `CREATE SCHEMA IF NOT EXISTS`, `CREATE TABLE IF NOT EXISTS`, `CREATE INDEX IF NOT EXISTS`,
   `INSERT ... ON CONFLICT DO NOTHING`, `CREATE OR REPLACE FUNCTION`, `DROP TRIGGER IF EXISTS` + `CREATE TRIGGER`.
3. **Nombres** en `snake_case` y español: `sesiones_caja`, `fecha_apertura`. Tablas en plural.
4. **Claves:** `UUID DEFAULT gen_random_uuid()` para entidades de negocio; `SERIAL` para catálogos.
5. **Dinero:** `NUMERIC(14,2)` (nunca `FLOAT`). Tasas: `NUMERIC(7,4)`.
6. **Fechas:** `TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP`.
7. **Estados:** `VARCHAR(20)` + `CHECK (estado IN (...))`.
8. **Sin FK hacia otros esquemas:** usa una columna con comentario `-- referencia lógica a auth_bc.usuarios`.
9. **Append-only** para movimientos, transacciones y auditoría: trigger que impide `UPDATE`/`DELETE`.
10. **Semillas ficticias** en `database/seeds/0X-<modulo>-semillas.sql`.

### Plantilla de tabla append-only (cópiala)

```sql
CREATE SCHEMA IF NOT EXISTS cuentas_bc;

CREATE TABLE IF NOT EXISTS cuentas_bc.movimientos (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cuenta_id UUID NOT NULL REFERENCES cuentas_bc.cuentas(id),
    tipo VARCHAR(10) NOT NULL CHECK (tipo IN ('CREDITO', 'DEBITO')),
    monto NUMERIC(14, 2) NOT NULL CHECK (monto > 0),
    saldo_resultante NUMERIC(14, 2) NOT NULL,
    referencia VARCHAR(100) NOT NULL,
    creado_en TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Función reutilizable que bloquea UPDATE/DELETE
CREATE OR REPLACE FUNCTION cuentas_bc.impedir_modificacion()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'La tabla %.% es inmutable (append-only): no se permite %',
        TG_TABLE_SCHEMA, TG_TABLE_NAME, TG_OP;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_movimientos_inmutables ON cuentas_bc.movimientos;
CREATE TRIGGER trg_movimientos_inmutables
    BEFORE UPDATE OR DELETE ON cuentas_bc.movimientos
    FOR EACH ROW EXECUTE FUNCTION cuentas_bc.impedir_modificacion();
```

### Cédulas ficticias válidas

Algoritmo módulo 10 (cédula ecuatoriana): usa números claramente de prueba, por ejemplo provincia `30`
o secuencias como `1710034065`, y nombres como "Socio Demo Uno". **Nunca** la cédula de una persona real.

## Probar tu script en local

```powershell
docker compose down -v
docker compose up -d
docker logs banco_chillo_postgres      # debe decir ">> Ejecutando .../tu-archivo.sql" y terminar en "inicializada"
```

Explorar con `psql` dentro del contenedor:

```powershell
docker exec -it banco_chillo_postgres psql -U banco_admin -d banco_chillo_dev
```

```sql
\dn                         -- listar esquemas
\dt cuentas_bc.*            -- tablas de tu esquema
\d cuentas_bc.movimientos   -- columnas de una tabla
UPDATE cuentas_bc.movimientos SET monto = 1;  -- debe FALLAR si es append-only
\q
```

O con **DBeaver**: Nueva conexión → PostgreSQL → host `localhost`, puerto `5432`, BD `banco_chillo_dev`,
usuario `banco_admin`, clave `solo_desarrollo_local`.

**Diagrama ER:** en DBeaver, clic derecho sobre el esquema → *View Diagram* → exporta PNG a `docs/diagramas/er-<modulo>.png`.
También puedes escribirlo en Mermaid (`erDiagram`) dentro de tu spec.

## Supabase (base en la nube)

> Solo el **responsable de Auth** (dueño del proyecto Supabase) y el docente tienen la contraseña.
> Los demás trabajan en local; sus scripts llegan a Supabase por el pipeline.

### Crear el proyecto (una sola vez)

1. <https://supabase.com> → *Sign in with GitHub* → **New project**.
   - Nombre: `banco-chillo` · Región: la más cercana (ej. *East US*) · Contraseña: generada (guárdala en un gestor, **no** en el repo).
2. *Project Settings → Data API → Exposed schemas*: deja solo `public`. Nuestros esquemas `*_bc` **no** se exponen por la API REST de Supabase; solo los usan nuestros microservicios.
3. *Connect* (botón arriba) → **Session pooler** → copia:
   - **URI** (para Node.js y para el workflow): `postgresql://postgres.<ref>:<CLAVE>@aws-0-<region>.pooler.supabase.com:5432/postgres`
   - Formato .NET (Npgsql): `Host=aws-0-<region>.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<ref>;Password=<CLAVE>;SSL Mode=Require;Trust Server Certificate=true`
4. GitHub → *Settings → Secrets and variables → Actions* → **New repository secret**: `SUPABASE_DB_URL` = la URI.

### Aplicar los scripts en Supabase

GitHub → **Actions** → *Base de Datos — Aplicar scripts en Supabase* → **Run workflow** (rama `develop` o `main`).
Revisa en Supabase → *Table Editor* (selector de esquema) que aparezcan tus tablas.

### Row Level Security (RLS)

Supabase recomienda RLS en todas las tablas. Como nuestros esquemas no se exponen por la API pública y los
microservicios se conectan con un usuario propio, basta con **activar RLS sin políticas** (nadie entra por la API de Supabase):

```sql
ALTER TABLE auth_bc.usuarios ENABLE ROW LEVEL SECURITY;
```

El responsable de Auth documenta en su spec qué tablas tienen RLS y por qué.
