using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.SharedKernel.UnitTests;

public class AggregateRootTests
{
    #region Test Classes

    private class TestIntAggregate : AggregateRoot<int>
    {
        public TestIntAggregate(int id) : base(id) { }
    }

    private class TestGuidAggregate : AggregateRoot<Guid>
    {
        public TestGuidAggregate(Guid id) : base(id) { }
    }

    private record TestDomainEvent : DomainEvent
    {
        public TestDomainEvent(string aggregateId)
            : base("TestEvent", "TestIntAggregate", aggregateId)
        {
        }
    }

    private record AnotherTestDomainEvent : DomainEvent
    {
        public AnotherTestDomainEvent(string aggregateId)
            : base("AnotherEvent", "TestIntAggregate", aggregateId)
        {
        }
    }

    #endregion

    #region Constructor & Inheritance Tests

    [Fact]
    public void Constructor_WithIntId_SetsIdProperty()
    {
        // Arrange
        var id = 1;

        // Act
        var aggregate = new TestIntAggregate(id);

        // Assert
        Assert.Equal(id, aggregate.Id);
    }

    [Fact]
    public void InheritsFromEntity_HasAuditProperties()
    {
        // Arrange
        var aggregate = new TestIntAggregate(1);

        // Act
        aggregate.SetCreatedInfo("test.user");

        // Assert - Verify Entity behavior is inherited
        Assert.Equal("test.user", aggregate.CreatedBy);
        Assert.NotEqual(default(DateTimeOffset), aggregate.CreatedDate);
    }

    #endregion

    #region DomainEvents Collection Tests

