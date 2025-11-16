using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.DomainEvents;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Contracts;

namespace MyTodos.BuildingBlocks.Infrastructure.UnitTests.Messaging.DomainEvents;

public class DomainEventDispatcherTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<DomainEventDispatcher>> _loggerMock;
    private readonly DomainEventDispatcher _sut;

    public DomainEventDispatcherTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<DomainEventDispatcher>>();
        _sut = new DomainEventDispatcher(_serviceProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task DispatchAsync_EmptyCollection_DoesNotResolveHandlers()
    {
        // Arrange
        var emptyEvents = new List<TestDomainEvent>();

        // Act
        await _sut.DispatchAsync(emptyEvents, CancellationToken.None);

        // Assert
        _serviceProviderMock.Verify(
            x => x.GetService(It.IsAny<Type>()),
            Times.Never);
    }

    [Fact]
    public async Task DispatchAsync_SingleEvent_InvokesHandlerOnce()
    {
        // Arrange
        var handlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var events = new List<TestDomainEvent>
        {
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "123")
        };

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
            .Returns(new List<IDomainEventHandler<TestDomainEvent>> { handlerMock.Object });

        // Act
        await _sut.DispatchAsync(events, CancellationToken.None);

        // Assert
        handlerMock.Verify(
            x => x.Handle(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_MultipleEvents_InvokesHandlerForEachEvent()
    {
        // Arrange
        var handlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var events = new List<TestDomainEvent>
        {
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "1"),
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "2"),
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "3")
        };

        var handledEvents = new List<string>();
        handlerMock
            .Setup(x => x.Handle(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TestDomainEvent, CancellationToken>((evt, _) =>
            {
                handledEvents.Add(evt.AggregateId);
            })
            .Returns(Task.CompletedTask);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
            .Returns(new List<IDomainEventHandler<TestDomainEvent>> { handlerMock.Object });

        // Act
        await _sut.DispatchAsync(events, CancellationToken.None);

        // Assert
        handlerMock.Verify(
            x => x.Handle(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));

        // Verify sequential order
        Assert.Equal(new[] { "1", "2", "3" }, handledEvents);
    }

    [Fact]
    public async Task DispatchAsync_MultipleHandlersForSameEvent_InvokesAllHandlers()
    {
        // Arrange
        var handler1Mock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var handler2Mock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var events = new List<TestDomainEvent>
        {
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "123")
        };

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
            .Returns(new List<IDomainEventHandler<TestDomainEvent>> { handler1Mock.Object, handler2Mock.Object });

        // Act
        await _sut.DispatchAsync(events, CancellationToken.None);

        // Assert
        handler1Mock.Verify(
            x => x.Handle(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        handler2Mock.Verify(
            x => x.Handle(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_NoHandlersRegistered_LogsWarning()
    {
        // Arrange
        var events = new List<TestDomainEvent>
        {
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "123")
        };

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
            .Returns(new List<IDomainEventHandler<TestDomainEvent>>());

        // Act
        await _sut.DispatchAsync(events, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No handlers registered")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_HandlerThrowsException_LogsErrorAndContinuesProcessing()
    {
        // Arrange
        var failingHandlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var successHandlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();

        var testException = new InvalidOperationException("Test handler error");

        failingHandlerMock
            .Setup(x => x.Handle(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(testException);

        var events = new List<TestDomainEvent>
        {
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "123")
        };

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
            .Returns(new List<IDomainEventHandler<TestDomainEvent>>
            {
                failingHandlerMock.Object,
                successHandlerMock.Object
            });

        // Act
        await _sut.DispatchAsync(events, CancellationToken.None);

        // Assert - Error is logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error executing handler")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);

        // Assert - Second handler still gets called
        successHandlerMock.Verify(
            x => x.Handle(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_PassesCancellationTokenToHandlers()
    {
        // Arrange
        var handlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var events = new List<TestDomainEvent>
        {
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "123")
        };
        var cancellationToken = new CancellationTokenSource().Token;

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
            .Returns(new List<IDomainEventHandler<TestDomainEvent>> { handlerMock.Object });

        // Act
        await _sut.DispatchAsync(events, cancellationToken);

        // Assert
        handlerMock.Verify(
            x => x.Handle(It.IsAny<TestDomainEvent>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_LogsDispatchingStartAndCompletion()
    {
        // Arrange
        var handlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var events = new List<TestDomainEvent>
        {
            new TestDomainEvent("TestEventOccurred", "TestAggregate", "123")
        };

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
            .Returns(new List<IDomainEventHandler<TestDomainEvent>> { handlerMock.Object });

        // Act
        await _sut.DispatchAsync(events, CancellationToken.None);

        // Assert - Logs start
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Dispatching") && v.ToString()!.Contains("domain events")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);

        // Assert - Logs completion
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully dispatched")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
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
