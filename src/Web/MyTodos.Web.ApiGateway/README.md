# MyTodos API Gateway

YARP-based reverse proxy that routes requests to microservices.

## What It Does

Routes client requests to the right service (Identity, Todo, Notification) and handles auth, CORS, and health checks.

## Architecture

```
Client (React) → API Gateway (8080) → Services
                      ├── /api/identity/** → IdentityService (5001)
                      ├── /api/todos/** → TodoService (5011)
                      └── /api/notifications/** → NotificationService (5250)
```

## Getting Started

```bash
cd src/Web/ApiGateway/MyTodos.Web.ApiGateway
dotnet run
```

Gateway runs on:
- HTTP: http://localhost:8080
- HTTPS: https://localhost:8443

## Routes

| Path | Service | Auth |
|------|---------|------|
| `/api/identity/auth/**` | IdentityService | Public |
| `/api/identity/users/**` | IdentityService | Protected |
| `/api/todos/**` | TodoService | Protected |
| `/api/notifications/**` | NotificationService | Protected |
| `/health` | Health check | Public |
| `/swagger` | API docs (dev only) | Public |

## CORS

Configured for React dev server:
- `http://localhost:3000`
- `http://localhost:5173`

Edit `appsettings.json` to add more origins.

## Authentication

Login flow: Client → Gateway → IdentityService → JWT token → Client stores it → Gateway validates it on protected routes.

JWT settings must match IdentityService (see `appsettings.json`).

## Configuration

All routing config is in `appsettings.json` under the `ReverseProxy` section.

Health checks ping services every 30 seconds at their `/health` endpoints.

---

*Part of a take-home assignment showcasing YARP, JWT auth, and microservices routing.*
