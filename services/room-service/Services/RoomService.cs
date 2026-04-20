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
            return await (from r in _context.Rooms
                          join l in _context.RoomTypes on r.MaLoai equals l.MaLoai
                          let latestStatus = _context.RoomStatuses
                                .Where(s => s.MaPhong == r.MaPhong)
                                .OrderByDescending(s => s.NgayCapNhat)
                                .FirstOrDefault()
                          select new RoomResponse
                          {
                              MaPhong = r.MaPhong,
                              TenPhong = r.TenPhong,
                              SucChua = r.SucChua,
                              DGNgay = r.DGNgay,
                              LoaiPhong = l.TenLoai,
                              TrangThai = latestStatus != null ? latestStatus.TrangThai : "Chưa cập nhật"
                          }).ToListAsync();
        }

        public async Task<RoomResponse?> GetById(int id)
        {
            return await (from r in _context.Rooms
                          join l in _context.RoomTypes on r.MaLoai equals l.MaLoai
                          let latestStatus = _context.RoomStatuses
                                .Where(s => s.MaPhong == r.MaPhong)
                                .OrderByDescending(s => s.NgayCapNhat)
                                .FirstOrDefault()
                          where r.MaPhong == id
                          select new RoomResponse
                          {
                              MaPhong = r.MaPhong,
                              TenPhong = r.TenPhong,
                              SucChua = r.SucChua,
                              DGNgay = r.DGNgay,
                              LoaiPhong = l.TenLoai,
                              TrangThai = latestStatus != null ? latestStatus.TrangThai : "Chưa cập nhật"
                          }).FirstOrDefaultAsync();
        }
    }
}