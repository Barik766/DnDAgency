using DnDAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DnDAgency.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Master> Masters => Set<Master>();
        public DbSet<Campaign> Campaigns => Set<Campaign>();
        public DbSet<Slot> Slots => Set<Slot>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<CampaignTag> CampaignTags => Set<CampaignTag>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
