using Microsoft.Extensions.Logging;

namespace DnDAgency.EmailWorker.Services;

public class ReminderService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(IEmailService emailService, ILogger<ReminderService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendMonthReminderAsync(
        Guid bookingId,
        string email,
        string username,
        string campaignTitle,
        DateTime startTime)
    {
        _logger.LogInformation(
            "Sending 30-day reminder for booking {BookingId} to {Email}",
            bookingId,
            email
        );

        await _emailService.SendReminderAsync(
            email,
            username,
            campaignTitle,
            startTime,
            "Your game is in 30 days"
        );
    }

    public async Task SendThreeDayReminderAsync(
        Guid bookingId,
        string email,
        string username,
        string campaignTitle,
        DateTime startTime)
    {
        _logger.LogInformation(
            "Sending 3-day reminder for booking {BookingId} to {Email}",
            bookingId,
            email
        );

        await _emailService.SendReminderAsync(
            email,
            username,
            campaignTitle,
            startTime,
            "Your game is in 3 days"
        );
    }
    public async Task SendSameDayReminderAsync(
        Guid bookingId,
        string email,
        string username,
        string campaignTitle,
        DateTime startTime)
    {
        _logger.LogInformation(
            "Sending same-day reminder for booking {BookingId} to {Email}",
            bookingId,
            email
        );

        await _emailService.SendReminderAsync(
            email,
            username,
            campaignTitle,
            startTime,
            "Your game is TODAY"
        );
    }
}