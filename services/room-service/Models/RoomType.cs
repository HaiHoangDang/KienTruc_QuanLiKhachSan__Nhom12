using System.ComponentModel.DataAnnotations;
namespace room_service.Models
{
    public class RoomType
    {
        [Key]
        public int MaLoai { get; set; }
        public string TenLoai { get; set; }
        public string GhiChu { get; set; }
    }
}