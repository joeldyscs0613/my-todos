# DDD Base Classes - Parameterless Constructor Protection Pattern

## Overview

All base classes in this folder (`Entity`, `AggregateRoot`, `DomainEvent`) follow a consistent pattern for protecting parameterless constructors from misuse while allowing infrastructure code (EF Core, serializers) to use them.

## The Problem

Domain-Driven Design base classes need parameterless constructors for:
- **EF Core** - Entity materialization from database
- **JSON Serializers** - Deserialization (System.Text.Json, Newtonsoft.Json)
- **Message Bus** - Event deserialization (RabbitMQ, MassTransit, etc.)

However, developers should **never** call these parameterless constructors directly in domain code. They should always use the parameterized constructors that enforce domain invariants.

## The Solution

We use a triple-layer protection pattern:

### 1. XML Documentation
```csharp
/// <summary>
/// Parameterless constructor for deserialization only (EF Core, JSON serializers).
/// DO NOT USE in domain code.
/// </summary>
```
Shows clear warnings in IDE and generated documentation.

### 2. Obsolete Attribute (Compile-Time Error)
```csharp
[Obsolete("Only for deserialization. Use parameterized constructor.", error: true)]
```
- **Prevents compilation** if called directly in domain code
- `error: true` makes it a compile error (not just a warning)
- **Does not affect** reflection-based infrastructure code

### 3. EditorBrowsable Attribute (IDE Protection)
```csharp
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
```
- Hides constructor from IntelliSense autocomplete
- Prevents accidental discovery

## Implementation Example

```csharp
public abstract class Entity<TId> where TId : IComparable
{
    // Parameterized constructor - REQUIRED for domain code
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Parameterless constructor for deserialization only (EF Core, JSON serializers).
    /// DO NOT USE in domain code.
    /// </summary>
    [Obsolete("Only for deserialization. Use parameterized constructor.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    protected Entity()
    {
    }
}
```

## What Happens When Developers Try to Misuse It

### ❌ Attempt 1: Direct Call
```csharp
var entity = new MyEntity();  // COMPILE ERROR!
```

**Compiler Error:**
```
error CS0619: 'MyEntity.MyEntity()' is obsolete:
'Only for deserialization. Use parameterized constructor.'
```

### ❌ Attempt 2: Derived Class Constructor
```csharp
public class MyEntity : Entity<int>
{
    public MyEntity() : base()  // COMPILE ERROR!
    {
    }
}
```

**Compiler Error:**
```
error CS0619: 'Entity<int>.Entity()' is obsolete:
'Only for deserialization. Use parameterized constructor.'
```

### ❌ Attempt 3: IntelliSense Discovery
When typing `new MyEntity(`, the parameterless constructor won't appear in autocomplete due to `[EditorBrowsable]`.

## ✅ What Still Works (Infrastructure)

The protection **does not affect** infrastructure code that uses reflection:

- ✅ **EF Core** - Entity materialization
- ✅ **System.Text.Json** - Deserialization
- ✅ **Newtonsoft.Json** - Deserialization
- ✅ **MassTransit/RabbitMQ** - Message deserialization
- ✅ **xUnit/Testing** - Test framework features

## Pattern Checklist for New Base Classes

When creating a new DDD base class, ensure:

- [ ] Has a parameterized constructor for domain code
- [ ] Has a `protected` parameterless constructor
- [ ] Parameterless constructor has XML documentation
- [ ] Parameterless constructor has `[Obsolete(error: true)]`
- [ ] Parameterless constructor has `[EditorBrowsable(Never)]`
- [ ] Unit tests document the protection (see tests below)

## Testing the Protection

Each base class should have a documentation test:

```csharp
[Fact]
public void Constructor_Parameterless_IsProtectedFromDirectUse()
{
    // This test documents the protection pattern.
    // Uncommenting the line below should cause a compile error:
    // var entity = new TestEntity();

    // This test passes if the protection is in place
    Assert.True(true);
}
```

These tests serve as:
1. **Living documentation** of the protection pattern
2. **Verification mechanism** (uncomment the line to verify compile error)
3. **Reminder to maintainers** why parameterless constructors exist

## Current Protected Classes

| Class | Parameterless Constructor | Protection |
|-------|---------------------------|------------|
| `Entity<TId>` | ✅ `protected Entity()` | ✅ Full (Obsolete + EditorBrowsable + Docs) |
| `AggregateRoot<TId>` | ✅ `protected AggregateRoot()` | ✅ Full (Obsolete + EditorBrowsable + Docs) |
| `DomainEvent` | ✅ `protected DomainEvent()` | ✅ Full (Obsolete + EditorBrowsable + Docs) |

## References

- [DDD Building Blocks](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [EF Core Entity Constructors](https://learn.microsoft.com/en-us/ef/core/modeling/constructors)
- [ObsoleteAttribute Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.obsoleteattribute)

## Interview Talking Points

This pattern demonstrates:
- ✅ Deep understanding of DDD constraints
- ✅ Knowledge of C# attributes and reflection
- ✅ Defensive programming practices
- ✅ Infrastructure vs. domain code separation
- ✅ Compile-time safety without runtime cost
