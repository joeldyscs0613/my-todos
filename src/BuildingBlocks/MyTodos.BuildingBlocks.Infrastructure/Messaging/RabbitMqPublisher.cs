using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTodos.BuildingBlocks.Application.Contracts.Messaging;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.Configuration;
using RabbitMQ.Client;

namespace MyTodos.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Publishes messages to RabbitMQ with connection management and error handling.
/// </summary>
public class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed;

    public RabbitMqPublisher(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task PublishAsync(string eventType, string message, CancellationToken ct = default)
    {
        await EnsureConnectionAsync(ct);

        if (_channel == null)
        {
            throw new InvalidOperationException("RabbitMQ channel is not initialized");
        }

        try
        {
            var exchange = _settings.Exchange;
            var routingKey = _settings.RoutingKey;

            // Convert message to bytes for RabbitMQ
            var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message));

            // Set message properties - persistent delivery ensures messages survive broker restart
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Type = eventType
            };

            // Publish message to RabbitMQ exchange
            await _channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: ct);

            _logger.LogInformation(
                "Published message to RabbitMQ - Exchange: {Exchange}, RoutingKey: {RoutingKey}, EventType: {EventType}",
                exchange,
                routingKey,
                eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to RabbitMQ - EventType: {EventType}", eventType);
            throw;
        }
    }

    private async Task EnsureConnectionAsync(CancellationToken ct = default)
    {
        if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
        {
            return;
        }

        await _connectionLock.WaitAsync(ct);
        try
        {
            if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
            {
                return;
            }

            await DisposeConnectionAsync();

            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost
            };

            _logger.LogInformation("Connecting to RabbitMQ at {Host}:{Port}", _settings.Host, _settings.Port);

            _connection = await factory.CreateConnectionAsync(ct);
            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

            var exchange = _settings.Exchange;

            // Declare exchange (idempotent) - Using Topic for flexible routing
            await _channel.ExchangeDeclareAsync(
                exchange: exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct);

            _logger.LogInformation("Successfully connected to RabbitMQ and declared exchange: {Exchange}", exchange);
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

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await DisposeConnectionAsync();
        _connectionLock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
