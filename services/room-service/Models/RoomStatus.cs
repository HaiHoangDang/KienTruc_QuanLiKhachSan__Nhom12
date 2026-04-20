using System.ComponentModel.DataAnnotations;
namespace room_service.Models
{
    public class RoomStatus
    {
        [Key]
        public int MaTrang { get; set; }
        public int MaPhong { get; set; }
        public string TrangThai { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public int? MaNVCapNhat { get; set; }
        public string GhiChu { get; set; }
    }
}