# MyTodos.SharedKernel

## Overview

`MyTodos.SharedKernel` is a foundational library containing shared domain-driven design (DDD) building blocks, value objects, and helper types used across all MyTodos microservices.

This library follows Clean Architecture and DDD principles, providing:

- **Base abstractions** for domain modeling (Entity, AggregateRoot, ValueObject, DomainEvent)
- **Result pattern** for functional error handling (railway-oriented programming)
- **Error types** and error handling primitives
- **Helper utilities** for common operations

## Architecture Patterns

### Domain-Driven Design (DDD)

The SharedKernel implements core DDD building blocks:

- **Entity**: Objects with unique identity and lifecycle (e.g., Task, Project)
- **AggregateRoot**: Entities that serve as consistency boundaries and manage domain events
- **ValueObject**: Immutable objects compared by their values rather than identity (e.g., Address, Money)
- **DomainEvent**: Records of significant occurrences in the domain

### Result Pattern

Instead of throwing exceptions for control flow, we use the Result pattern:

```csharp
// Success case
Result<Task> result = Result.Success(task);

// Failure case
Result<Task> result = Result.NotFound("Task not found");

// Pattern matching
var response = result.Match(
    onSuccess: task => Ok(task),
    onFailure: errors => NotFound(errors)
);
```

## Project Structure

```
src/MyTodos.SharedKernel/
├── README.md               # This project's documentation
├── Abstractions/           # DDD base classes
│   ├── Entity.cs           # Base entity with Id and audit properties
│   ├── AggregateRoot.cs    # Base aggregate root with domain events
│   ├── ValueObject.cs      # Base for value objects
│   ├── DomainEvent.cs      # Base for domain events
├── Contracts/              # Interfaces and contracts
└── Helpers/                # Utility types
    ├── Result.cs           # Result pattern implementation
    ├── Error.cs            # Error types and factory methods
    └── DateTimeOffsetHelper.cs  # Testable UTC time access
```

## Usage Examples

### Creating an Entity

```csharp
public class TaskItem : Entity<Guid>
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TaskStatus Status { get; private set; }

    public TaskItem(Guid id, string title, string description) : base(id)
    {
        Title = title;
        Description = description;
        Status = TaskStatus.NotStarted;
    }

    public void MarkComplete()
    {
        Status = TaskStatus.Completed;
        // Audit trail automatically maintained via Entity base class
    }
}
```

### Creating an Aggregate Root

```csharp
public class Project : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    private readonly List<TaskItem> _tasks = new();

    public Project(Guid id, string name) : base(id)
    {
        Name = name;
        AddDomainEvent(new ProjectCreatedEvent(id.ToString(), name));
    }

    public void AddTask(TaskItem task)
    {
        _tasks.Add(task);
        AddDomainEvent(new TaskAddedToProjectEvent(Id.ToString(), task.Id.ToString()));
    }
}
```

### Creating a Value Object

```csharp
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string ZipCode { get; }

    public Address(string street, string city, string zipCode)
    {
        Street = street;
        City = city;
        ZipCode = zipCode;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return ZipCode;
    }
}

// Usage
var address1 = new Address("123 Main St", "Springfield", "12345");
var address2 = new Address("123 Main St", "Springfield", "12345");
Console.WriteLine(address1 == address2); // True (value equality)
```

### Using the Result Pattern

```csharp
public class TaskService
{
    public Result<TaskItem> GetTask(Guid id)
    {
        var task = _repository.Find(id);

        if (task is null)
            return Result.NotFound<TaskItem>($"Task with id {id} not found");

        return Result.Success(task);
    }

    public Result CreateTask(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.BadRequest("Title is required");

        if (string.IsNullOrWhiteSpace(description))
            return Result.BadRequest("Description is required");

        var task = new TaskItem(Guid.NewGuid(), title, description);
        _repository.Add(task);

        return Result.Success();
    }
}
```

## Key Patterns and Conventions

### Parameterless Constructor Protection

