# TodoService.Domain

Domain layer for todo item management and business rules.

## What It Does

Contains the core business entities and logic for creating, updating, and managing todo items. Pure domain logic with no external dependencies.

## Structure

```
Domain/
└── TodoAggregate/
    ├── Todo.cs                    # Todo aggregate root
    ├── Constants/
    │   └── TodoConstants.cs       # Max lengths, defaults
    ├── Enums/
    │   ├── TodoStatus.cs          # NotStarted, InProgress, Completed
    │   └── TodoPriority.cs        # Low, Medium, High, Urgent
    └── DomainEvents/
        ├── TodoCreatedDomainEvent.cs
        └── TodoCompletedDomainEvent.cs
```

## Key Features

- **Todo Aggregate**: Title, description, status, priority, due date
- **Multi-tenant**: Each todo belongs to a tenant
- **Static Factory**: `Todo.Create()` for creation
- **Business Rules**: Can't complete without starting, overdue detection

---

*Part of MyTodos TodoService - domain layer for a microservices take-home assignment.*
