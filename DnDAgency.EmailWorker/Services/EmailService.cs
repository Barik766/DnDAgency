using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DnDAgency.EmailWorker.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendBookingConfirmationAsync(
        string toEmail,
        string username,
        string campaignTitle,
        DateTime startTime,
        int playersCount)
    {
        var subject = "✅ Booking Confirmation - DnD Agency";

        var body = $@"
            <h2>Hello, {username}!</h2>
            <p>Your booking has been confirmed! 🎲</p>
            
            <h3>Booking Details:</h3>
            <ul>
                <li><strong>Campaign:</strong> {campaignTitle}</li>
                <li><strong>Date & Time:</strong> {startTime:yyyy-MM-dd HH:mm} UTC</li>
                <li><strong>Players:</strong> {playersCount}</li>
            </ul>
            
            <p>We look forward to seeing you at the game!</p>
            <p><em>DnD Agency Team</em></p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendReminderAsync(
        string toEmail,
        string username,
        string campaignTitle,
        DateTime startTime,
        string reminderType)
    {
        var subject = $"⏰ Reminder: {reminderType} - DnD Agency";

        var body = $@"
            <h2>Hello, {username}!</h2>
            <p>This is a reminder about your upcoming game! 🎲</p>
            
            <h3>Game Details:</h3>
            <ul>
                <li><strong>Campaign:</strong> {campaignTitle}</li>
                <li><strong>Date & Time:</strong> {startTime:yyyy-MM-dd HH:mm} UTC</li>
            </ul>
            
            <p>See you soon!</p>
            <p><em>DnD Agency Team</em></p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }


    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _configuration["Email:SenderName"],
                _configuration["Email:SenderEmail"]
            ));

            message.To.Add(MailboxAddress.Parse(toEmail));

            message.Subject = subject;

            message.Body = new TextPart("html") { Text = htmlBody };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _configuration["Email:SmtpHost"],
                int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _configuration["Email:SenderEmail"],
                _configuration["Email:Password"]
            );

            await smtp.SendAsync(message);

            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }
}