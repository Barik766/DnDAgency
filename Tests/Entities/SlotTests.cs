using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Tests.Entities;

public class SlotTests
{
    [Fact]
    public void Constructor_WhenStartTimeIsInPast_ThrowsPastSlotBookingException()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var pastTime = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = () => new Slot(campaignId, pastTime);

        // Assert
        act.Should().Throw<PastSlotBookingException>();
    }

    [Fact]
    public void Constructor_WhenStartTimeIsValid_CreatesSlot()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var futureTime = DateTime.UtcNow.AddDays(1);

        // Act
        var slot = new Slot(campaignId, futureTime);

        // Assert
        slot.Should().NotBeNull();
        slot.CampaignId.Should().Be(campaignId);
        slot.StartTime.Should().Be(futureTime);
        slot.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void IsInPast_WhenSlotIsInPast_ReturnsTrue()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var pastTime = DateTime.UtcNow.AddDays(-1);

        var slot = (Slot)Activator.CreateInstance(typeof(Slot), true);
        SetPrivateProperty(slot, "CampaignId", campaignId);
        SetPrivateProperty(slot, "StartTime", pastTime);

        // Act & Assert
        slot.IsInPast.Should().BeTrue();
    }

    [Fact]
    public void IsInPast_WhenSlotIsInFuture_ReturnsFalse()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var futureTime = DateTime.UtcNow.AddDays(1);
        var slot = new Slot(campaignId, futureTime);

        // Act & Assert
        slot.IsInPast.Should().BeFalse();
    }

    [Fact]
    public void IsFull_WhenNoBookings_ReturnsFalse()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var futureTime = DateTime.UtcNow.AddDays(1);
        var slot = new Slot(campaignId, futureTime);

        var campaign = CreateTestCampaign(campaignId, maxPlayers: 6);
        SetPrivateProperty(slot, "Campaign", campaign);

        // Act & Assert
        slot.IsFull.Should().BeFalse();
        slot.AvailableSlots.Should().Be(6);
    }

    [Fact]
    public void IsFull_WhenMaxPlayersReached_ReturnsTrue()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var futureTime = DateTime.UtcNow.AddDays(1);
        var slot = new Slot(campaignId, futureTime);

        var campaign = CreateTestCampaign(campaignId, maxPlayers: 6);
        SetPrivateProperty(slot, "Campaign", campaign);

        // Добавляем бронирования до максимума
        var bookings = GetPrivateProperty<List<Booking>>(slot, "Bookings");
        for (int i = 0; i < 6; i++)
        {
            bookings.Add(new Booking(Guid.NewGuid(), slot.Id, 1));
        }

        // Act & Assert
        slot.IsFull.Should().BeTrue();
        slot.AvailableSlots.Should().Be(0);
        slot.CurrentPlayers.Should().Be(6);
    }

    [Fact]
    public void CanBeBooked_WhenSlotIsInPast_ReturnsFalse()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var pastTime = DateTime.UtcNow.AddDays(-1);

        var slot = (Slot)Activator.CreateInstance(typeof(Slot), true);
        SetPrivateProperty(slot, "CampaignId", campaignId);
        SetPrivateProperty(slot, "StartTime", pastTime);

        // Act & Assert
        slot.CanBeBooked(6).Should().BeFalse();
    }

    [Fact]
    public void CanBeBooked_WhenSlotIsFull_ReturnsFalse()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var futureTime = DateTime.UtcNow.AddDays(1);
        var slot = new Slot(campaignId, futureTime);

        var bookings = GetPrivateProperty<List<Booking>>(slot, "Bookings");
        for (int i = 0; i < 6; i++)
        {
            bookings.Add(new Booking(Guid.NewGuid(), slot.Id, 1));
        }

        // Act & Assert
        slot.CanBeBooked(6).Should().BeFalse();
    }

    [Fact]
    public void CanBeBooked_WhenSlotIsAvailable_ReturnsTrue()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var futureTime = DateTime.UtcNow.AddDays(1);
        var slot = new Slot(campaignId, futureTime);

        // Act & Assert
        slot.CanBeBooked(6).Should().BeTrue();
    }

    [Fact]
    public void CurrentPlayers_CalculatesCorrectly()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var futureTime = DateTime.UtcNow.AddDays(1);
        var slot = new Slot(campaignId, futureTime);

        var bookings = GetPrivateProperty<List<Booking>>(slot, "Bookings");
        bookings.Add(new Booking(Guid.NewGuid(), slot.Id, 2)); // 2 игрока
        bookings.Add(new Booking(Guid.NewGuid(), slot.Id, 3)); // 3 игрока

        // Act & Assert
        slot.CurrentPlayers.Should().Be(5);
    }

    // Helper methods
    private static Campaign CreateTestCampaign(Guid id, int maxPlayers = 6)
    {
        var campaign = (Campaign)Activator.CreateInstance(typeof(Campaign), true);
        SetPrivateProperty(campaign, "Id", id);
        SetPrivateProperty(campaign, "MaxPlayers", maxPlayers);
        SetPrivateProperty(campaign, "DurationHours", 4.0);
        SetPrivateProperty(campaign, "WorkingHoursStart", TimeSpan.FromHours(10));
        SetPrivateProperty(campaign, "WorkingHoursEnd", TimeSpan.FromHours(22));
        return campaign;
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