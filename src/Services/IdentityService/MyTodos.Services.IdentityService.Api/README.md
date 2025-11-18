# IdentityService.Api

REST API for user authentication, tenant (organization) management, and authorization.

## What It Does

ASP.NET Core Web API that exposes HTTP endpoints for signing in, managing users, tenants, roles, and permissions. Runs on port 5001.

## Structure

```
Api/
├── Controllers/
│   ├── AuthenticationController.cs    # POST /api/auth/sign-in, sign-out
│   ├── UsersController.cs             # CRUD users, invitations
│   ├── TenantsController.cs           # CRUD tenants
│   ├── RolesController.cs             # CRUD roles
│   └── PermissionsController.cs       # GET permissions
└── Program.cs                         # API setup, JWT, Swagger
```

## Endpoints

| Path | Method | Auth | Description |
|------|--------|------|-------------|
| `/api/auth/sign-in` | POST | Public | Login with username/password |
| `/api/users` | GET | Protected | Get paginated user list |
| `/api/users/{id}` | GET | Protected | Get user details |
| `/api/users/invite` | POST | Protected | Invite new user |
| `/api/tenants` | POST | Protected | Create tenant |
| `/health` | GET | Public | Health check |

## Getting Started

```bash
cd src/Services/IdentityService/MyTodos.Services.IdentityService.Api
dotnet run
```

API runs on http://localhost:5001 and https://localhost:5002.

Swagger docs: http://localhost:5001/swagger

---

*Part of MyTodos IdentityService - API layer for a microservices take-home assignment.*
