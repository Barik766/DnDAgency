using DnDAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DnDAgency.Infrastructure.Configurations
{
    public class MasterConfig : IEntityTypeConfiguration<Master>
    {
        public void Configure(EntityTypeBuilder<Master> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(m => m.Bio)
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(m => m.UserId)
                   .IsRequired();

            // Связь один к одному с User
            builder.HasOne(m => m.User)
                   .WithOne(u => u.MasterProfile)
                   .HasForeignKey<Master>(m => m.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Связь многие-ко-многим с Campaign
            builder.HasMany(m => m.Campaigns)
                   .WithMany(c => c.Masters)
                   .UsingEntity<Dictionary<string, object>>(
                       "CampaignMaster",
                       j => j.HasOne<Campaign>()
                             .WithMany()
                             .HasForeignKey("CampaignId")
                             .OnDelete(DeleteBehavior.Cascade),
                       j => j.HasOne<Master>()
                             .WithMany()
                             .HasForeignKey("MasterId")
                             .OnDelete(DeleteBehavior.Cascade),
                       j =>
                       {
                           j.HasKey("CampaignId", "MasterId");
                       });

            builder.HasMany(m => m.Reviews)
                   .WithOne(r => r.Master)
                   .HasForeignKey(r => r.MasterId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Уникальный индекс на UserId
            builder.HasIndex(m => m.UserId).IsUnique();
        }
    }
}
