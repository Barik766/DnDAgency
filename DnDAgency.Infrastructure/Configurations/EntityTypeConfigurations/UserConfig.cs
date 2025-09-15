using DnDAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DnDAgency.Infrastructure.Configurations
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
            builder.Property(u => u.PasswordHash).IsRequired();

            // Уникальные индексы
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.Username).IsUnique();

            builder.HasMany(u => u.Bookings)
                   .WithOne(b => b.User)
                   .HasForeignKey(b => b.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Связь один к одному с Master
            builder.HasOne(u => u.MasterProfile)
                   .WithOne(m => m.User)
                   .HasForeignKey<Master>(m => m.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(u => u.Role)
                   .HasConversion<string>()
                   .HasMaxLength(20);
        }
    }
}