using System.ComponentModel.DataAnnotations;

namespace payment_service.Models
{
    public class Payment
    {
        [Key]
        public int MaTT { get; set; }

        public int MaThue { get; set; }

        public string HinhThucTT { get; set; }

        public decimal? ThanhTien { get; set; }

        public DateTime? NgayTT { get; set; }
    }
}