using System.ComponentModel.DataAnnotations;
namespace room_service.Models
{
    public class Room
    {
        [Key]
        public int MaPhong { get; set; }

        public string TenPhong { get; set; }
        public int MaKS { get; set; }
        public int MaLoai { get; set; }
        public int SucChua { get; set; }
        public int Tang { get; set; }
        public double? DienTich { get; set; }
        public decimal DGNgay { get; set; }
    }
}