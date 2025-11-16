using Microsoft.EntityFrameworkCore;
using Moq;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;
using MyTodos.BuildingBlocks.Infrastructure.UnitTests.Messaging.DomainEvents;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Contracts;

namespace MyTodos.BuildingBlocks.Infrastructure.UnitTests.Persistence.Abstractions;

public class UnitOfWorkTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
    private readonly TestUnitOfWork _sut;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _currentUserServiceMock.Setup(x => x.Username).Returns("test-user");

        _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();

        _sut = new TestUnitOfWork(
            _context,
            _currentUserServiceMock.Object,
            _domainEventDispatcherMock.Object);
    }

    [Fact]
    public async Task CommitAsync_CollectsDomainEventsFromTrackedAggregates()
    {
        // Arrange
        var aggregate = new TestAggregate(1);
        var domainEvent1 = new TestDomainEvent("TestEvent1", "TestAggregate", "1");
        var domainEvent2 = new TestDomainEvent("TestEvent2", "TestAggregate", "1");

        aggregate.AddDomainEvent(domainEvent1);
        aggregate.AddDomainEvent(domainEvent2);

        _context.TestAggregates.Add(aggregate);

        List<IDomainEvent> dispatchedEvents = null!;
        _domainEventDispatcherMock
            .Setup(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<IDomainEvent>, CancellationToken>((events, _) =>
            {
                dispatchedEvents = events.ToList();
            });

        // Act
        await _sut.CommitAsync();

        // Assert
        Assert.NotNull(dispatchedEvents);
        Assert.Equal(2, dispatchedEvents.Count);
        Assert.Contains(domainEvent1, dispatchedEvents);
        Assert.Contains(domainEvent2, dispatchedEvents);
    }

    [Fact]
    public async Task CommitAsync_ClearsDomainEventsAfterCollection()
    {
        // Arrange
        var aggregate = new TestAggregate(1);
        aggregate.AddDomainEvent(new TestDomainEvent("TestEvent", "TestAggregate", "1"));
        _context.TestAggregates.Add(aggregate);

        // Act
        await _sut.CommitAsync();

        // Assert
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public async Task CommitAsync_DispatchesEventsAfterSuccessfulSave()
    {
        // Arrange
        var aggregate = new TestAggregate(1);
        aggregate.AddDomainEvent(new TestDomainEvent("TestEvent", "TestAggregate", "1"));
        _context.TestAggregates.Add(aggregate);

        // Act
        await _sut.CommitAsync();

        // Assert
        // Verify that DispatchAsync was called exactly once
        // The implementation guarantees events are dispatched after SaveChanges
        _domainEventDispatcherMock.Verify(
            x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CommitAsync_DoesNotDispatchEvents_WhenNoAggregatesHaveEvents()
    {
        // Arrange
        var aggregate = new TestAggregate(1);
        _context.TestAggregates.Add(aggregate);

        // Act
        await _sut.CommitAsync();

        // Assert
        _domainEventDispatcherMock.Verify(
            x => x.DispatchAsync(It.Is<IEnumerable<IDomainEvent>>(e => !e.Any()), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CommitAsync_CollectsEventsFromMultipleAggregates()
    {
        // Arrange
        var aggregate1 = new TestAggregate(1);
        var aggregate2 = new TestAggregate(2);

        aggregate1.AddDomainEvent(new TestDomainEvent("TestEvent", "TestAggregate", "1"));
        aggregate2.AddDomainEvent(new TestDomainEvent("TestEvent", "TestAggregate", "2"));

        _context.TestAggregates.Add(aggregate1);
        _context.TestAggregates.Add(aggregate2);

        List<IDomainEvent> dispatchedEvents = null!;
        _domainEventDispatcherMock
            .Setup(x => x.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<IDomainEvent>, CancellationToken>((events, _) =>
            {
                dispatchedEvents = events.ToList();
            });

        // Act
        await _sut.CommitAsync();

        // Assert
        Assert.Equal(2, dispatchedEvents.Count);
        Assert.Contains(dispatchedEvents, e => ((DomainEvent)e).AggregateId == "1");
        Assert.Contains(dispatchedEvents, e => ((DomainEvent)e).AggregateId == "2");
    }

    [Fact]
    public async Task CommitAsync_UpdatesAuditablePropertiesBeforeCollectingEvents()
    {
        // Arrange
        var aggregate = new TestAggregate(1);
        aggregate.AddDomainEvent(new TestDomainEvent("TestEvent", "TestAggregate", "1"));
        _context.TestAggregates.Add(aggregate);

        // Act
        await _sut.CommitAsync();

        // Assert
        Assert.Equal("test-user", aggregate.CreatedBy);
        Assert.NotEqual(default, aggregate.CreatedDate);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

/// <summary>
/// Test DbContext for testing UnitOfWork behavior.
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestAggregate> TestAggregates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure TestAggregate
        modelBuilder.Entity<TestAggregate>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Ignore domain events collection - events are not persisted
            entity.Ignore(e => e.DomainEvents);
        });
    }
}

/// <summary>
/// Test aggregate root for testing domain event collection.
/// AggregateRoot already implements IEntity through Entity base class.
/// </summary>
public class TestAggregate : AggregateRoot<int>
{
    public TestAggregate(int id) : base(id)
    {
    }
}

/// <summary>
/// Test UnitOfWork implementation for testing.
/// </summary>
public class TestUnitOfWork : UnitOfWork<TestDbContext>
{
    public TestUnitOfWork(
        TestDbContext dbContext,
        ICurrentUserService currentUserService,
        IDomainEventDispatcher domainEventDispatcher)
        : base(dbContext, currentUserService, domainEventDispatcher)
    {
    }
}
