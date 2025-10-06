using DnDAgency.Application.Messages;
using DnDAgency.EmailWorker.Services;
using Hangfire;
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
    private readonly IBackgroundJobClient _backgroundJobs;
    private IConnection? _connection;
    private IModel? _channel;

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        IEmailService emailService,
        IBackgroundJobClient backgroundJobs)
    {
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;
        _backgroundJobs = backgroundJobs;
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

                ScheduleReminders(message);

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

    private void ScheduleReminders(BookingConfirmationMessage message)
    {
        var now = DateTime.UtcNow;
        var gameTime = message.StartTime;

        // Rminder 30 days
        var monthBefore = gameTime.AddDays(-30);
        if (monthBefore > now)
        {
            _backgroundJobs.Schedule<ReminderService>(
                service => service.SendMonthReminderAsync(
                    message.BookingId,
                    message.UserEmail,
                    message.Username,
                    message.CampaignTitle,
                    message.StartTime
                ),
                monthBefore
            );
            _logger.LogInformation("Scheduled 30-day reminder for {Time}", monthBefore);
        }

        // reminder 3 days
        var threeDaysBefore = gameTime.AddDays(-3);
        if (threeDaysBefore > now)
        {
            _backgroundJobs.Schedule<ReminderService>(
                service => service.SendThreeDayReminderAsync(
                    message.BookingId,
                    message.UserEmail,
                    message.Username,
                    message.CampaignTitle,
                    message.StartTime
                ),
                threeDaysBefore
            );
            _logger.LogInformation("Scheduled 3-day reminder for {Time}", threeDaysBefore);
        }

        // reminder same day at 9:00 AM UTC
        var gameDay = new DateTime(
            gameTime.Year,
            gameTime.Month,
            gameTime.Day,
            9, 0, 0,
            DateTimeKind.Utc
        );

        if (gameDay > now)
        {
            _backgroundJobs.Schedule<ReminderService>(
                service => service.SendSameDayReminderAsync(
                    message.BookingId,
                    message.UserEmail,
                    message.Username,
                    message.CampaignTitle,
                    message.StartTime
                ),
                gameDay
            );
            _logger.LogInformation("Scheduled same-day reminder for {Time}", gameDay);
        }
    }
}