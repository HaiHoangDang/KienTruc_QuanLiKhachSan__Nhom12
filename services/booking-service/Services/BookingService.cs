//using MaDatPhongicrosoft.EntityFrameworkCore;
//using booking_service.Data;
//using booking_service.DTOs;
//using booking_service.Models;
//using booking_service.Services.Interfaces;
//namespace booking_service.Services
//{
//    public class BookingService : IBookingService
//    {
//        private readonly AppDbContext _context;

//        public BookingService(AppDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<List<BookingResponse>> GetAll()
//        {
//            return await _context.Bookings
//                .Select(b => new BookingResponse
//                {
//                    MaThue = b.MaThue,
//                    MaPhong = b.MaPhong,
//                    NgayVao = b.NgayVao,
//                    NgayTra = b.NgayTra,
//                    TrangThai = b.TrangThai,
//                    DatCoc = b.DatCoc
//                })
//                .ToListAsync();
//        }

//        public async Task<BookingResponse?> GetById(int id)
//        {
//            return await _context.Bookings
//                .Where(b => b.MaThue == id)
//                .Select(b => new BookingResponse
//                {
//                    MaThue = b.MaThue,
//                    MaPhong = b.MaPhong,
//                    NgayVao = b.NgayVao,
//                    NgayTra = b.NgayTra,
//                    TrangThai = b.TrangThai,
//                    DatCoc = b.DatCoc
//                })
//                .FirstOrDefaultAsync();
//        }
//        public async Task<BookingResponse> Create(BookingRequest request, int userId)
//        {
//            // validate ngày
//            if (request.NgayTra <= request.NgayVao)
//            {
//                throw new Exception("Ngày trả phải sau ngày vào.");
//            }

//            if (request.NgayVao.Date < DateTime.Now.Date)
//            {
//                throw new Exception("Không thể đặt phòng trong quá khứ.");
//            }

//            // kiểm tra trùng lịch
//            bool isConflict = await _context.Bookings.AnyAsync(b =>
//                b.MaPhong == request.MaPhong &&
//                b.TrangThai != "Đã hủy" &&
//                b.NgayVao.HasValue &&
//                b.NgayTra.HasValue &&

//                request.NgayVao < b.NgayTra &&
//                request.NgayTra > b.NgayVao
//            );

//            if (isConflict)
//            {
//                throw new Exception("Phòng đã được đặt trong khoảng thời gian này.");
//            }

//            // lấy thông tin phòng để tính giá
//            var room = await _context.PHONGs
//                .FirstOrDefaultAsync(p => p.MaPhong == request.MaPhong);

//            if (room == null)
//            {
//                throw new Exception("Không tìm thấy phòng.");
//            }

//            // số đêm
//            int totalDays = (request.NgayTra - request.NgayVao).Days;

//            if (totalDays <= 0)
//            {
//                totalDays = 1;
//            }

//            // tổng tiền
//            decimal totalPrice = room.DGNgay * totalDays;

//            // đặt cọc 30%
//            decimal datCoc = totalPrice * 0.3m;

//            var booking = new Booking
//            {
//                MaPhong = request.MaPhong,
//                MaKH = userId,
//                NgayDat = DateTime.Now,
//                NgayVao = request.NgayVao,
//                NgayTra = request.NgayTra,
//                DatCoc = datCoc,
//                TrangThai = "Đã đặt"
//            };

//            _context.Bookings.Add(booking);

//            await _context.SaveChangesAsync();

//            return new BookingResponse
//            {
//                MaThue = booking.MaThue,
//                MaPhong = booking.MaPhong,
//                NgayVao = booking.NgayVao,
//                NgayTra = booking.NgayTra,
//                TrangThai = booking.TrangThai,
//                DatCoc = booking.DatCoc
//            };
//        }
//    }
//}
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
                .OrderByDescending(b => b.NgayDat)
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

        public async Task<BookingResponse> Create(BookingRequest request, int customerId)
        {
            if (request.NgayTra <= request.NgayVao)
            {
                throw new Exception("Ngày trả phải sau ngày vào.");
            }

            if (request.NgayVao.Date < DateTime.Now.Date)
            {
                throw new Exception("Không thể đặt phòng trong quá khứ.");
            }

            var room = await _context.PHONGs
                .FirstOrDefaultAsync(p => p.MaPhong == request.MaPhong);

            if (room == null)
            {
                throw new Exception("Không tìm thấy phòng.");
            }

            bool isConflict = await _context.Bookings.AnyAsync(b =>
                b.MaPhong == request.MaPhong &&
                b.TrangThai != "Đã hủy" &&
                b.NgayVao.HasValue &&
                b.NgayTra.HasValue &&
                request.NgayVao < b.NgayTra &&
                request.NgayTra > b.NgayVao
            );

            if (isConflict)
            {
                throw new Exception("Phòng đã được đặt trong khoảng thời gian này.");
            }

            int newMaThue = await _context.Bookings.AnyAsync()
                ? await _context.Bookings.MaxAsync(b => b.MaThue) + 1
                : 1;

            int totalDays = (request.NgayTra - request.NgayVao).Days;

            if (totalDays <= 0)
            {
                totalDays = 1;
            }

            decimal totalPrice = room.DGNgay * totalDays;

            decimal datCoc = totalPrice * 0.3m;

            var booking = new Booking
            {
                MaThue = newMaThue,
                MaNV = request.MaNV,
                MaPhong = request.MaPhong,
                NgayDat = DateTime.Now,
                NgayVao = request.NgayVao,
                NgayTra = request.NgayTra,
                DatCoc = datCoc,
                //MaDatPhong = "DP" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                MaDatPhong = "DP" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                TrangThai = "Đã đặt"
            };

            _context.Bookings.Add(booking);

            //var detail = new BookingDetail
            //{
            //    MaThue = newMaThue,
            //    KHACH = customerId,
            //    VaiTro = "Người đặt"
            //};

            //_context.BookingDetails.Add(detail);

            //await _context.SaveChangesAsync();
            var customerExists = await _context.Database
                .SqlQueryRaw<int>($"SELECT COUNT(*) AS Value FROM KHACHHANG WHERE MKH = {customerId}")
                .FirstAsync();

            if (customerExists <= 0)
            {
                throw new Exception($"Không tìm thấy khách hàng có MKH = {customerId} trong bảng KHACHHANG.");
            }

            var detail = new BookingDetail
            {
                MaThue = newMaThue,
                KHACH = customerId,
                VaiTro = "Người đặt"
            };

            _context.BookingDetails.Add(detail);

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