using DnDAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DnDAgency.Infrastructure.Configurations;

public class SlotConfig : IEntityTypeConfiguration<Slot>
{
    public void Configure(EntityTypeBuilder<Slot> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.StartTime).IsRequired();

        builder.HasOne(s => s.Campaign)
               .WithMany(c => c.Slots)
               .HasForeignKey(s => s.CampaignId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Bookings)
               .WithOne(b => b.Slot)
               .HasForeignKey(b => b.SlotId)
               .OnDelete(DeleteBehavior.Cascade);

        // Уникальный индекс: одна кампания не может иметь два слота на одно время
        builder.HasIndex(s => new { s.CampaignId, s.StartTime })
               .IsUnique();

        // Индекс для быстрого поиска слотов по времени
        builder.HasIndex(s => s.StartTime);
    }
}