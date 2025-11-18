# TodoService.Infrastructure

Infrastructure layer for data persistence and technical implementations.

## What It Does

Implements EF Core repositories, DbContext, and database migrations for todo items. Handles data access and persistence concerns.

## Structure

```
Infrastructure/
├── TodoAggregate/
│   ├── Persistence/
│   │   └── TodoConfig.cs          # EF Core configuration
│   └── Repositories/
│       ├── TodoReadRepository.cs
│       └── TodoWriteRepository.cs
└── Persistence/
    ├── TodoServiceDbContext.cs
    ├── TodoServiceUnitOfWork.cs
    └── Migrations/                # EF Core migrations
```

## Key Features

- **EF Core**: SQLite database with Todo aggregate
- **Read/Write Separation**: Separate repos for queries and commands
- **Multi-tenant Filtering**: Automatic TenantId filtering in queries
- **Migrations**: Database schema versioning

---

*Part of MyTodos TodoService - infrastructure layer for a microservices take-home assignment.*
