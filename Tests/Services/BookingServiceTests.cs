using DnDAgency.Application.Services;
using DnDAgency.Domain.Entities;
using DnDAgency.Infrastructure.Interfaces;
using Moq;
using Xunit;
using FluentAssertions;
using DnDAgency.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DnDAgency.Tests.Services;

public class BookingServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IConflictCheckService> _conflictCheckMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _conflictCheckMock = new Mock<IConflictCheckService>();
        _cacheMock = new Mock<ICacheService>();
        _sut = new BookingService(_unitOfWorkMock.Object, _conflictCheckMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenUserNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddDays(1);

        _unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId))
            .ReturnsAsync((User)null);

        // Act
        var act = async () => await _sut.CreateBookingAsync(userId, campaignId, startTime, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task CancelBookingAsync_WhenMultipleBookingsExist_ShouldNotDeleteSlot()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddDays(1);

        var campaign = CreateTestCampaign(campaignId);
        var slot = CreateTestSlot(campaign, startTime);
        var booking1 = CreateTestBooking(userId, slot.Id, bookingId);
        var booking2 = CreateTestBooking(Guid.NewGuid(), slot.Id, Guid.NewGuid());

        // Связываем сущности
        SetPrivateProperty(booking1, "Slot", slot);
        var bookingsList = GetPrivateProperty<List<Booking>>(slot, "Bookings");
        bookingsList.Add(booking1);
        bookingsList.Add(booking2);

        _unitOfWorkMock.Setup(x => x.Bookings.GetByIdAsync(bookingId))
            .ReturnsAsync(booking1);

        var transactionMock = new Mock<IDbContextTransaction>();
        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(default))
            .ReturnsAsync(transactionMock.Object);

        // Act
        await _sut.CancelBookingAsync(bookingId, userId);

        // Assert
        _unitOfWorkMock.Verify(x => x.Slots.Delete(It.IsAny<Slot>()), Times.Never,
            "Slot should NOT be deleted when multiple bookings exist");
        _unitOfWorkMock.Verify(x => x.Bookings.Delete(booking1), Times.Once);
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
        var slot = new Slot(campaign.Id, startTime);
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

    private static T GetPrivateProperty<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (T)property?.GetValue(obj);
    }
}