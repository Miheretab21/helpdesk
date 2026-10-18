# IT Help Desk

A full-stack IT help desk / support ticketing system. Employees raise tickets, agents work them through their lifecycle, and admins manage users and oversee the whole queue. The solution is split into a **.NET Web API** backend (Clean Architecture, CQRS) and an **Angular** single-page frontend.

---

## Table of contents

- [Features](#features)
- [Tech stack](#tech-stack)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Getting started](#getting-started)
- [Demo credentials](#demo-credentials)
- [Seeded data](#seeded-data)
- [API endpoints](#api-endpoints)
- [Roles and permissions](#roles-and-permissions)
- [Testing](#testing)
- [Ticket lifecycle](#ticket-lifecycle)
- [Project structure](#project-structure)

---

## Features

- **JWT authentication** with role-based access (Employee / Agent / Admin) and an Angular route guard + HTTP interceptor.
- **Ticket management** — create, view, edit, cancel, reopen, escalate, change status, assign to an agent, and bulk update.
- **Ticket lifecycle** — `New → Assigned → In Progress → Resolved → Closed` (plus `Cancelled`), with SLA due dates and escalation tracking.
- **Comments** on tickets.
- **File attachments** — upload and download images/documents, with authenticated downloads (fetched as blobs so the bearer token is sent) and an inline thumbnail + lightbox viewer.
- **Audit history** per ticket (who did what, when), shown on the ticket detail page.
- **Role-aware dashboards** with metric cards and filterable ticket queues (search, status, priority, assignee).
- **User management** (admin only) — create, edit, and delete users.


---

## Tech stack

### Backend

| Area | Technology |
| --- | --- |
| Runtime | .NET 10 (`net10.0`) |
| Web framework | ASP.NET Core Web API |
| Data access | Entity Framework Core 10 + Npgsql provider |
| Database | PostgreSQL |
| Auth | JWT Bearer (HMAC-SHA256) |
| Password hashing | Bcrypt |
| API docs | Microsoft.AspNetCore.OpenApi |
| Architecture | Clean Architecture + CQRS (command/query handlers) |

### Frontend

| Area | Technology |
| --- | --- |
| Framework | Angular 22 (standalone components) |
| Language | TypeScript ~6.0 |
| Reactive state | Angular signals (`signal`, `computed`, `httpResource`) |
| Styling | SCSS with semantic design tokens |
| Unit tests | Vitest |
| Formatting | Prettier |

---

## Architecture

The backend follows Clean Architecture with dependencies pointing inward:

```
HelpDesk.Domain          → Entities, enums, common types, domain exceptions (no dependencies)
HelpDesk.Application     → CQRS commands/queries + handlers, DTOs, repository interfaces
HelpDesk.Infrastructure  → EF Core DbContext, migrations, repositories, security, seeder
HelpDesk.WebAPI          → Controllers, middleware, DI composition root, Program.cs
```

- **Domain** holds `Ticket`, `User`, `Category`, `Comment`, `TicketAttachment`, `TicketAuditEntry` and the `TicketStatus`, `TicketPriority`, `UserRole` enums.
- **Application** defines one folder per feature (Auth, Tickets, Comments, Categories, Dashboard, Users) with a `Commands/` or `Queries/` folder containing a request + handler pair, and a `Dtos/` folder. Repository contracts live in `Abstractions/`.
- **Infrastructure** implements those contracts with EF Core repositories and owns the PostgreSQL schema (migrations) and the development `Seeder`.
- **WebAPI** exposes REST controllers, wires everything up in `Program.cs`, and applies `DomainExceptionMiddleware` to translate domain exceptions into proper HTTP responses.

The Angular client is organized into `core/` (models, services, guards, interceptors), `features/` (auth, dashboard, tickets, admin, layout), and `shared/` (reusable UI components).

---

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (npm 11+; the client pins `npm@11.8.0`)
- [PostgreSQL](https://www.postgresql.org/download/) running on `localhost:5432`
- (Optional) [Angular CLI](https://angular.dev/tools/cli) for `ng` commands — otherwise use the npm scripts below.

---

## Configuration

### Database

The connection string is read from `ConnectionStrings:HelpDeskDb` in
`src/HelpDesk.WebAPI/appsettings.json`:

```
Host=localhost;Port=5432;Database=helpdesk;Username=helpdesk;Password=#
```

Create the database and role (or adjust the connection string to match your setup). Migrations are applied automatically on startup in the Development environment, and demo data is seeded if the database is empty.

### JWT

JWT settings live under the `Jwt` section of `src/HelpDesk.WebAPI/appsettings.json`
(`Secret`, `Issuer` = `HelpDesk`, `Audience` = `HelpDeskClient`, `ExpiryMinutes` = `60`).
Provide a strong, secret value for `Secret` in any non-development environment.

### Frontend API proxy

During development the Angular dev server proxies `/api` to the backend
(`client/src/proxy.conf.json` → `http://localhost:5029`), so no CORS setup is needed.

---

## Getting started

### 1. Start the backend

From the repository root:

```bash
dotnet run --project src/HelpDesk.WebAPI
```

The API listens on **http://localhost:5029** (HTTP profile). On first run it applies migrations and seeds demo data.

### 2. Start the frontend

In a second terminal:

```bash
cd client
npm install
npm start
```

The app is served at **http://localhost:4200** and automatically reloads on changes. All `/api` calls are proxied to the backend on port 5029.

Open http://localhost:4200, sign in with one of the demo accounts below.

> Note: if you change backend handler logic, **restart the API** — a running instance may serve stale results.

---

## Demo credentials

Seeded on first run (password `password123` for all):

| Email | Name | Role |
| --- | --- | --- |
| `alice@example.com` | Alice Employee | Employee |
| `bob@example.com` | Bob Agent | Agent |
| `carol@example.com` | Carol Admin | Admin |

The login page also provides quick-fill chips for these accounts.

---

## Seeded data

- **Categories:** Hardware, Software, Network
- **7 tickets** across the lifecycle (New / Assigned / In Progress / Resolved / Closed / Cancelled) owned by Alice, mostly assigned to Bob — enough to exercise dashboards, filters, and history.

---

## API endpoints

All routes are prefixed with `/api`. Authenticated routes require a `Bearer` token returned by login.

### Auth — `/api/auth`

| Method | Route | Description |
| --- | --- | --- |
| POST | `/api/auth/login` | Authenticate and receive a JWT. |
| GET | `/api/auth/me` | Current user profile. |

### Tickets — `/api/tickets`

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/tickets` | List tickets (supports `search`, `status`, `priority`, `assignedToId`, `unassignedOnly`). |
| GET | `/api/tickets/{id}` | Ticket detail. |
| POST | `/api/tickets` | Create a ticket. |
| PUT | `/api/tickets/{id}` | Edit a ticket. |
| POST | `/api/tickets/{id}/assign` | Assign to a user. |
| POST | `/api/tickets/{id}/status` | Change status. |
| POST | `/api/tickets/{id}/cancel` | Cancel the ticket. |
| POST | `/api/tickets/{id}/reopen` | Reopen a closed/cancelled ticket. |
| POST | `/api/tickets/{id}/escalate` | Escalate the ticket. |
| POST | `/api/tickets/bulk` | Bulk action on multiple tickets. |
| GET | `/api/tickets/{id}/history` | Audit history. |
| GET | `/api/tickets/{id}/attachments` | List attachments. |
| POST | `/api/tickets/{id}/attachments` | Upload an attachment. |
| GET | `/api/tickets/{id}/attachments/{attachmentId}/download` | Download an attachment (requires auth). |

### Comments — `/api/tickets/{ticketId}/comments`

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/tickets/{ticketId}/comments` | List comments. |
| POST | `/api/tickets/{ticketId}/comments` | Add a comment. |

### Categories — `/api/categories`

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/categories` | List categories. |

### Dashboard — `/api/dashboard`

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/dashboard/metrics` | Shared metric counts. |
| GET | `/api/dashboard/my-dashboard` | Current user's dashboard. |
| GET | `/api/dashboard/employee` | Employee dashboard. |
| GET | `/api/dashboard/agent` | Agent dashboard. |
| GET | `/api/dashboard/admin` | Admin dashboard. |

### Users — `/api/users` (Admin only)

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/users` | List users. |
| GET | `/api/users/{id}` | User detail. |
| POST | `/api/users` | Create user. |
| PUT | `/api/users/{id}` | Update user. |
| DELETE | `/api/users/{id}` | Delete user. |

---

## Roles and permissions

| Role | Ticket visibility | Capabilities |
| --- | --- | --- |
| **Employee** | Tickets they created | Create/edit own tickets, comment, attach files. |
| **Agent** | Unassigned tickets + tickets assigned to them | Work the queue, assign, change status, comment. |
| **Admin** | All tickets | Everything, plus user management. |

The sidebar is role-aware: Employees see **My Tickets** first, Agents see **My Tickets**, Admins see **Assigned Tickets** and user management.

---

## Testing

### Backend (xUnit)

From the repository root:

```bash
dotnet test
```

- `tests/HelpDesk.Domain.Tests` — domain entity/behavior tests.
- `tests/HelpDesk.Application.Tests` — command/query handler tests (uses a `FakeTicketRepository`).
- `tests/HelpDesk.Infrastructure.Tests` — EF Core repository tests (uses a `DatabaseFixture`).

### Frontend (Vitest)

```bash
cd client
npm test
```

---

## Ticket lifecycle

```
New ──assign──► Assigned ──► In Progress ──► Resolved ──► Closed
 │                                                          
 └──────────────────── Cancelled ◄──────────────────────────
```

- **New** — created, unassigned (surfaces in the "Unassigned" queue).
- **Assigned** — has an owner.
- **In Progress** — being worked on.
- **Resolved / Closed** — done.
- **Cancelled** — abandoned; a closed/cancelled ticket can be reopened.

Each transition is recorded in the ticket's audit history along with the actor and timestamp.

---

## Project structure

```
HelpDesk/
├── HelpDesk.slnx                 # Solution
├── client/                       # Angular SPA
│   ├── src/app/
│   │   ├── core/                 # models, services, guards, interceptors
│   │   ├── features/             # auth, dashboard, tickets, admin, layout, errors
│   │   └── shared/               # reusable components
│   ├── src/proxy.conf.json       # /api → http://localhost:5029
│   └── package.json
├── src/
│   ├── HelpDesk.Domain/          # entities, enums, common
│   ├── HelpDesk.Application/     # CQRS handlers, DTOs, abstractions
│   ├── HelpDesk.Infrastructure/  # EF Core, migrations, repositories, security, seeder
│   └── HelpDesk.WebAPI/          # controllers, middleware, Program.cs
└── tests/                        # Domain / Application / Infrastructure tests
```
