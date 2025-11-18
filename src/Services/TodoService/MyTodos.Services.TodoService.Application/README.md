# TodoService.Application

Application layer with CQRS commands and queries for todo operations.

## What It Does

Implements use cases for creating, updating, completing, and querying todo items using MediatR. Business workflows orchestrated here.

## Structure

```
Application/
└── Todos/
    ├── Commands/
    │   ├── CreateTodo/            # CreateTodoCommand + Handler + Validator
    │   ├── UpdateTodo/            # UpdateTodoCommand + Handler + Validator
    │   ├── CompleteTodo/          # CompleteTodoCommand + Handler
    │   └── DeleteTodo/            # DeleteTodoCommand + Handler
    ├── Queries/
    │   ├── GetTodoDetails/        # GetTodoDetailsQuery + Handler + Dto
    │   └── GetPagedList/          # GetTodosQuery + Handler + Filter + Spec
    └── Contracts/
        ├── ITodoReadRepository.cs
        └── ITodoWriteRepository.cs
```

## Key Features

- **CQRS**: Commands modify state, queries fetch data
- **MediatR**: Request/response pipeline
- **FluentValidation**: Title required, max lengths enforced
- **Multi-tenant Filtering**: Users only see their tenant's todos

---

*Part of MyTodos TodoService - application layer for a microservices take-home assignment.*
