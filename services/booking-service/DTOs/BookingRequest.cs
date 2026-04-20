namespace booking_service.DTOs
{
    public class BookingRequest
    {
        public int MaPhong { get; set; }
        public int MaNV { get; set; }

        public DateTime NgayVao { get; set; }
        public DateTime NgayTra { get; set; }

        public decimal DatCoc { get; set; }
    }
}