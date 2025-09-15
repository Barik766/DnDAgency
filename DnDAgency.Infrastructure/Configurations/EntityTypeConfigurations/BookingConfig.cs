using DnDAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DnDAgency.Infrastructure.Configurations;

public class BookingConfig : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.CreatedAt).IsRequired();

        builder.HasOne(b => b.User)
               .WithMany(u => u.Bookings)
               .HasForeignKey(b => b.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Slot)
               .WithMany(s => s.Bookings)
               .HasForeignKey(b => b.SlotId)
               .OnDelete(DeleteBehavior.Cascade);

        // Уникальный индекс - один пользователь не может забронировать один слот дважды
        builder.HasIndex(b => new { b.UserId, b.SlotId }).IsUnique();

        // Индекс для быстрого поиска бронирований по дате
        builder.HasIndex(b => b.CreatedAt);
    }
}