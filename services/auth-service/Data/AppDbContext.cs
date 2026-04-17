using Microsoft.EntityFrameworkCore;
using auth_service.Models;

namespace auth_service.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<KhachHang> KhachHangs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KhachHang>().ToTable("KHACHHANG");

            modelBuilder.Entity<KhachHang>().HasKey(x => x.MKH);

            modelBuilder.Entity<KhachHang>().Property(x => x.TKH).HasColumnName("TKH");
            modelBuilder.Entity<KhachHang>().Property(x => x.TenDN).HasColumnName("TenDN");
            modelBuilder.Entity<KhachHang>().Property(x => x.MatKhau).HasColumnName("MatKhau");
            modelBuilder.Entity<KhachHang>().Property(x => x.Email).HasColumnName("Email");
        }
    }
}