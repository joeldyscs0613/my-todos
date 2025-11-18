# NotificationService.Domain

Domain layer for notification management and delivery rules.

## What It Does

Contains the core business entities and rules for managing notifications (email, SMS, push). Pure domain logic with no external dependencies.

## Structure

```
Domain/
└── NotificationAggregate/
    ├── Notification.cs            # Notification aggregate root
    ├── Constants/
    │   └── NotificationConstants.cs
    ├── Enums/
    │   ├── NotificationType.cs    # Email, SMS, Push
    │   └── NotificationStatus.cs  # Pending, Sent, Failed
    └── DomainEvents/
        └── NotificationSentDomainEvent.cs
```

## Key Features

- **Notification Aggregate**: Recipient, message, type, status, send date
- **Multi-channel**: Email, SMS, and push notifications
- **Static Factory**: `Notification.Create()` for creation
- **Retry Logic**: Failed notifications can be retried

---

*Part of MyTodos NotificationService - domain layer for a microservices take-home assignment.*
