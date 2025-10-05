using DnDAgency.Application.Services;
using DnDAgency.Domain.Entities;
using DnDAgency.Infrastructure.Interfaces;
using Moq;
using Xunit;
using FluentAssertions;
using DnDAgency.Domain.Interfaces;

namespace Tests.Services;

public class BookingServiceAuthorizationTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IConflictCheckService> _conflictCheckMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly BookingService _sut;

    public BookingServiceAuthorizationTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _conflictCheckMock = new Mock<IConflictCheckService>();
        _cacheMock = new Mock<ICacheService>();
        _sut = new BookingService(_unitOfWorkMock.Object, _conflictCheckMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenUserTriesToCancelAnotherUsersBooking_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var bookingOwnerId = Guid.NewGuid();
        var maliciousUserId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddDays(1);

        var campaign = CreateTestCampaign(campaignId);
        var slot = CreateTestSlot(campaign, startTime);
        var booking = CreateTestBooking(bookingOwnerId, slot.Id, bookingId);

        SetPrivateProperty(booking, "Slot", slot);

        _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(default))
            .ReturnsAsync(transactionMock.Object);

        // Act
        var act = async () => await _sut.CancelBookingAsync(bookingId, maliciousUserId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Cannot cancel booking of another user");
    }

    [Fact]
    public async Task CancelBookingAsync_WhenCancellingLessThan2HoursBeforeStart_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(1); // Только 1 час до начала

        var campaign = CreateTestCampaign(campaignId);
        var slot = CreateTestSlot(campaign, startTime);
        var booking = CreateTestBooking(userId, slot.Id, bookingId);

        SetPrivateProperty(booking, "Slot", slot);

        _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(default))
            .ReturnsAsync(transactionMock.Object);

        // Act
        var act = async () => await _sut.CancelBookingAsync(bookingId, userId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Cannot cancel booking less than 2 hours before start");
    }

    [Fact]
    public async Task CancelBookingAsync_WhenSlotIsInPast_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddDays(-1); // Вчера

        var campaign = CreateTestCampaign(campaignId);
        var slot = CreateTestSlot(campaign, startTime);
        var booking = CreateTestBooking(userId, slot.Id, bookingId);

        SetPrivateProperty(booking, "Slot", slot);

        _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(default))
            .ReturnsAsync(transactionMock.Object);

        // Act
        var act = async () => await _sut.CancelBookingAsync(bookingId, userId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Cannot cancel booking for past slot");
    }

    // Helper methods
    private static Campaign CreateTestCampaign(Guid id)
    {
        var campaign = (Campaign)Activator.CreateInstance(typeof(Campaign), true);
        SetPrivateProperty(campaign, "Id", id);
        SetPrivateProperty(campaign, "DurationHours", 4.0);
        SetPrivateProperty(campaign, "MaxPlayers", 6);
        SetPrivateProperty(campaign, "WorkingHoursStart", TimeSpan.FromHours(10));
        SetPrivateProperty(campaign, "WorkingHoursEnd", TimeSpan.FromHours(22));
        return campaign;
    }

    private static Slot CreateTestSlot(Campaign campaign, DateTime startTime)
    {
        // Для слота в прошлом создаём через рефлексию
        var slot = (Slot)Activator.CreateInstance(typeof(Slot), true);
        SetPrivateProperty(slot, "Id", Guid.NewGuid());
        SetPrivateProperty(slot, "CampaignId", campaign.Id);
        SetPrivateProperty(slot, "StartTime", startTime);
        SetPrivateProperty(slot, "Campaign", campaign);
        return slot;
    }

    private static Booking CreateTestBooking(Guid userId, Guid slotId, Guid bookingId)
    {
        var booking = new Booking(userId, slotId, 1);
        SetPrivateProperty(booking, "Id", bookingId);
        return booking;
    }

    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        property?.SetValue(obj, value);
    }
}