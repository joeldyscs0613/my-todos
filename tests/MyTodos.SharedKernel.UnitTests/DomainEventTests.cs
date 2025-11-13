using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.SharedKernel.UnitTests;

public class DomainEventTests
{
    #region Test Event Classes

    private record TaskCreatedEvent : DomainEvent
    {
        public TaskCreatedEvent(string aggregateId)
            : base("TaskCreated", "Task", aggregateId)
        {
        }
    }

    private record TaskCompletedEvent : DomainEvent
    {
        public TaskCompletedEvent(string aggregateId)
            : base("TaskCompleted", "Task", aggregateId)
        {
        }
    }

    #endregion

    #region Constructor & Properties Tests

    [Fact]
    public void Constructor_WithValidParameters_SetsAllProperties()
    {
        // Arrange
        var aggregateId = "task-123";

        // Act
        var domainEvent = new TaskCreatedEvent(aggregateId);

        // Assert
        Assert.Equal("TaskCreated", domainEvent.EventType);
        Assert.Equal("Task", domainEvent.AggregateType);
        Assert.Equal(aggregateId, domainEvent.AggregateId);
        Assert.NotEqual(default(DateTimeOffset), domainEvent.OccurredOn);
    }

    [Fact]
    public void Constructor_SetsEventTypeCorrectly()
    {
        // Act
        var domainEvent = new TaskCreatedEvent("task-123");

        // Assert
        Assert.Equal("TaskCreated", domainEvent.EventType);
    }

    [Fact]
    public void Constructor_SetsAggregateTypeCorrectly()
    {
        // Act
        var domainEvent = new TaskCreatedEvent("task-123");

        // Assert
        Assert.Equal("Task", domainEvent.AggregateType);
    }

    [Fact]
    public void Constructor_SetsAggregateIdCorrectly()
    {
        // Arrange
        var aggregateId = "task-456";

        // Act
        var domainEvent = new TaskCreatedEvent(aggregateId);

        // Assert
        Assert.Equal(aggregateId, domainEvent.AggregateId);
    }

    [Fact]
    public void Constructor_SetsOccurredOnToUtcNow()
    {
        // Arrange
        var before = DateTimeOffsetHelper.UtcNow;

        // Act
        var domainEvent = new TaskCreatedEvent("task-123");

        // Assert
        var after = DateTimeOffsetHelper.UtcNow;
        Assert.True(domainEvent.OccurredOn >= before);
        Assert.True(domainEvent.OccurredOn <= after);
    }

    #endregion

    #region Constructor Validation - eventType Parameter

    [Fact]
    public void Constructor_WithNullEventType_ThrowsArgumentException()
    {
        // This test requires a custom event record that exposes the base constructor
        // Since we can't directly call the protected constructor, we create a test record
        Assert.Throws<ArgumentNullException>(() =>
            new TestEventWithNullEventType("task-123")
        );
    }

