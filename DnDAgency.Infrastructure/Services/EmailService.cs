namespace DnDAgency.Infrastructure.Services;

public class EmailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        // здесь будет интеграция с SMTP или SendGrid
        return Task.CompletedTask;
    }
}
