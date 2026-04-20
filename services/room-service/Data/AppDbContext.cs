using Microsoft.EntityFrameworkCore;
using room_service.Models;

namespace room_service.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<RoomStatus> RoomStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Room>().ToTable("PHONG");
            modelBuilder.Entity<RoomType>().ToTable("LOAIPHONG");
            modelBuilder.Entity<RoomStatus>().ToTable("TRANGTHAI_PHONG");

            modelBuilder.Entity<Room>().HasKey(r => r.MaPhong);
            modelBuilder.Entity<RoomType>().HasKey(r => r.MaLoai);
            modelBuilder.Entity<RoomStatus>().HasKey(r => r.MaTrang);
        }
    }
}