#!/bin/sh
# Keycloak keeps its own schema; give it a separate database on the same server.
set -eu

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<SQL
    CREATE DATABASE ${KEYCLOAK_DB};
    GRANT ALL PRIVILEGES ON DATABASE ${KEYCLOAK_DB} TO ${POSTGRES_USER};
SQL
