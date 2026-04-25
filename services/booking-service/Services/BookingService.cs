using Microsoft.EntityFrameworkCore;
using booking_service.Data;
using booking_service.DTOs;
using booking_service.Models;
using booking_service.Services.Interfaces;
namespace booking_service.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BookingResponse>> GetAll()
        {
            return await _context.Bookings
                .Select(b => new BookingResponse
                {
                    MaThue = b.MaThue,
                    MaPhong = b.MaPhong,
                    NgayVao = b.NgayVao,
                    NgayTra = b.NgayTra,
                    TrangThai = b.TrangThai,
                    DatCoc = b.DatCoc
                })
                .ToListAsync();
        }

        public async Task<BookingResponse?> GetById(int id)
        {
            return await _context.Bookings
                .Where(b => b.MaThue == id)
                .Select(b => new BookingResponse
                {
                    MaThue = b.MaThue,
                    MaPhong = b.MaPhong,
                    NgayVao = b.NgayVao,
                    NgayTra = b.NgayTra,
                    TrangThai = b.TrangThai,
                    DatCoc = b.DatCoc
                })
                .FirstOrDefaultAsync();
        }
        public async Task<BookingResponse> Create(BookingRequest request, int userId)
        {
            bool isConflict = await _context.Bookings.AnyAsync(b =>
                b.MaPhong == request.MaPhong &&
                b.TrangThai != "Đã hủy" &&
                (
                    (request.NgayVao >= b.NgayVao && request.NgayVao < b.NgayTra) ||
                    (request.NgayTra > b.NgayVao && request.NgayTra <= b.NgayTra) ||
                    (request.NgayVao <= b.NgayVao && request.NgayTra >= b.NgayTra)
                )
            );

            if (isConflict)
            {
                throw new Exception("Phòng đã được đặt trong khoảng thời gian này.");
            }

            var booking = new Booking
            {
                MaPhong = request.MaPhong,
                MaNV = userId,
                NgayDat = DateTime.Now,
                NgayVao = request.NgayVao,
                NgayTra = request.NgayTra,
                DatCoc = request.DatCoc,
                TrangThai = "Đã đặt"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return new BookingResponse
            {
                MaThue = booking.MaThue,
                MaPhong = booking.MaPhong,
                NgayVao = booking.NgayVao,
                NgayTra = booking.NgayTra,
                TrangThai = booking.TrangThai,
                DatCoc = booking.DatCoc
            };
        }
    }
}