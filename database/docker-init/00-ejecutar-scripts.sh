#!/bin/bash
# =====================================================================
# Inicialización de PostgreSQL en Docker (solo la PRIMERA vez que se crea el volumen).
# Postgres solo ejecuta archivos sueltos de /docker-entrypoint-initdb.d, no subcarpetas;
# por eso este script recorre database/schema/** y luego database/seeds/** en orden alfabético.
#
# Para volver a ejecutarlo: docker compose down -v && docker compose up -d postgres
# =====================================================================
set -euo pipefail

ejecutar() {
  echo ">> Ejecutando $1"
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -f "$1"
}

for archivo in $(find /banco-chillo/schema -name '*.sql' | sort); do ejecutar "$archivo"; done
for archivo in $(find /banco-chillo/seeds -name '*.sql' | sort); do ejecutar "$archivo"; done

echo ">> Base de datos de Banco Chillo inicializada."