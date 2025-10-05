using DnDAgency.Application.Services;
using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Exceptions;
using DnDAgency.Infrastructure.Interfaces;
using Moq;
using Xunit;
using FluentAssertions;
using DnDAgency.Domain.Interfaces;

namespace Tests.Services;

public class BookingServiceCreateTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IConflictCheckService> _conflictCheckMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly BookingService _sut;

    public BookingServiceCreateTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _conflictCheckMock = new Mock<IConflictCheckService>();
        _cacheMock = new Mock<ICacheService>();
        _sut = new BookingService(_unitOfWorkMock.Object, _conflictCheckMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenCampaignNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddDays(1);

        var user = CreateTestUser(userId);

        _unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.Campaigns.GetByIdAsync(campaignId))
            .ReturnsAsync((Campaign)null);

        var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(default))
            .ReturnsAsync(transactionMock.Object);

        // Act
        var act = async () => await _sut.CreateBookingAsync(userId, campaignId, startTime, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Campaign not found");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(9)]
    [InlineData(100)]
    public async Task CreateBookingAsync_WhenInvalidPlayersCount_ThrowsArgumentException(int playersCount)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddDays(1);

        var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(default))
            .ReturnsAsync(transactionMock.Object);

        // Act
        var act = async () => await _sut.CreateBookingAsync(userId, campaignId, startTime, playersCount);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Players count must be between 1 and 8");
    }

    [Fact]
    public async Task CreateBookingAsync_WhenTimeSlotHasConflict_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddDays(1);

        var user = CreateTestUser(userId);
        var campaign = CreateTestCampaign(campaignId);

        _unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.Campaigns.GetByIdAsync(campaignId))
            .ReturnsAsync(campaign);

        _conflictCheckMock.Setup(x => x.HasConflictAsync(campaignId, startTime, It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);

        var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(default))
            .ReturnsAsync(transactionMock.Object);

        // Act
        var act = async () => await _sut.CreateBookingAsync(userId, campaignId, startTime, 1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Time slot conflicts with existing bookings");
    }

    [Fact]
    public async Task CreateBookingAsync_WhenSlotIsInPast_ThrowsPastSlotBookingException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddDays(-1); // Прошлое

        var user = CreateTestUser(userId);
        var campaign = CreateTestCampaign(campaignId);
        var existingSlot = CreateTestSlot(campaign, startTime);

        _unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.Campaigns.GetByIdAsync(campaignId))
            .ReturnsAsync(campaign);

        _conflictCheckMock.Setup(x => x.HasConflictAsync(campaignId, startTime, It.IsAny<TimeSpan>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Slots.GetByCampaignAndTimeAsync(campaignId, startTime))
            .ReturnsAsync(existingSlot);

        var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(default))
            .ReturnsAsync(transactionMock.Object);

        // Act
        var act = async () => await _sut.CreateBookingAsync(userId, campaignId, startTime, 1);

        // Assert
        await act.Should().ThrowAsync<PastSlotBookingException>();
    }

    [Fact]
    public async Task CreateBookingAsync_WhenUserAlreadyHasBookingForSlot_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddDays(1);

        var user = CreateTestUser(userId);
        var campaign = CreateTestCampaign(campaignId);
        var existingSlot = CreateTestSlot(campaign, startTime);
        var existingBooking = CreateTestBooking(userId, existingSlot.Id, Guid.NewGuid());

        var bookingsList = GetPrivateProperty<List<Booking>>(existingSlot, "Bookings");
        bookingsList.Add(existingBooking);

        _unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.Campaigns.GetByIdAsync(campaignId))
            .ReturnsAsync(campaign);

        _conflictCheckMock.Setup(x => x.HasConflictAsync(campaignId, startTime, It.IsAny<TimeSpan>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Slots.GetByCampaignAndTimeAsync(campaignId, startTime))
            .ReturnsAsync(existingSlot);

        var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(default))
            .ReturnsAsync(transactionMock.Object);

        // Act
        var act = async () => await _sut.CreateBookingAsync(userId, campaignId, startTime, 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("User already has booking for this time slot");
    }

    [Fact]
    public async Task CreateBookingAsync_WhenNotEnoughSlotsAvailable_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddDays(1);

        var user = CreateTestUser(userId);
        var campaign = CreateTestCampaign(campaignId);
        var existingSlot = CreateTestSlot(campaign, startTime);

        // Заполняем слот до максимума
        var bookingsList = GetPrivateProperty<List<Booking>>(existingSlot, "Bookings");
        for (int i = 0; i < 6; i++)
        {
            bookingsList.Add(CreateTestBooking(Guid.NewGuid(), existingSlot.Id, Guid.NewGuid()));
        }

        _unitOfWorkMock.Setup(x => x.Users.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.Campaigns.GetByIdAsync(campaignId))
            .ReturnsAsync(campaign);

        _conflictCheckMock.Setup(x => x.HasConflictAsync(campaignId, startTime, It.IsAny<TimeSpan>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Slots.GetByCampaignAndTimeAsync(campaignId, startTime))
            .ReturnsAsync(existingSlot);

        var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(default))
            .ReturnsAsync(transactionMock.Object);

        // Act
        var act = async () => await _sut.CreateBookingAsync(userId, campaignId, startTime, 2);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Not enough available slots*");
    }

    // Helper methods
    private static User CreateTestUser(Guid id)
    {
        var user = (User)Activator.CreateInstance(typeof(User), true);
        SetPrivateProperty(user, "Id", id);
        SetPrivateProperty(user, "Username", "testuser");
        SetPrivateProperty(user, "Email", "test@example.com");
        return user;
    }

    private static Campaign CreateTestCampaign(Guid id)
    {
        var campaign = (Campaign)Activator.CreateInstance(typeof(Campaign), true);
        SetPrivateProperty(campaign, "Id", id);
        SetPrivateProperty(campaign, "DurationHours", 4.0);
        SetPrivateProperty(campaign, "MaxPlayers", 6);
        SetPrivateProperty(campaign, "WorkingHoursStart", TimeSpan.FromHours(10));
        SetPrivateProperty(campaign, "WorkingHoursEnd", TimeSpan.FromHours(22));

        var slots = new List<Slot>();
        SetPrivateProperty(campaign, "Slots", slots);

        return campaign;
    }

    private static Slot CreateTestSlot(Campaign campaign, DateTime startTime)
    {
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

    private static T GetPrivateProperty<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (T)property?.GetValue(obj);
    }
}