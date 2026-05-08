namespace booking_service.DTOs
{
    public class BookingResponse
    {
        public int MaThue { get; set; }
        public int MaPhong { get; set; }

        public DateTime? NgayVao { get; set; }
        public DateTime? NgayTra { get; set; }

        public string TrangThai { get; set; }
        public decimal? DatCoc { get; set; }
    }
}