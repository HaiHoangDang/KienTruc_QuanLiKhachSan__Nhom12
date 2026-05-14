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

        public DbSet<BookingDetail> BookingDetails { get; set; }

        public DbSet<PHONG> PHONGs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>()
                .ToTable("THUEPHONG")
                .HasKey(b => b.MaThue);

            modelBuilder.Entity<Booking>()
                .Property(b => b.MaThue)
                .ValueGeneratedNever();

            modelBuilder.Entity<Booking>()
                .Property(b => b.DatCoc)
                .HasColumnType("money");

            modelBuilder.Entity<BookingDetail>()
                .ToTable("CTTHUEPHONG")
                .HasKey(x => new { x.MaThue, x.KHACH });

            modelBuilder.Entity<PHONG>()
                .ToTable("PHONG")
                .HasKey(p => p.MaPhong);

            modelBuilder.Entity<PHONG>()
                .Property(p => p.DGNgay)
                .HasColumnType("money");
        }
    }
}
//using Microsoft.EntityFrameworkCore;
//using booking_service.Models;

//namespace booking_service.Data
//{
//    public class AppDbContext : DbContext
//    {
//        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
//        {
//        }

//        public DbSet<Booking> Bookings { get; set; }
//        public DbSet<PHONG> PHONGs { get; set; }
//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            modelBuilder.Entity<Booking>()
//                .ToTable("THUEPHONG")
//                .HasKey(b => b.MaThue);
//        }
//    }
//}