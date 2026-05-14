using Microsoft.EntityFrameworkCore;
using payment_service.Models;

namespace payment_service.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>()
                .ToTable("THANHTOAN")
                .HasKey(x => x.MaTT);

            modelBuilder.Entity<Payment>()
                .Property(x => x.MaTT)
                .ValueGeneratedNever();

            modelBuilder.Entity<Payment>()
                .Property(x => x.HinhThucTT)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Payment>()
                .Property(x => x.ThanhTien)
                .HasColumnType("money");

            modelBuilder.Entity<Payment>()
                .Property(x => x.NgayTT)
                .HasColumnType("date");

            modelBuilder.Entity<Booking>()
                .ToTable("THUEPHONG")
                .HasKey(x => x.MaThue);

            modelBuilder.Entity<Booking>()
                .Property(x => x.MaThue)
                .ValueGeneratedNever();

            modelBuilder.Entity<Booking>()
                .Property(x => x.DatCoc)
                .HasColumnType("money");

            modelBuilder.Entity<Booking>()
                .Property(x => x.NgayDat)
                .HasColumnType("date");

            modelBuilder.Entity<Booking>()
                .Property(x => x.NgayVao)
                .HasColumnType("date");

            modelBuilder.Entity<Booking>()
                .Property(x => x.NgayTra)
                .HasColumnType("date");
        }
    }
}