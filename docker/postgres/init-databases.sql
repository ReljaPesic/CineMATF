-- "cinemadb" is created automatically by the postgres image from the POSTGRES_DB
-- env var (see .env) - creating it again here would abort this whole script,
-- since docker's init-db runner stops on the first SQL error.
CREATE DATABASE "ReservationServiceDb";
CREATE DATABASE "ScreeningDB";
CREATE DATABASE "IdentityDB";
