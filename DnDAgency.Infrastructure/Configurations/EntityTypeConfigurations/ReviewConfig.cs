using DnDAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DnDAgency.Infrastructure.Configurations;

public class ReviewConfig : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(1000);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasOne(r => r.User)
               .WithMany()
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Master)
               .WithMany(m => m.Reviews)
               .HasForeignKey(r => r.MasterId)
               .OnDelete(DeleteBehavior.Cascade);

        // Проверка рейтинга на уровне БД
        builder.HasCheckConstraint("CK_Review_Rating", "\"Rating\" >= 1 AND \"Rating\" <= 5");

        // Уникальный индекс - один пользователь не может оставить два отзыва одному мастеру
        builder.HasIndex(r => new { r.UserId, r.MasterId }).IsUnique();
    }
}