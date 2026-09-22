# CineMATF

CineMATF is a cinema ticketing platform built as a set of independent .NET microservices with an
Angular single-page front end. Users can browse movies and screenings, pick seats on an interactive
hall map, book and pay for tickets, and receive a QR-coded PDF e-ticket by email.

This is the seminar/term project for the Software Development 2 (RS2) course.

![Movies listing](Pictures/Screenshot%202026-09-05%20at%2013.50.12.png)
![Seat selection](Pictures/Screenshot%202026-09-05%20at%2013.50.55.png)
![Reservation details](Pictures/Screenshot%202026-09-05%20at%2013.51.14.png)
![E-ticket PDF with QR code](Pictures/Screenshot%202026-09-05%20at%2013.51.25.png)
![Stripe Checkout](Pictures/Screenshot%202026-09-22%20at%2022.44.46.png)
![Payment confirmation](Pictures/Screenshot%202026-09-22%20at%2022.45.46.png)

## Architecture

The system follows a database-per-service microservices architecture. Services never read each
other's databases directly — they talk to each other over gRPC or HTTP (via typed clients in
`ExternalServices`), and a screening is resolved as a plain `Guid` reference rather than a
foreign key across service boundaries.

| Service           | Responsibility                                             | Storage                     | Port(s) (docker-compose) |
|-------------------|-------------------------------------------------------------|------------------------------|---------------------------|
| `Identity.API`    | User registration/login, JWT + refresh tokens, roles        | Postgres (`IdentityDB`)      | 8005                      |
| `Cinema.API`      | Cinemas, halls, seats; exposes a gRPC endpoint used internally | Postgres (`cinemadb`)      | 8000 (HTTP), 8006 (gRPC)  |
| `Movie.API`       | Movie catalog (title, genres, cast, rating); exposes a gRPC endpoint used internally | MongoDB (`MovieDB`) | 8001 (HTTP), 8007 (gRPC) |
| `Screening.API`   | Screenings (movie + hall + time + format); exposes a gRPC endpoint used internally | Postgres (`ScreeningDB`) | 8003 (HTTP), 8004 (gRPC) |
| `Reservation.API` | Seat locking, reservations, tickets, PDF/QR ticket generation, ticket emails | Postgres (`ReservationServiceDb`) | 8002 |
| `WebApp`          | Angular SPA consuming all of the above                       | —                              | 4200                      |

See [`docs/database-schema.md`](docs/database-schema.md) for the full entity-relationship diagram
and per-service data ownership.

Cross-cutting concerns:
- **Security** — Identity.API issues JWT access tokens (plus refresh tokens); every other service
  validates the same JWT via `AddJwtBearer`/`AddAuthorization`, with role-based checks (e.g. `Admin`)
  on management endpoints.
- **Inter-service communication** — Reservation.API calls Identity.API over HTTP through a typed
  client, and calls Cinema.API, Movie.API and Screening.API over **gRPC** (via
  `Services/Cinema.API/Protos/cinema.proto`, `Services/Movie.API/Protos/movie.proto` and
  `Services/Screening.API/Protos/screening.proto`) to resolve seat, movie and screening details.
- **Payments** — `Reservation.API` takes payment via **Stripe Checkout** (card only). `POST
  /api/v1/reservations/{id}/checkout-session` extends the seat-lock hold and creates a hosted
  Stripe Checkout session; the browser redirects there to pay. Confirmation is asynchronous: a
  `POST /api/v1/payments/webhook` endpoint handles Stripe's `checkout.session.completed` event and
  idempotently promotes the reservation from `Locked` to `Confirmed` — or refunds the payment if
  the hold had already expired by the time it arrives. The old `POST
  /api/v1/reservations/{id}/pay` endpoint still exists but is now restricted to
  `Development`/`Testing` environments for test setup only.
- **API docs** — every service exposes Swagger/OpenAPI (`/swagger`) in development.

## Tech stack

