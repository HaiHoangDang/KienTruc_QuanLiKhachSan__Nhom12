using System;

namespace DKS_HotelManager.DTOs.Booking
{
    public class CreateBookingRequest
    {
        public int MaNV { get; set; }
        public int MaPhong { get; set; }
        public DateTime NgayVao { get; set; }
        public DateTime NgayTra { get; set; }
        public decimal DatCoc { get; set; }
    }
}