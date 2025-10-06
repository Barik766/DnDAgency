using DnDAgency.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace DnDAgency.Infrastructure.Messaging;
public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;  
    private readonly IModel _channel;         
    private readonly ILogger<RabbitMqPublisher> _logger;
    private const string QueueName = "booking-confirmations"; 

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        try
        {
            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,       
                exclusive: false,     
                autoDelete: false,   
                arguments: null
            );

            _logger.LogInformation("RabbitMQ Publisher connected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
        }
    }

    public Task PublishBookingConfirmationAsync<T>(T message) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(message);

            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; 

            _channel.BasicPublish(
                exchange: "",              
                routingKey: QueueName,     
                basicProperties: properties,
                body: body               
            );

            _logger.LogInformation("Published message to queue {QueueName}", QueueName);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to RabbitMQ");
            return Task.CompletedTask;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}