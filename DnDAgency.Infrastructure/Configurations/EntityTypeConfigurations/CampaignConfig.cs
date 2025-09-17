using DnDAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DnDAgency.Infrastructure.Configurations;

public class CampaignConfig : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.Description)
               .HasMaxLength(1000);

        builder.Property(c => c.Price)
               .HasPrecision(18, 2);

        builder.Property(c => c.ImageUrl)
               .HasMaxLength(500);

        builder.Property(c => c.Level)
               .IsRequired();

        builder.Property(c => c.MaxPlayers)
               .IsRequired();

        builder.Property(c => c.DurationHours);

        builder.Property(c => c.IsActive)
               .IsRequired();

        // Связь многие-ко-многим с Master
        builder.HasMany(c => c.Masters)
               .WithMany(m => m.Campaigns)
               .UsingEntity<Dictionary<string, object>>(
                   "CampaignMaster",
                   j => j.HasOne<Master>()
                         .WithMany()
                         .HasForeignKey("MasterId")
                         .OnDelete(DeleteBehavior.Cascade),
                   j => j.HasOne<Campaign>()
                         .WithMany()
                         .HasForeignKey("CampaignId")
                         .OnDelete(DeleteBehavior.Cascade),
                   j =>
                   {
                       j.HasKey("CampaignId", "MasterId");
                   });

        // Связь с Slots
        builder.HasMany(c => c.Slots)
               .WithOne(s => s.Campaign)
               .HasForeignKey(s => s.CampaignId)
               .OnDelete(DeleteBehavior.Cascade);

        // Связь с Tags
        builder.HasMany(c => c.Tags)
               .WithOne(t => t.Campaign)
               .HasForeignKey(t => t.CampaignId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
