using Microsoft.EntityFrameworkCore;
using payment_service.Data;
using payment_service.DTOs;
using payment_service.Models;
using payment_service.Services.Interfaces;

namespace payment_service.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentResponse>> GetAll()
        {
            return await _context.Payments
                .OrderByDescending(x => x.NgayTT)
                .ThenByDescending(x => x.MaTT)
                .Select(x => new PaymentResponse
                {
                    MaTT = x.MaTT,
                    MaThue = x.MaThue,
                    HinhThucTT = x.HinhThucTT,
                    ThanhTien = x.ThanhTien,
                    NgayTT = x.NgayTT
                })
                .ToListAsync();
        }

        public async Task<PaymentResponse?> GetById(int id)
        {
            return await _context.Payments
                .Where(x => x.MaTT == id)
                .Select(x => new PaymentResponse
                {
                    MaTT = x.MaTT,
                    MaThue = x.MaThue,
                    HinhThucTT = x.HinhThucTT,
                    ThanhTien = x.ThanhTien,
                    NgayTT = x.NgayTT
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentResponse> Create(PaymentRequest request)
        {
            if (request.MaThue <= 0)
            {
                throw new Exception("Mã thuê không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(request.HinhThucTT))
            {
                throw new Exception("Hình thức thanh toán không được để trống.");
            }

            if (request.HinhThucTT.Length > 50)
            {
                throw new Exception("Hình thức thanh toán không được vượt quá 50 ký tự.");
            }

            if (request.ThanhTien <= 0)
            {
                throw new Exception("Thành tiền phải lớn hơn 0.");
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(x => x.MaThue == request.MaThue);

            if (booking == null)
            {
                throw new Exception($"Không tìm thấy phiếu thuê MaThue = {request.MaThue}.");
            }

            int newMaTT = await _context.Payments.AnyAsync()
                ? await _context.Payments.MaxAsync(x => x.MaTT) + 1
                : 1;

            var payment = new Payment
            {
                MaTT = newMaTT,
                MaThue = request.MaThue,
                HinhThucTT = request.HinhThucTT.Trim(),
                ThanhTien = request.ThanhTien,
                NgayTT = DateTime.Now.Date
            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                MaTT = payment.MaTT,
                MaThue = payment.MaThue,
                HinhThucTT = payment.HinhThucTT,
                ThanhTien = payment.ThanhTien,
                NgayTT = payment.NgayTT
            };
        }
    }
}