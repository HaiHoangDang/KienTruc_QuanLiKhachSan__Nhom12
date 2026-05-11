using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace booking_service.Models
{
    [Table("PHONG")]
    public class PHONG
    {
        [Key]
        public int MaPhong { get; set; }

        public decimal DGNgay { get; set; }

        public string? TenPhong { get; set; }
    }
}