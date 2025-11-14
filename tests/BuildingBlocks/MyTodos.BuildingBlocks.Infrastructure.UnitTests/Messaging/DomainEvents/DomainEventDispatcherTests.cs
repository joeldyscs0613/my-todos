using MediatR;
using Moq;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.DomainEvents;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.UnitTests.Messaging.DomainEvents;

public class DomainEventDispatcherTests
{
    private readonly Mock<IPublisher> _publisherMock;
    private readonly DomainEventDispatcher _sut;

    public DomainEventDispatcherTests()
    {
        _publisherMock = new Mock<IPublisher>();
        _sut = new DomainEventDispatcher(_publisherMock.Object);
    }

    [Fact]
    public void Constructor_NullPublisher_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new DomainEventDispatcher(null!));
        Assert.Equal("publisher", exception.ParamName);
    }

    [Fact]
    public async Task DispatchAsync_NullDomainEvents_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _sut.DispatchAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task DispatchAsync_EmptyCollection_DoesNotPublishEvents()
    {
        // Arrange
        var emptyEvents = new List<TestDomainEvent>();

        // Act
        await _sut.DispatchAsync(emptyEvents, CancellationToken.None);

        // Assert
        _publisherMock.Verify(
            x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DispatchAsync_SingleEvent_PublishesEventOnce()
    {
        // Arrange
        var events = new List<TestDomainEvent>
        {
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "123")
        };

        // Act
        await _sut.DispatchAsync(events, CancellationToken.None);

        // Assert
        _publisherMock.Verify(
            x => x.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_MultipleEvents_PublishesAllEventsSequentially()
    {
        // Arrange
        var events = new List<TestDomainEvent>
        {
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "1"),
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "2"),
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "3")
        };

        var publishOrder = new List<string>();
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((evt, _) =>
            {
                var domainEvent = (TestDomainEvent)evt;
                publishOrder.Add(domainEvent.AggregateId);
            });

        // Act
        await _sut.DispatchAsync(events, CancellationToken.None);

        // Assert
        _publisherMock.Verify(
            x => x.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));

        // Verify sequential order
        Assert.Equal(new[] { "1", "2", "3" }, publishOrder);
    }

    [Fact]
    public async Task DispatchAsync_PassesCancellationTokenToPublisher()
    {
        // Arrange
        var events = new List<TestDomainEvent>
        {
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "123")
        };
        var cancellationToken = new CancellationToken();

        // Act
        await _sut.DispatchAsync(events, cancellationToken);

        // Assert
        _publisherMock.Verify(
            x => x.Publish(It.IsAny<TestDomainEvent>(), cancellationToken),
            Times.Once);
    }
}

/// <summary>
/// Test implementation of DomainEvent for testing purposes.
/// </summary>
public sealed record TestDomainEvent : DomainEvent
{
    public TestDomainEvent(string eventType, string aggregateType, string aggregateId)
        : base(eventType, aggregateType, aggregateId)
    {
    }
}
