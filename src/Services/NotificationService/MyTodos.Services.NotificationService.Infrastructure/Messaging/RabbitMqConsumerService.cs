using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTodos.BuildingBlocks.Application.Contracts.IntegrationEvents;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyTodos.Services.NotificationService.Infrastructure.Messaging;

/// <summary>
/// Background service that consumes messages from RabbitMQ and routes them to integration event handlers.
/// Implements retry logic with exponential backoff for failed messages.
/// </summary>
public sealed class RabbitMqConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly RabbitMqConsumerSettings _consumerSettings;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed;

    public RabbitMqConsumerService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<RabbitMqSettings> rabbitMqSettings,
        IOptions<RabbitMqConsumerSettings> consumerSettings,
        ILogger<RabbitMqConsumerService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _rabbitMqSettings = rabbitMqSettings.Value;
        _consumerSettings = consumerSettings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation(
            "RabbitMQ Consumer started - Queue: {Queue}, Exchange: {Exchange}, RoutingKey: {RoutingKey}",
            _consumerSettings.QueueName,
            _consumerSettings.Exchange,
            _consumerSettings.RoutingKey);

        // Wait for the application to fully start and for RabbitMQ to be ready. In production, we can have
        // the delay as a setting and twick it accordingly
        await Task.Delay(TimeSpan.FromSeconds(10), ct);

        try
        {
            await EnsureConnectionAsync(ct);
            await StartConsumingAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in RabbitMQ consumer");
            throw;
        }
    }

    private async Task EnsureConnectionAsync(CancellationToken ct)
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
        {
            return;
        }

        await _connectionLock.WaitAsync(ct);
        
        try
        {
            if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
            {
                return;
            }

            await DisposeConnectionAsync();

            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqSettings.Host,
                Port = _rabbitMqSettings.Port,
                UserName = _rabbitMqSettings.Username,
                Password = _rabbitMqSettings.Password,
                VirtualHost = _rabbitMqSettings.VirtualHost,
                // Enable automatic recovery for connection resilience
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _logger.LogInformation(
                "Connecting to RabbitMQ at {Host}:{Port}",
                _rabbitMqSettings.Host,
                _rabbitMqSettings.Port);

            _connection = await factory.CreateConnectionAsync(ct);
            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

            // Set prefetch count (quality of service) - limits how many unacknowledged messages
            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: _consumerSettings.PrefetchCount,
                global: false,
                cancellationToken: ct);

            // Declare exchange (idempotent - safe to call even if it exists)
            await _channel.ExchangeDeclareAsync(
                exchange: _consumerSettings.Exchange,
                type: ExchangeType.Topic, // Using Topic for flexible routing
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct);

            // Declare queue (idempotent)
            await _channel.QueueDeclareAsync(
                queue: _consumerSettings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct);

            // Bind queue to exchange with routing key
            await _channel.QueueBindAsync(
                queue: _consumerSettings.QueueName,
                exchange: _consumerSettings.Exchange,
                routingKey: _consumerSettings.RoutingKey,
                arguments: null,
                cancellationToken: ct);

            _logger.LogInformation(
                "Successfully connected to RabbitMQ - Queue: {Queue} bound " +
                "to Exchange: {Exchange} with RoutingKey: {RoutingKey}",
                _consumerSettings.QueueName,
                _consumerSettings.Exchange,
                _consumerSettings.RoutingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task StartConsumingAsync(CancellationToken ct)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("RabbitMQ channel is not initialized");
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                await ProcessMessageAsync(ea, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing message");
                // Reject message and requeue for retry
                await _channel.BasicNackAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: ct);
            }
        };

        // Start consuming messages
        await _channel.BasicConsumeAsync(
            queue: _consumerSettings.QueueName,
            autoAck: false, // Manual acknowledgment for reliability
            consumer: consumer,
            cancellationToken: ct);

        _logger.LogInformation("Started consuming messages from queue: {Queue}", _consumerSettings.QueueName);

        // Keep the service running
        await Task.Delay(Timeout.Infinite, ct);
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea, CancellationToken ct)
    {
        var messageBody = Encoding.UTF8.GetString(ea.Body.ToArray());
        var eventType = ea.BasicProperties.Type ?? "Unknown";
        var deliveryTag = ea.DeliveryTag;

        _logger.LogInformation(
            "Received message - Type: {EventType}, DeliveryTag: {DeliveryTag}",
            eventType,
            deliveryTag);

        // Get retry count from message headers (if exists)
        var currentRetryCount = GetRetryCount(ea.BasicProperties);

        try
        {
            await RouteToHandlerAsync(eventType, messageBody, ct);

            // Successfully processed - acknowledge message
            if (_channel != null)
            {
                await _channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: ct);
                _logger.LogInformation("Message acknowledged - Type: {EventType}", eventType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process message - Type: {EventType}, Retry: {RetryCount}/{MaxRetries}",
                eventType,
                currentRetryCount,
                _consumerSettings.MaxRetries);

            if (_channel != null)
            {
                // Check if we should retry
                if (currentRetryCount < _consumerSettings.MaxRetries)
                {
                    // Calculate backoff delay
                    var delayMs = CalculateBackoffDelay(currentRetryCount);
                    _logger.LogWarning(
                        "Retrying message after {Delay}ms - Type: {EventType}, Retry: {RetryCount}",
                        delayMs,
                        eventType,
                        currentRetryCount + 1);

                    await Task.Delay(delayMs, ct);

                    // Reject and requeue for retry
                    await _channel.BasicNackAsync(
                        deliveryTag: deliveryTag,
                        multiple: false,
                        requeue: true,
                        cancellationToken: ct);
                }
                else
                {
                    // Max retries exceeded - acknowledge to remove from queue
                    _logger.LogError(
                        "Max retries exceeded for message - Type: {EventType}. Acknowledging to remove from queue.",
                        eventType);

                    await _channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: ct);
                }
            }
        }
    }

    private async Task RouteToHandlerAsync(string eventType, string messageBody, CancellationToken ct)
    {
        // Create a new scope for handler dependencies
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        // Route to appropriate handler based on event type
        switch (eventType)
        {
            case MyTodos.Services.IdentityService.Contracts.IntegrationEvents.UserCreatedIntegrationEvent.EventName:
            {
                var integrationEvent = JsonSerializer.Deserialize<
                    MyTodos.Services.IdentityService.Contracts.IntegrationEvents.UserCreatedIntegrationEvent>(
                    messageBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (integrationEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize UserCreatedIntegrationEvent");
                    return;
                }

                var handler = scope.ServiceProvider.GetRequiredService<
                    IIntegrationEventHandler<
                        MyTodos.Services.IdentityService.Contracts.IntegrationEvents.UserCreatedIntegrationEvent>>();

                await handler.HandleAsync(integrationEvent, ct);
                break;
            }

            default:
                _logger.LogWarning("No handler registered for event type: {EventType}", eventType);
                break;
        }
    }

    private static int GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers != null &&
            properties.Headers.TryGetValue("x-retry-count", out var retryCountObj) &&
            retryCountObj is int retryCount)
        {
            return retryCount;
        }

        return 0;
    }

    private int CalculateBackoffDelay(int retryCount)
    {
        var delay = _consumerSettings.InitialRetryDelayMs *
                    Math.Pow(_consumerSettings.RetryBackoffMultiplier, retryCount);

        return (int)Math.Min(delay, 60000); // Cap at 60 seconds
    }

    private async Task DisposeConnectionAsync()
    {
        if (_channel != null)
        {
            try
            {
                await _channel.CloseAsync();
            }
            catch
            {
                // Ignore errors during cleanup
            }
            finally
            {
                await _channel.DisposeAsync();
                _channel = null;
            }
        }

        if (_connection != null)
        {
            try
            {
                await _connection.CloseAsync();
            }
            catch
            {
                // Ignore errors during cleanup
            }
            finally
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }

    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DisposeConnectionAsync().GetAwaiter().GetResult();
        _connectionLock.Dispose();
        _disposed = true;
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
