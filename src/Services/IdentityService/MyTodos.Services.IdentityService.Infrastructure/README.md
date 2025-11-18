# IdentityService.Infrastructure

Infrastructure layer for data persistence, security, and external integrations.

## What It Does

Implements EF Core repositories, DbContext, JWT token generation, password hashing, and database seeding.

## Structure

```
Infrastructure/
├── Persistence/
│   ├── Users/
│   │   ├── Configs/
│   │   └── Repositories/
│   ├── Tenants/
│   │   ├── Config/
│   │   └── Repositories/
│   ├── Roles/
│   │   ├── Configs/
│   │   └── Repositories/
│   ├── Permissions/
│   │   ├── Configs/
│   │   └── Repositories/
│   ├── Constants/
│   ├── Migrations/
│   ├── IdentityServiceDbContext.cs
│   ├── IdentityServiceDbContextFactory.cs
│   └── IdentityServiceUnitOfWork.cs
├── Security/
│   ├── JwtTokenService.cs
│   ├── PasswordHashingService.cs
│   ├── CurrentUserService.cs
│   ├── TenantService.cs
│   ├── SecurityService.cs
│   └── JwtSettings.cs
└── Seeding/
    └── DatabaseSeederService.cs
```

## Key Features

- **Vertical Slicing**: Each aggregate has its own Configs + Repositories folders
- **EF Core**: SQLite with migrations
- **Security**: BCrypt password hashing, JWT tokens
- **Data Seeding**: Default admin, roles, and permissions

---

*Part of MyTodos IdentityService - infrastructure layer for a microservices take-home assignment.*
