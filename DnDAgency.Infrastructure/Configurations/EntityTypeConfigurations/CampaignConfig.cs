using DnDAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DnDAgency.Infrastructure.Configurations;

public class CampaignConfig : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.Property(c => c.Price).HasPrecision(18, 2);

        builder.HasMany(c => c.Slots)
               .WithOne(s => s.Campaign)
               .HasForeignKey(s => s.CampaignId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
