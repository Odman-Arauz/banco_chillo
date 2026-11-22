import pg from 'pg';

// Pool de conexiones a PostgreSQL (local en Docker o Supabase).
// Se crea de forma perezosa para que las pruebas unitarias no necesiten base de datos.
let pool;

export function obtenerPool(databaseUrl) {
  if (!pool) {
    pool = new pg.Pool({
      connectionString: databaseUrl,
      // Supabase exige SSL; en local (Docker) no.
      ssl: databaseUrl.includes('supabase') ? { rejectUnauthorized: false } : false,
      max: 5,
    });
  }
  return pool;
}
