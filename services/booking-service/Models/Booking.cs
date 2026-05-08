using System.ComponentModel.DataAnnotations;

namespace booking_service.Models
{
    public class Booking
    {
        [Key]
        public int MaThue { get; set; }

        public int MaNV { get; set; }
        public int MaPhong { get; set; }

        public DateTime? NgayDat { get; set; }
        public DateTime? NgayVao { get; set; }
        public DateTime? NgayTra { get; set; }

        public decimal? DatCoc { get; set; }

        public string? MaDatPhong { get; set; }
        public string? TrangThai { get; set; }
    }
}