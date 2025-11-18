# NotificationService.Application

Application layer with CQRS commands and queries for notification operations.

## What It Does

Implements use cases for sending notifications and listening to integration events from other services using MediatR.

## Structure

```
Application/
├── Notifications/
│   ├── Commands/
│   │   ├── SendEmail/             # SendEmailCommand + Handler + Validator
│   │   ├── SendSms/               # SendSmsCommand + Handler + Validator
│   │   └── SendPush/              # SendPushCommand + Handler + Validator
│   ├── Queries/
│   │   └── GetNotificationDetails/
│   └── Contracts/
│       ├── INotificationReadRepository.cs
│       └── INotificationWriteRepository.cs
└── IntegrationEventHandlers/
    ├── UserCreatedEventHandler.cs     # Sends welcome email
    └── TodoCompletedEventHandler.cs   # Sends completion notification
```

## Key Features

- **CQRS**: Commands send notifications, queries fetch history
- **MediatR**: Request/response pipeline
- **Event-Driven**: Listens to RabbitMQ events from other services
- **FluentValidation**: Email/phone validation

---

*Part of MyTodos NotificationService - application layer for a microservices take-home assignment.*
