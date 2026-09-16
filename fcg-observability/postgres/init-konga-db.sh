#!/bin/sh
# O Konga usa um banco proprio, separado do banco do Kong.
# Sem isto o container do Konga sobe e morre com erro do Postgres.
set -e
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    CREATE DATABASE konga;
    GRANT ALL PRIVILEGES ON DATABASE konga TO $POSTGRES_USER;
EOSQL
