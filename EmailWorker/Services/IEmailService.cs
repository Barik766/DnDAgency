namespace DnDAgency.EmailWorker.Services;

public interface IEmailService
{

    Task SendBookingConfirmationAsync(string toEmail, string username, string campaignTitle, DateTime startTime, int playersCount);
    Task SendReminderAsync(string toEmail, string username, string campaignTitle, DateTime startTime, string reminderType);
}