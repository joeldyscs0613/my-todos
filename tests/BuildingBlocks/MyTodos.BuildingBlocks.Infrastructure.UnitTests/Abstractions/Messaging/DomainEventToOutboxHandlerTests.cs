using Microsoft.Extensions.Logging;
using Moq;
using MyTodos.BuildingBlocks.Application.Abstractions.IntegrationEvents;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.Abstractions;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.DomainEvents;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.UnitTests.Abstractions.Messaging;

public class DomainEventToOutboxHandlerTests
{
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
    private readonly Mock<ILogger<TestDomainEventToOutboxHandler>> _loggerMock;
    private readonly TestDomainEventToOutboxHandler _sut;

    public DomainEventToOutboxHandlerTests()
    {
        _outboxRepositoryMock = new Mock<IOutboxRepository>();
        _loggerMock = new Mock<ILogger<TestDomainEventToOutboxHandler>>();
        _sut = new TestDomainEventToOutboxHandler(_outboxRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ConvertsAndSavesToOutbox()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("TestEvent", "Aggregate", "123");
        IntegrationEvent? capturedIntegrationEvent = null;

        _outboxRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<IntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IntegrationEvent, CancellationToken>((evt, _) => capturedIntegrationEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(domainEvent, CancellationToken.None);

        // Assert
        _outboxRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<IntegrationEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.NotNull(capturedIntegrationEvent);
        Assert.Equal("TestIntegrationEvent", capturedIntegrationEvent.EventType);
        Assert.NotEqual(Guid.Empty, capturedIntegrationEvent.EventId);
    }

    [Fact]
    public async Task Handle_CreatesCorrectIntegrationEvent()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("TestEvent", "Aggregate", "123");
        IntegrationEvent? capturedIntegrationEvent = null;

        _outboxRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<IntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IntegrationEvent, CancellationToken>((evt, _) => capturedIntegrationEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(domainEvent, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedIntegrationEvent);
        var testEvent = Assert.IsType<TestIntegrationEvent>(capturedIntegrationEvent);
        Assert.Equal("TestIntegrationEvent", testEvent.EventType);
        Assert.Equal("123", testEvent.AggregateId);
    }

    [Fact]
    public async Task Handle_SetsIntegrationEventOccurredOn()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("TestEvent", "Aggregate", "123");
        IntegrationEvent? capturedIntegrationEvent = null;

        _outboxRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<IntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IntegrationEvent, CancellationToken>((evt, _) => capturedIntegrationEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(domainEvent, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedIntegrationEvent);
        // OccurredOn should be close to now (within a few seconds)
        Assert.True(capturedIntegrationEvent.OccurredOn <= DateTime.UtcNow);
        Assert.True(capturedIntegrationEvent.OccurredOn >= DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public async Task Handle_PassesCancellationToken()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("TestEvent", "Aggregate", "123");
        var cancellationToken = new CancellationTokenSource().Token;

        _outboxRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<IntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(domainEvent, cancellationToken);

        // Assert
        _outboxRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<IntegrationEvent>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PropagatesExceptionFromToIntegrationEvent()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("TestEvent", "Aggregate", "123");
        var failingLogger = new Mock<ILogger<FailingTestDomainEventToOutboxHandler>>();
        var failingHandler = new FailingTestDomainEventToOutboxHandler(
            _outboxRepositoryMock.Object,
            failingLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => failingHandler.Handle(domainEvent, CancellationToken.None));

        // Repository should not be called
        _outboxRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<IntegrationEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_PropagatesExceptionFromRepository()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("TestEvent", "Aggregate", "123");
        var repositoryException = new InvalidOperationException("Database error");

        _outboxRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<IntegrationEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(repositoryException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Handle(domainEvent, CancellationToken.None));

        Assert.Equal("Database error", exception.Message);
    }
}

// Test domain event
public sealed record TestDomainEvent : DomainEvent
{
    public TestDomainEvent(string eventType, string aggregateType, string aggregateId)
        : base(eventType, aggregateType, aggregateId)
    {
    }
}

// Test integration event
public sealed record TestIntegrationEvent : IntegrationEvent
{
    public string AggregateId { get; init; }

    public TestIntegrationEvent(string aggregateId)
    {
        EventType = "TestIntegrationEvent";
        AggregateId = aggregateId;
    }
}

// Test handler implementation
public class TestDomainEventToOutboxHandler : DomainEventToOutboxHandler<TestDomainEvent>
{
    public TestDomainEventToOutboxHandler(
        IOutboxRepository outboxRepository,
        ILogger<TestDomainEventToOutboxHandler> logger)
        : base(outboxRepository, logger)
    {
    }

    protected override Task<IntegrationEvent> ToIntegrationEvent(
        TestDomainEvent domainEvent,
        CancellationToken ct)
    {
        var integrationEvent = new TestIntegrationEvent(domainEvent.AggregateId);
        return Task.FromResult<IntegrationEvent>(integrationEvent);
    }
}

// Failing test handler
public class FailingTestDomainEventToOutboxHandler : DomainEventToOutboxHandler<TestDomainEvent>
{
    public FailingTestDomainEventToOutboxHandler(
        IOutboxRepository outboxRepository,
        ILogger<FailingTestDomainEventToOutboxHandler> logger)
        : base(outboxRepository, logger)
    {
    }

    protected override Task<IntegrationEvent> ToIntegrationEvent(
        TestDomainEvent domainEvent,
        CancellationToken ct)
    {
        throw new InvalidOperationException("Conversion failed");
    }
}
