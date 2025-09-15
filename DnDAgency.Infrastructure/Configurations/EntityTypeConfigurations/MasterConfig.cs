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
            builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
            builder.Property(m => m.Bio).IsRequired().HasMaxLength(2000);
            builder.Property(m => m.UserId).IsRequired();

            // Связь один к одному с User
            builder.HasOne(m => m.User)
                   .WithOne(u => u.MasterProfile)
                   .HasForeignKey<Master>(m => m.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(m => m.Campaigns)
                   .WithOne(c => c.Master)
                   .HasForeignKey(c => c.MasterId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(m => m.Reviews)
                   .WithOne(r => r.Master)
                   .HasForeignKey(r => r.MasterId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Уникальный индекс на UserId
            builder.HasIndex(m => m.UserId).IsUnique();
        }
    }
}