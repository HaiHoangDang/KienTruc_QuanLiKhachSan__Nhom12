using Microsoft.EntityFrameworkCore;
using room_service.Data;
using room_service.DTOs;
using room_service.Services.Interfaces;

namespace room_service.Services
{
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;

        public RoomService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoomResponse>> GetRooms()
        {
            var rooms = await (
                from p in _context.Rooms
                join l in _context.RoomTypes on p.MaLoai equals l.MaLoai
                join tt in _context.RoomStatuses on p.MaPhong equals tt.MaPhong into statusGroup
                from status in statusGroup
                    .OrderByDescending(x => x.NgayCapNhat)
                    .Take(1)
                    .DefaultIfEmpty()
                orderby p.MaPhong
                select new RoomResponse
                {
                    MaPhong = p.MaPhong,
                    TenPhong = p.TenPhong,
                    SucChua = p.SucChua,
                    DGNgay = p.DGNgay,
                    LoaiPhong = l.TenLoai,
                    TrangThai = status != null ? status.TrangThai : "Chưa cập nhật"
                }
            ).ToListAsync();

            return rooms;
        }

        public async Task<RoomResponse?> GetById(int id)
        {
            var room = await (
                from p in _context.Rooms
                join l in _context.RoomTypes on p.MaLoai equals l.MaLoai
                join tt in _context.RoomStatuses on p.MaPhong equals tt.MaPhong into statusGroup
                from status in statusGroup
                    .OrderByDescending(x => x.NgayCapNhat)
                    .Take(1)
                    .DefaultIfEmpty()
                where p.MaPhong == id
                select new RoomResponse
                {
                    MaPhong = p.MaPhong,
                    TenPhong = p.TenPhong,
                    SucChua = p.SucChua,
                    DGNgay = p.DGNgay,
                    LoaiPhong = l.TenLoai,
                    TrangThai = status != null ? status.TrangThai : "Chưa cập nhật"
                }
            ).FirstOrDefaultAsync();

            return room;
        }
    }
}