# ADR-0007: Una base PostgreSQL con un esquema por microservicio

- **Estado:** Aceptado
- **Fecha:** 2026-11-22
- **Autores:** Docente / Tech Lead
- **Decisores:** Tech Lead

---

## 1. Contexto y Planteamiento del Problema

El patrón "base de datos por microservicio" exige que cada servicio sea dueño de sus datos. Con el plan gratuito
de Supabase (2 proyectos) no es viable tener 7 bases separadas.

---

## 2. Opciones Consideradas

- **Opción 1:** una base por servicio (ideal, inviable en plan gratuito).
- **Opción 2:** una base compartida sin separación (rompe la independencia de los servicios).
- **Opción 3:** una base con **un esquema por servicio** y prohibición de consultas entre esquemas.

---

## 3. Decisión

Elegimos la **Opción 3**:

| Carpeta en `database/schema/` | Esquema | Dueño |
|---|---|---|
| `01-auth/` | `auth_bc` | Auth |
| `02-cuentas/` | `cuentas_bc` | Cuentas |
| `03-cajeros/` | `cajeros_bc` | Cajeros |
| `04-transacciones/` | `transacciones_bc` | Transacciones |
| `05-creditos/` | `creditos_bc` | Créditos |
| `06-auditoria/` | `auditoria_bc` | Auditoría |
| `07-notificaciones/` | `notificaciones_bc` | Notificaciones (Node.js) |

Reglas:

1. **Sin claves foráneas entre esquemas.** Las referencias a otro servicio se guardan como "referencia lógica"
   (ej. `cajero_id UUID -- referencia lógica a auth_bc.usuarios`) y se validan llamando a la API dueña.
2. **Scripts idempotentes:** `CREATE ... IF NOT EXISTS`, `INSERT ... ON CONFLICT DO NOTHING`. Se pueden ejecutar varias veces.
3. **Tablas contables y de transacciones append-only** (regla de oro 3 de AGENTS.md): sin `UPDATE` ni `DELETE`;
   se protegen con un trigger que lanza error, y las correcciones se hacen con un asiento de reverso.
4. **Montos en `NUMERIC(14,2)`**, nunca `FLOAT`/`REAL`.
5. **Fechas en `TIMESTAMP WITH TIME ZONE`.**
6. El SQL es la fuente de verdad; EF Core solo mapea (no se usan migraciones de EF en esta fase).

---

## 4. Consecuencias

### Positivas
- Un solo proyecto de Supabase y un solo contenedor local, con independencia lógica entre servicios.
- En el futuro, cada esquema puede moverse a su propia base sin reescribir el servicio.

### Negativas / Trade-offs
- No hay integridad referencial entre servicios; se compensa con validación vía API y eventos.
