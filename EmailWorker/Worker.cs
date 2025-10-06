using DnDAgency.Application.Messages;
using DnDAgency.EmailWorker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DnDAgency.EmailWorker;


public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private IConnection? _connection;
    private IModel? _channel;

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;
    }


    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email Worker starting...");

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"],
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:Username"],
            Password = _configuration["RabbitMQ:Password"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        var queueName = _configuration["RabbitMQ:QueueName"];

        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        _logger.LogInformation("Connected to RabbitMQ queue: {QueueName}", queueName);

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = _configuration["RabbitMQ:QueueName"];

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Received message: {Message}", json);

                var message = JsonSerializer.Deserialize<BookingConfirmationMessage>(json);

                if (message == null)
                {
                    _logger.LogWarning("Failed to deserialize message");
                    return;
                }

                await _emailService.SendBookingConfirmationAsync(
                    message.UserEmail,
                    message.Username,
                    message.CampaignTitle,
                    message.StartTime,
                    message.PlayersCount
                );

                // TODO: create reminder

                _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                _logger.LogInformation("Message processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");

                _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false, 
            consumer: consumer
        );

        _logger.LogInformation("Started consuming messages from queue: {QueueName}", queueName);

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email Worker stopping...");

        _channel?.Close();
        _connection?.Close();

        return base.StopAsync(cancellationToken);
    }
}