    [Fact]
    public void Constructor_WithEmptyEventType_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new TestEventWithEmptyEventType("task-123")
        );
    }

    [Fact]
    public void Constructor_WithWhitespaceEventType_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new TestEventWithWhitespaceEventType("task-123")
        );
    }

    #endregion

    #region Constructor Validation - aggregateType Parameter

    [Fact]
    public void Constructor_WithNullAggregateType_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new TestEventWithNullAggregateType("task-123")
        );
    }

    [Fact]
    public void Constructor_WithEmptyAggregateType_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new TestEventWithEmptyAggregateType("task-123")
        );
    }

    [Fact]
    public void Constructor_WithWhitespaceAggregateType_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new TestEventWithWhitespaceAggregateType("task-123")
        );
    }

    #endregion

    #region Constructor Validation - aggregateId Parameter

    [Fact]
    public void Constructor_WithNullAggregateId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new TaskCreatedEvent(null!)
        );
    }

    [Fact]
    public void Constructor_WithEmptyAggregateId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new TaskCreatedEvent(string.Empty)
        );
    }

    [Fact]
    public void Constructor_WithWhitespaceAggregateId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new TaskCreatedEvent("   ")
        );
    }

    #endregion

    #region UTC Timestamp Tests

    [Fact]
    public void OccurredOn_UsesUtcTime()
    {
        // Act
        var domainEvent = new TaskCreatedEvent("task-123");

        // Assert
        Assert.Equal(TimeSpan.Zero, domainEvent.OccurredOn.Offset);
    }

    [Fact]
    public void OccurredOn_IsCurrentTime()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var domainEvent = new TaskCreatedEvent("task-123");

        // Assert
        var after = DateTimeOffset.UtcNow;
        Assert.True(domainEvent.OccurredOn >= before);
        Assert.True(domainEvent.OccurredOn <= after);
        Assert.True((after - before).TotalSeconds < 1);
    }

    #endregion

    #region Record Equality Tests

    [Fact]
    public void DomainEvents_WithSameValues_AreEqual()
    {
        // Arrange
        var aggregateId = "task-123";
        var event1 = new TaskCreatedEvent(aggregateId);

        // Need to create event2 with same timestamp for true equality
        var event2 = event1 with { }; // Record with-expression creates a copy

        // Assert
        Assert.Equal(event1, event2);
    }

    [Fact]
    public void DomainEvents_WithDifferentEventType_AreNotEqual()
    {
        // Arrange
        var aggregateId = "task-123";
        DomainEvent event1 = new TaskCreatedEvent(aggregateId);
        DomainEvent event2 = new TaskCompletedEvent(aggregateId);

        // Assert - Different types means different EventType values
        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void DomainEvents_WithDifferentAggregateId_AreNotEqual()
    {
        // Arrange
        var event1 = new TaskCreatedEvent("task-123");
        var event2 = new TaskCreatedEvent("task-456");

        // Assert
        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void DomainEvents_WithDifferentOccurredOn_AreNotEqual()
    {
        // Arrange
        var aggregateId = "task-123";
        var event1 = new TaskCreatedEvent(aggregateId);

        System.Threading.Thread.Sleep(1); // Ensure different timestamp

        var event2 = new TaskCreatedEvent(aggregateId);

        // Assert - Different timestamps mean not equal
        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void DifferentEventTypes_WithSameValues_AreNotEqual()
    {
        // Arrange
        var aggregateId = "task-123";
        DomainEvent createdEvent = new TaskCreatedEvent(aggregateId);
        DomainEvent completedEvent = new TaskCompletedEvent(aggregateId);

        // Assert - Different event types are not equal even with same aggregateId
        Assert.NotEqual(createdEvent, completedEvent);
        Assert.NotEqual(createdEvent.EventType, completedEvent.EventType);
    }

    #endregion

    #region Test Helper Classes for Validation

    private record TestEventWithNullEventType : DomainEvent
    {
        public TestEventWithNullEventType(string aggregateId)
            : base(null!, "TestAggregate", aggregateId)
        {
        }
    }

    private record TestEventWithEmptyEventType : DomainEvent
    {
        public TestEventWithEmptyEventType(string aggregateId)
            : base(string.Empty, "TestAggregate", aggregateId)
        {
        }
    }

    private record TestEventWithWhitespaceEventType : DomainEvent
    {
        public TestEventWithWhitespaceEventType(string aggregateId)
            : base("   ", "TestAggregate", aggregateId)
        {
        }
    }

    private record TestEventWithNullAggregateType : DomainEvent
    {
        public TestEventWithNullAggregateType(string aggregateId)
            : base("TestEvent", null!, aggregateId)
        {
        }
    }

    private record TestEventWithEmptyAggregateType : DomainEvent
    {
        public TestEventWithEmptyAggregateType(string aggregateId)
            : base("TestEvent", string.Empty, aggregateId)
        {
        }
    }

    private record TestEventWithWhitespaceAggregateType : DomainEvent
    {
        public TestEventWithWhitespaceAggregateType(string aggregateId)
            : base("TestEvent", "   ", aggregateId)
        {
        }
    }

    #endregion
}
