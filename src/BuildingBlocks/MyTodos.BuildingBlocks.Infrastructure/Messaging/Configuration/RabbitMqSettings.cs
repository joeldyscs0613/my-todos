namespace MyTodos.BuildingBlocks.Infrastructure.Messaging.Configuration;

/// <summary>
/// RabbitMQ connection and messaging configuration.
/// </summary>
public class RabbitMqSettings
{
    /// <summary>
    /// RabbitMQ server host.
    /// </summary>
    public string Host { get; set; } = "rabbitmq";

    /// <summary>
    /// RabbitMQ server port.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ username.
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ password.
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ virtual host.
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Exchange name for publishing events.
    /// </summary>
    public string Exchange { get; set; } = "mytodos.events";

    /// <summary>
    /// Default routing key for published events.
    /// </summary>
    public string RoutingKey { get; set; } = "mytodos.tasks";
}
