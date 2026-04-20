using Microsoft.EntityFrameworkCore;
using booking_service.Models;

namespace booking_service.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>()
                .ToTable("THUEPHONG")
                .HasKey(b => b.MaThue);
        }
    }
}