- **Backend**: ASP.NET Core (.NET 10), Entity Framework Core, MongoDB driver, gRPC
- **Frontend**: Angular 18 (standalone SPA)
- **Databases**: PostgreSQL 16 (one instance, one database per service) and MongoDB 7
- **Containerization**: Docker & Docker Compose
- **Testing**: xUnit — unit tests per service plus integration tests (WebApplicationFactory-based) for the API layer

## Running with Docker (recommended)

Prerequisites: Docker and Docker Compose.

1. Copy the environment template and fill in the values:
   ```bash
   cp .env.example .env
   ```
   - `POSTGRES_USER` / `POSTGRES_PASSWORD` — credentials for the shared Postgres container.
   - `EMAIL_SENDER` / `EMAIL_APP_PASSWORD` — a Gmail address with an
     [app password](https://myaccount.google.com/apppasswords) (2‑Step Verification must be enabled),
     used by Reservation.API to email ticket confirmations. This is optional for local testing — the
     rest of the app works without it — but ticket emails won't be sent if left blank.
2. Start everything:
   ```bash
   docker compose up --build
   ```
   This brings up Postgres, pgAdmin, MongoDB, and all five services + the web app, wired together on
   a shared `cinema-network`. Postgres schemas are created automatically from
   `docker/postgres/init-databases.sql` on first boot.
3. Open the app:
   - Web app: http://localhost:4200
   - Swagger UIs: http://localhost:8000/swagger (Cinema), http://localhost:8001/swagger (Movie),
     http://localhost:8002/swagger (Reservation), http://localhost:8003/swagger (Screening),
     http://localhost:8005/swagger (Identity)
   - pgAdmin: http://localhost:5050 (login with the credentials in `docker-compose.yml`)

`docker-compose.override.yml` is applied automatically alongside `docker-compose.yml` and mounts the
Angular source into the `webapp` container with polling enabled, so front-end changes hot-reload
without rebuilding the image.

## Running locally without Docker

Each service can also be run directly with the .NET SDK, e.g.:

```bash
cd Services/Cinema.API
dotnet run
```

You'll need Postgres and MongoDB reachable locally (or point at the Dockerized databases — Postgres
is published on host port `5433`, see the connection strings in `.env.example`) and each service's
`appsettings.Development.json` configured accordingly. Migrations are managed with `dotnet-ef`
(pinned in `dotnet-tools.json`); restore it with:

```bash
dotnet tool restore
```

Services that expose gRPC (`Cinema.API`, `Movie.API`, `Screening.API`) listen on two ports when run
this way: `8080` for HTTP/REST (Swagger) and `8081` for gRPC (HTTP/2). If you're running
`Reservation.API` locally too, point its `CinemaApi:BaseUrl`, `MovieApi:BaseUrl` and
`ScreeningApi:BaseUrl` settings at each service's `8081` gRPC port, not the Swagger port.

For the front end:

```bash
cd WebApp
npm install
npm start
```

Navigate to http://localhost:4200.

## Tests

Run the full backend test suite from the repo root:

```bash
dotnet test
```

Each service has its own unit test project (business/service-layer logic) plus integration tests
that spin up the API in-memory (via `WebApplicationFactory`) against a real test database, covering
controllers end-to-end, including authenticated requests (see `TestJwt.cs`).

Front-end unit tests (Karma/Jasmine):

```bash
cd WebApp
npm test
```

## Repository layout

```
Services/
  Cinema.API/          # cinemas, halls, seats, gRPC endpoint
  Movie.API/           # movie catalog (MongoDB), gRPC endpoint
  Screening.API/       # screenings, gRPC endpoint
  Reservation.API/     # reservations, tickets, PDF/QR generation, emails
  Identity.API/        # auth, users, roles
Tests/                 # unit + integration tests, one project per service
WebApp/                # Angular SPA
docs/                  # architecture docs (database schema, ER diagram)
docker/                # Postgres init scripts
docker-compose.yml
docker-compose.override.yml
```
