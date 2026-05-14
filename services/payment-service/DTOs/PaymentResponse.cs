namespace payment_service.DTOs
{
    public class PaymentResponse
    {
        public int MaTT { get; set; }

        public int MaThue { get; set; }

        public string HinhThucTT { get; set; }

        public decimal? ThanhTien { get; set; }

        public DateTime? NgayTT { get; set; }
    }
}