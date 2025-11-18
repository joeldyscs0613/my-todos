# TodoService.Contracts

Public contracts for inter-service communication and client integration.

## What It Does

Defines DTOs and events that other services and clients use to interact with TodoService. No logic, just data structures.

## Structure

```
Contracts/
└── Todos/
    ├── TodoDto.cs                     # Todo data transfer object
    ├── TodoCreatedEvent.cs            # Integration event
    └── TodoCompletedEvent.cs          # Integration event
```

## Key Concepts

- **DTOs**: Data shapes for API responses (TodoDto with id, title, status, priority, dueDate)
- **Integration Events**: Published to RabbitMQ when todos are created or completed
- **No dependencies**: Contracts are standalone

## When to Use

- **React Client** calls TodoService API → uses TodoDto from Swagger/OpenAPI
- **NotificationService** listens to TodoCompletedEvent → sends notification

---

*Part of MyTodos TodoService - contracts layer for a microservices take-home assignment.*
