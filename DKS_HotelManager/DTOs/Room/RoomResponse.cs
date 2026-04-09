namespace DKS_HotelManager.DTOs.Room
{
    public class RoomResponse
    {
        public int MaPhong { get; set; }
        public string TenPhong { get; set; }
        public int MaKS { get; set; }
        public int MaLoai { get; set; }
        public int SucChua { get; set; }
        public int Tang { get; set; }
        public double? DienTich { get; set; }
        public decimal DGNgay { get; set; }

        public string TenKS { get; set; }
        public string TenLoai { get; set; }
        public string FirstImagePath { get; set; }
    }
}