All base classes (Entity, AggregateRoot, ValueObject, DomainEvent) include a protected parameterless constructor required for EF Core and serializers, but it's protected from developer misuse using:

- `[Obsolete(error: true)]` - Causes compile error if used
- `[EditorBrowsable(Never)]` - Hides from IntelliSense
- XML documentation explaining it's for infrastructure only

See [Abstractions/README.md](Abstractions/README.md) for detailed documentation.

### Audit Trail

All entities automatically track:

- `CreatedBy` / `CreatedDate` - Set via `SetCreatedInfo(username)`
- `ModifiedBy` / `ModifiedDate` - Set via `SetUpdatedInfo(username)`

### Domain Events

Aggregate roots can raise domain events to communicate changes to other parts of the system:

```csharp
public class Task : AggregateRoot<Guid>
{
    public void Complete()
    {
        Status = TaskStatus.Completed;
        AddDomainEvent(new TaskCompletedEvent(Id.ToString()));
    }
}

// Events are collected in the DomainEvents property
// and can be dispatched after saving changes
foreach (var domainEvent in task.DomainEvents)
{
    await _eventDispatcher.Dispatch(domainEvent);
}
task.ClearDomainEvents();
```

### Error Handling

The Result pattern provides type-safe error handling without exceptions:

```csharp
// Single error
Result.NotFound("Resource not found")
Result.Unauthorized("Authentication required")
Result.BadRequest("Invalid input")

// Multiple errors (validation scenarios)
var errors = new List<Error>
{
    Error.BadRequest("Title is required"),
    Error.BadRequest("Description is too long")
};
Result.BadRequest(errors)
```

## Testing

The SharedKernel has comprehensive unit test coverage (137 tests):

**EntityTests** - 17 tests covering identity, audit trail, and protection patterns
```bash
 dotnet test tests/MyTodos.SharedKernel.UnitTests --filter EntityTests
```

**AggregateRootTests** - 17 tests for domain event management
```bash
 dotnet test tests/MyTodos.SharedKernel.UnitTests --filter AggregateRootTests
```

**ValueObjectTests** - 28 tests for equality, hash codes, and operators
```bash
 dotnet test tests/MyTodos.SharedKernel.UnitTests --filter ValueObjectTests
```

**DomainEventTests** - 21 tests for event creation and properties
```bash
 dotnet test tests/MyTodos.SharedKernel.UnitTests --filter DomainEventTests
```

**ResultTests** - 37 tests for all success/failure scenarios
```bash
 dotnet test tests/MyTodos.SharedKernel.UnitTests --filter ResultTests
```

**ErrorTests** - 18 tests for error types and factory methods
```bash
 dotnet test tests/MyTodos.SharedKernel.UnitTests --filter ErrorTests
```

**DateTimeOffsetHelperTests** - 2 tests for UTC time handling
```bash
 dotnet test tests/MyTodos.SharedKernel.UnitTests --filter DateTimeOffsetHelperTests
```

Run all tests:
```bash
 dotnet test tests/MyTodos.SharedKernel.UnitTests
```

## Dependencies

- .NET 9.0
- No external dependencies (intentionally kept lightweight and portable to follow DDD principles)

## Distribution

In a real enterprise application, this SharedKernel would be:

- Versioned using semantic versioning (e.g., 1.0.0, 1.1.0)
- Published as a NuGet package to a private package repository (e.g., Azure DevOps Artifacts, GitHub Packages)
- Referenced by all microservices in the solution to ensure consistency
- Updated independently with proper versioning to avoid breaking changes

This approach ensures that domain modeling patterns and business rules remain consistent across all services in the enterprise ecosystem.

## References

### Domain-Driven Design (DDD)

DDD is the core architectural foundation of this library.

- Evans, Eric. *Domain-Driven Design: Tackling Complexity in the Heart of Software*
- Vernon, Vaughn. *Implementing Domain-Driven Design*

### Result Pattern
- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/) by Scott Wlaschin
- [Result Pattern in C#](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/)

### Architecture
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html) by Martin Fowler
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) by Robert C. Martin
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html) by Martin Fowler