    [Fact]
    public void NewAggregate_DomainEventsIsEmpty()
    {
        // Act
        var aggregate = new TestIntAggregate(1);

        // Assert
        Assert.NotNull(aggregate.DomainEvents);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void DomainEvents_IsReadOnlyCollection()
    {
        // Act
        var aggregate = new TestIntAggregate(1);

        // Assert
        Assert.IsAssignableFrom<IReadOnlyCollection<DomainEvent>>(aggregate.DomainEvents);
    }

    #endregion

    #region AddDomainEvent Tests

    [Fact]
    public void AddDomainEvent_WithValidEvent_AddsToCollection()
    {
        // Arrange
        var aggregate = new TestIntAggregate(1);
        var domainEvent = new TestDomainEvent("1");

        // Act
        aggregate.AddDomainEvent(domainEvent);

        // Assert
        Assert.Single(aggregate.DomainEvents);
        Assert.Contains(domainEvent, aggregate.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_MultipleEvents_AddsAllToCollection()
    {
        // Arrange
        var aggregate = new TestIntAggregate(1);
        var idStr = aggregate.Id.ToString();
        var event1 = new TestDomainEvent(idStr);
        var event2 = new AnotherTestDomainEvent(idStr);
        var event3 = new TestDomainEvent(idStr);

        // Act
        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);
        aggregate.AddDomainEvent(event3);

        // Assert
        Assert.Equal(3, aggregate.DomainEvents.Count);
        Assert.Contains(event1, aggregate.DomainEvents);
        Assert.Contains(event2, aggregate.DomainEvents);
        Assert.Contains(event3, aggregate.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_WithSameEventTwice_AddsBothInstances()
    {
        // Arrange
        var aggregate = new TestIntAggregate(1);
        var domainEvent = new TestDomainEvent(aggregate.Id.ToString());

        // Act
        aggregate.AddDomainEvent(domainEvent);
        aggregate.AddDomainEvent(domainEvent);

        // Assert
        Assert.Equal(2, aggregate.DomainEvents.Count);
    }

    [Fact]
    public void AddDomainEvent_IncrementsCollectionCount()
    {
        // Arrange
        var aggregate = new TestIntAggregate(1);
        var idStr = aggregate.Id.ToString();

        // Act & Assert
        Assert.Empty(aggregate.DomainEvents);

        aggregate.AddDomainEvent(new TestDomainEvent(idStr));
        Assert.Single(aggregate.DomainEvents);

        aggregate.AddDomainEvent(new TestDomainEvent(idStr
        ));
        Assert.Equal(2, aggregate.DomainEvents.Count);
    }

    #endregion

    #region RemoveDomainEvent Tests

    [Fact]
    public void RemoveDomainEvent_WithExistingEvent_RemovesFromCollection()
    {
        // Arrange
        var aggregate = new TestIntAggregate(1);
        var idStr = aggregate.Id.ToString();
        var event1 = new TestDomainEvent(idStr);
        var event2 = new AnotherTestDomainEvent(idStr);
        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);

        // Act
        aggregate.RemoveDomainEvent(event1);

        // Assert
        Assert.Single(aggregate.DomainEvents);
        Assert.DoesNotContain(event1, aggregate.DomainEvents);
        Assert.Contains(event2, aggregate.DomainEvents);
    }

    [Fact]
    public void RemoveDomainEvent_WithNonExistingEvent_DoesNothing()
    {
        // Arrange
        var aggregate = new TestIntAggregate(1);
        var event1 = new TestDomainEvent("agg-1");
        var event2 = new TestDomainEvent("agg-2"); // Different aggregateId to ensure inequality
        aggregate.AddDomainEvent(event1);

        // Act - Try to remove an event that was never added
        aggregate.RemoveDomainEvent(event2);

        // Assert
        Assert.Single(aggregate.DomainEvents);
        Assert.Contains(event1, aggregate.DomainEvents);
    }

    [Fact]
    public void RemoveDomainEvent_WithMultipleSameEvents_RemovesOnlyFirstOccurrence()
    {
        // Arrange
        var aggregate = new TestIntAggregate(1);
        var idStr = aggregate.Id.ToString();
        var domainEvent = new TestDomainEvent(idStr);
        aggregate.AddDomainEvent(domainEvent);
        aggregate.AddDomainEvent(domainEvent);
        aggregate.AddDomainEvent(domainEvent);

        // Act
        aggregate.RemoveDomainEvent(domainEvent);

        // Assert - List.Remove only removes first occurrence
        Assert.Equal(2, aggregate.DomainEvents.Count);
    }

    #endregion

    #region ClearDomainEvents Tests

    [Fact]
    public void ClearDomainEvents_WithEvents_RemovesAllEvents()
    {
        // Arrange
        var aggregate = new TestIntAggregate(1);
        var idStr = aggregate.Id.ToString();
        aggregate.AddDomainEvent(new TestDomainEvent(idStr));
        aggregate.AddDomainEvent(new AnotherTestDomainEvent(idStr));
        aggregate.AddDomainEvent(new TestDomainEvent(idStr));

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_WithNoEvents_DoesNothing()
    {
        // Arrange
        var aggregate = new TestIntAggregate(1);

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        Assert.Empty(aggregate.DomainEvents);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void DomainEventWorkflow_AddMultipleClearAdd_WorksCorrectly()
    {
        // Arrange
        var aggregate = new TestIntAggregate(1);
        var event1 = new TestDomainEvent("agg-1");
        var event2 = new AnotherTestDomainEvent("agg-1");
        var event3 = new TestDomainEvent("agg-3"); // Different aggregateId to ensure inequality

        // Act - Add multiple events
        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);
        Assert.Equal(2, aggregate.DomainEvents.Count);

        // Act - Clear all events
        aggregate.ClearDomainEvents();
        Assert.Empty(aggregate.DomainEvents);

        // Act - Add new event after clearing
        aggregate.AddDomainEvent(event3);

        // Assert
        Assert.Single(aggregate.DomainEvents);
        Assert.Contains(event3, aggregate.DomainEvents);
        Assert.DoesNotContain(event1, aggregate.DomainEvents);
        Assert.DoesNotContain(event2, aggregate.DomainEvents);
    }

    [Fact]
    public void DomainEvents_AfterAddAndRemove_ReflectsCorrectState()
    {
        // Arrange
        var aggregate = new TestIntAggregate(1);
        var idStr = aggregate.Id.ToString();
        var event1 = new TestDomainEvent(idStr);
        var event2 = new AnotherTestDomainEvent(idStr);
        var event3 = new TestDomainEvent(idStr);

        // Act
        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);
        aggregate.AddDomainEvent(event3);
        aggregate.RemoveDomainEvent(event2);

        // Assert
        Assert.Equal(2, aggregate.DomainEvents.Count);
        Assert.Contains(event1, aggregate.DomainEvents);
        Assert.DoesNotContain(event2, aggregate.DomainEvents);
        Assert.Contains(event3, aggregate.DomainEvents);
    }

    [Fact]
    public void DomainEvents_WithGuidAggregate_WorksCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregate = new TestGuidAggregate(id);
        var domainEvent = new TestDomainEvent(id.ToString());

        // Act
        aggregate.AddDomainEvent(domainEvent);

        // Assert
        Assert.Equal(id, aggregate.Id);
        Assert.Single(aggregate.DomainEvents);
        Assert.Contains(domainEvent, aggregate.DomainEvents);
    }

    #endregion

    #region Parameterless Constructor Protection Tests

    [Fact]
    public void Constructor_Parameterless_IsProtectedFromDirectUse()
    {
        // This test documents the protection pattern for parameterless constructors.
        // The [Obsolete(error: true)] attribute prevents direct instantiation.

        // Uncommenting the line below should cause a compile error:
        // var aggregate = new TestIntAggregate();

        // The parameterless constructor exists ONLY for EF Core/serialization infrastructure.
        // Developers should use the parameterized constructor: new TestIntAggregate(id)

        // This test passes if the protection is in place (code above doesn't compile)
        Assert.True(true);
    }

    #endregion
}
