# BuildingBlocks.Application

Application layer contracts, CQRS infrastructure, and cross-cutting concerns.

## Structure

```
Application/
├── Contracts/          # Repository and service interfaces
├── Abstractions/       # Base classes for CQRS (Command, Query, Specification, Filter)
├── Behaviors/          # MediatR pipeline behaviors (Validation, UnitOfWork, Logging)
├── Validators/         # FluentValidation validators for queries and filters
├── Helpers/            # PagedList, OrderBy utilities
├── Constants/          # Paging and display constants
└── Exceptions/         # Application-specific exceptions
```

## Core Concepts

**CQRS Pattern**
- `ICommand<TResponse>` / `IQuery<TResponse>` marker interfaces
- Handlers use MediatR for request/response pipeline
- Commands modify state, queries read state

**Repository Contracts**
- `IReadRepository<T>` - No-tracking queries
- `IWriteRepository<T>` - CUD operations
- `IPagedListReadRepository<T>` - Paginated queries with filtering/sorting

**Pipeline Behaviors**
1. Validation → FluentValidation checks
2. UnitOfWork → Transaction boundary for commands
3. Logging → Request/response logging

**Specification Pattern**
- Encapsulates query logic (Where, OrderBy, Include)
- Reusable across queries
- Composable with `And()` / `Or()`

## Command Flow

```
Request → ValidationBehavior → UnitOfWorkBehavior → Handler → Response
                ↓                      ↓
          Throws if invalid    SaveChanges + DispatchEvents
```

## Query Flow

```
Request → ValidationBehavior → Handler → Response
                ↓
          Throws if invalid
```

## Design Principles

- **Interface Segregation:** Separate Read/Write repositories
- **Dependency Inversion:** Application defines contracts, Infrastructure implements
- **Single Responsibility:** Each behavior handles one cross-cutting concern

*This is a take-home assignment, so I kept all as simple as possible.*
