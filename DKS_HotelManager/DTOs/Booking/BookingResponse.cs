using System;

namespace DKS_HotelManager.DTOs.Booking
{
    public class BookingResponse
    {
        public int MaThue { get; set; }
        public string MaDatPhong { get; set; }
        public int? MaNV { get; set; }
        public int? MaPhong { get; set; }
        public DateTime? NgayDat { get; set; }
        public DateTime? NgayVao { get; set; }
        public DateTime? NgayTra { get; set; }
        public decimal? DatCoc { get; set; }
        public string TrangThai { get; set; }
    }
}