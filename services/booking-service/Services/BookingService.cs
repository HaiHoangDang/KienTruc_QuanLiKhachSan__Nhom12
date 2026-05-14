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
using Microsoft.Data.SqlClient;

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

        //public async Task<BookingResponse> Create(BookingRequest request, int customerId)
        //{
        //    if (request.NgayTra <= request.NgayVao)
        //    {
        //        throw new Exception("Ngày trả phải sau ngày vào.");
        //    }

        //    if (request.NgayVao.Date < DateTime.Now.Date)
        //    {
        //        throw new Exception("Không thể đặt phòng trong quá khứ.");
        //    }

        //    var room = await _context.PHONGs
        //        .FirstOrDefaultAsync(p => p.MaPhong == request.MaPhong);

        //    if (room == null)
        //    {
        //        throw new Exception("Không tìm thấy phòng.");
        //    }

        //    bool isConflict = await _context.Bookings.AnyAsync(b =>
        //        b.MaPhong == request.MaPhong &&
        //        b.TrangThai != "Đã hủy" &&
        //        b.NgayVao.HasValue &&
        //        b.NgayTra.HasValue &&
        //        request.NgayVao < b.NgayTra &&
        //        request.NgayTra > b.NgayVao
        //    );

        //    if (isConflict)
        //    {
        //        throw new Exception("Phòng đã được đặt trong khoảng thời gian này.");
        //    }

        //    int newMaThue = await _context.Bookings.AnyAsync()
        //        ? await _context.Bookings.MaxAsync(b => b.MaThue) + 1
        //        : 1;

        //    int totalDays = (request.NgayTra - request.NgayVao).Days;

        //    if (totalDays <= 0)
        //    {
        //        totalDays = 1;
        //    }

        //    decimal totalPrice = room.DGNgay * totalDays;
        //    decimal datCoc = totalPrice * 0.3m;

        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        var booking = new Booking
        //        {
        //            MaThue = newMaThue,
        //            MaNV = request.MaNV,
        //            MaPhong = request.MaPhong,
        //            NgayDat = DateTime.Now,
        //            NgayVao = request.NgayVao,
        //            NgayTra = request.NgayTra,
        //            DatCoc = datCoc,
        //            MaDatPhong = "DP" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
        //            TrangThai = "Đã đặt"
        //        };

        //        _context.Bookings.Add(booking);
        //        await _context.SaveChangesAsync();

        //        var detail = new BookingDetail
        //        {
        //            MaThue = booking.MaThue,
        //            KHACH = customerId,
        //            VaiTro = "Người đặt"
        //        };

        //        _context.BookingDetails.Add(detail);
        //        await _context.SaveChangesAsync();

        //        await transaction.CommitAsync();

        //        return new BookingResponse
        //        {
        //            MaThue = booking.MaThue,
        //            MaPhong = booking.MaPhong,
        //            NgayVao = booking.NgayVao,
        //            NgayTra = booking.NgayTra,
        //            TrangThai = booking.TrangThai,
        //            DatCoc = booking.DatCoc
        //        };
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}
        public async Task<BookingResponse> Create(BookingRequest request, int customerId)
        {
            Console.WriteLine("=== ĐANG CHẠY CREATE BOOKING BẢN SQL TRỰC TIẾP ===");
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

            int customerExists = await _context.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM KHACHHANG WHERE MKH = {0}", customerId)
                .FirstAsync();

            if (customerExists <= 0)
            {
                throw new Exception($"Không tìm thấy khách hàng MKH = {customerId} trong bảng KHACHHANG.");
            }

            int employeeExists = await _context.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM NHANVIEN WHERE MaNV = {0}", request.MaNV)
                .FirstAsync();

            if (employeeExists <= 0)
            {
                throw new Exception($"Không tìm thấy nhân viên MaNV = {request.MaNV} trong bảng NHANVIEN.");
            }

            int newMaThue = await _context.Database
                .SqlQueryRaw<int>("SELECT ISNULL(MAX(MaThue), 0) + 1 AS Value FROM THUEPHONG")
                .FirstAsync();

            int totalDays = (request.NgayTra - request.NgayVao).Days;

            if (totalDays <= 0)
            {
                totalDays = 1;
            }

            decimal totalPrice = room.DGNgay * totalDays;
            decimal datCoc = totalPrice * 0.3m;
            string maDatPhong = "DP" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string trangThai = "Đã đặt";
            string vaiTro = "Người đặt";

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                        INSERT INTO THUEPHONG
                        (
                            MaThue,
                            MaNV,
                            MaPhong,
                            NgayDat,
                            NgayVao,
                            NgayTra,
                            DatCoc,
                            MaDatPhong,
                            TrangThai
                        )
                        VALUES
                        (
                            {newMaThue},
                            {request.MaNV},
                            {request.MaPhong},
                            {DateTime.Now},
                            {request.NgayVao},
                            {request.NgayTra},
                            {datCoc},
                            {maDatPhong},
                            {trangThai}
                        )
                    ");
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                            INSERT INTO CTTHUEPHONG
                            (
                                MaThue,
                                KHACH,
                                VaiTro
                            )
                            VALUES
                            (
                                {newMaThue},
                                {customerId},
                                {vaiTro}
                            )
                        ");

                await transaction.CommitAsync();

                return new BookingResponse
                {
                    MaThue = newMaThue,
                    MaPhong = request.MaPhong,
                    NgayVao = request.NgayVao,
                    NgayTra = request.NgayTra,
                    DatCoc = datCoc,
                    TrangThai = "Đã đặt"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}