using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Helpers;
using DKS_HotelManager.Models;
using DKS_HotelManager.Models.ViewModels;

namespace DKS_HotelManager.Controllers
{
    [CustomerAuthorize]
    public class AccountController : Controller
    {
        private readonly DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();

        private int? GetCurrentCustomerId()
        {
            if (Session["KhachHangId"] != null && int.TryParse(Session["KhachHangId"].ToString(), out var id))
            {
                return id;
            }
            return null;
        }

        public ActionResult Index()
        {
            var customerId = GetCurrentCustomerId();
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var customer = db.KHACHHANGs.FirstOrDefault(c => c.MKH == customerId.Value);
            if (customer == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var bookings = db.THUEPHONGs
                .Include(b => b.PHONG)
                .Include(b => b.PHONG.KHACHSAN)
                .Include(b => b.CTTHUEPHONGs)
                .Where(b => b.CTTHUEPHONGs.Any(ct => ct.KHACH == customerId.Value))
                .OrderByDescending(b => b.NgayDat)
                .ToList()
                .Select(b => new BookingSummaryViewModel
                {
                    BookingId = b.MaThue,
                    BookingCode = b.MaDatPhong,
                    HotelName = b.PHONG?.KHACHSAN?.TenKS ?? "Khách sạn",
                    RoomName = b.PHONG?.TenPhong,
                    CheckIn = b.NgayVao,
                    CheckOut = b.NgayTra,
                    Status = string.IsNullOrWhiteSpace(b.TrangThai) ? "Đang chờ" : b.TrangThai,
                    Deposit = b.DatCoc
                })
                .ToList();

            var viewModel = new AccountPageViewModel
            {
                Profile = new AccountProfileViewModel
                {
                    CustomerId = customer.MKH,
                    FullName = customer.TKH,
                    Phone = customer.SDT,
                    Address = customer.DiaChi,
                    IdNumber = customer.CMND_CCCD
                },
                Password = new ChangePasswordViewModel(),
                Bookings = bookings
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(AccountProfileViewModel profile)
        {
            var customerId = GetCurrentCustomerId();
            if (!customerId.HasValue || profile.CustomerId != customerId.Value)
            {
                TempData["AccountError"] = "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Authentication");
            }

            if (!ModelState.IsValid)
            {
                TempData["AccountError"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m)));
                return RedirectToAction("Index");
            }

            var customer = db.KHACHHANGs.FirstOrDefault(c => c.MKH == customerId.Value);
            if (customer == null)
            {
                TempData["AccountError"] = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("Index");
            }

            customer.TKH = profile.FullName;
            customer.SDT = profile.Phone;
            customer.DiaChi = profile.Address;
            customer.CMND_CCCD = profile.IdNumber;
            db.Entry(customer).State = EntityState.Modified;
            db.SaveChanges();

            Session["KhachHang"] = customer;
            Session["KhachHangTen"] = customer.TKH;

            TempData["AccountSuccess"] = "Cập nhật thông tin tài khoản thành công.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var customerId = GetCurrentCustomerId();
            if (!customerId.HasValue)
            {
                TempData["AccountError"] = "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Authentication");
            }

            if (!ModelState.IsValid)
            {
                TempData["AccountError"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m)));
                return RedirectToAction("Index");
            }

            var customer = db.KHACHHANGs.FirstOrDefault(c => c.MKH == customerId.Value);
            if (customer == null)
            {
                TempData["AccountError"] = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("Index");
            }

            if (!PasswordHasher.VerifyPassword(customer.MatKhau, model.CurrentPassword, out var needsRehash))
            {
                TempData["AccountError"] = "Mat khau hien tai khong dung.";
                SecurityAuditLogger.Log("auth", "change_password_failed_wrong_current", "warning", new Dictionary<string, object>
                {
                    { "customerId", customer.MKH },
                    { "ip", SecurityAuditLogger.GetClientIp(Request) }
                });
                return RedirectToAction("Index");
            }

            customer.MatKhau = PasswordHasher.HashPassword(model.NewPassword);
            db.Entry(customer).State = EntityState.Modified;
            db.SaveChanges();
            SecurityAuditLogger.Log("auth", "password_changed", "info", new Dictionary<string, object>
            {
                { "customerId", customer.MKH },
                { "passwordRehashNeeded", needsRehash },
                { "ip", SecurityAuditLogger.GetClientIp(Request) }
            });

            TempData["AccountSuccess"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("Index");
        }

        public ActionResult EditBooking(int id)
        {
            var customerId = GetCurrentCustomerId();
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var booking = db.THUEPHONGs
                .Include(b => b.PHONG.KHACHSAN)
                .Include(b => b.CTTHUEPHONGs)
                .FirstOrDefault(b => b.MaThue == id && b.CTTHUEPHONGs.Any(ct => ct.KHACH == customerId.Value));

            if (booking == null)
            {
                LogPotentialIdorAttempt(customerId, id, "edit_booking_get");
                TempData["AccountError"] = "Không tìm thấy đặt phòng để chỉnh sửa.";
                return RedirectToAction("Index");
            }

            var roomEntities = db.PHONGs
                .Where(p => p.MaKS == booking.PHONG.MaKS)
                .ToList();

            var rooms = roomEntities
                .Select(p => new SelectListItem
                {
                    Value = p.MaPhong.ToString(),
                    Text = p.TenPhong + " - " + p.DGNgay.ToString("N0") + " VND/đêm",
                    Selected = p.MaPhong == booking.MaPhong
                })
                .ToList();

            var viewModel = new AccountBookingEditViewModel
            {
                BookingId = booking.MaThue,
                HotelId = booking.PHONG.MaKS,
                HotelName = booking.PHONG.KHACHSAN?.TenKS ?? "Khách sạn",
                RoomId = booking.MaPhong,
                CheckIn = booking.NgayVao,
                CheckOut = booking.NgayTra,
                Deposit = booking.DatCoc,
                RoomOptions = rooms
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBooking(AccountBookingEditViewModel model)
        {
            var customerId = GetCurrentCustomerId();
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (!ModelState.IsValid)
            {
                TempData["AccountError"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m)));
                return RedirectToAction("EditBooking", new { id = model.BookingId });
            }

            if (!model.CheckIn.HasValue || !model.CheckOut.HasValue || model.CheckIn.Value.Date >= model.CheckOut.Value.Date)
            {
                TempData["AccountError"] = "Ngày trả phòng phải sau ngày nhận phòng.";
                return RedirectToAction("EditBooking", new { id = model.BookingId });
            }

            var booking = db.THUEPHONGs
                .Include(b => b.PHONG)
                .Include(b => b.CTTHUEPHONGs)
                .FirstOrDefault(b => b.MaThue == model.BookingId && b.CTTHUEPHONGs.Any(ct => ct.KHACH == customerId.Value));

            if (booking == null)
            {
                LogPotentialIdorAttempt(customerId, model.BookingId, "edit_booking_post");
                TempData["AccountError"] = "Không tìm thấy đặt phòng để chỉnh sửa.";
                return RedirectToAction("Index");
            }

            var room = db.PHONGs.FirstOrDefault(p => p.MaPhong == model.RoomId && p.MaKS == booking.PHONG.MaKS);
            if (room == null)
            {
                TempData["AccountError"] = "Phòng chọn không hợp lệ.";
                return RedirectToAction("EditBooking", new { id = model.BookingId });
            }

            booking.MaPhong = room.MaPhong;
            booking.NgayVao = model.CheckIn;
            booking.NgayTra = model.CheckOut;
            booking.DatCoc = model.Deposit ?? booking.DatCoc;
            booking.TrangThai = "Đang chờ";

            db.Entry(booking).State = EntityState.Modified;
            db.SaveChanges();

            TempData["AccountSuccess"] = "Cập nhật đặt phòng thành công.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelBooking(int id)
        {
            var customerId = GetCurrentCustomerId();
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var booking = db.THUEPHONGs
                .Include(b => b.PHONG)
                .Include(b => b.CTTHUEPHONGs)
                .FirstOrDefault(b => b.MaThue == id && b.CTTHUEPHONGs.Any(ct => ct.KHACH == customerId.Value));

            if (booking == null)
            {
                LogPotentialIdorAttempt(customerId, id, "cancel_booking");
                TempData["AccountError"] = "Không tìm thấy đặt phòng để hủy.";
                return RedirectToAction("Index");
            }

            booking.TrangThai = "Đã hủy";
            booking.NgayVao = null;
            booking.NgayTra = null;
            db.Entry(booking).State = EntityState.Modified;
            db.SaveChanges();

            TempData["AccountSuccess"] = "Hủy đặt phòng thành công.";
            return RedirectToAction("Index");
        }

        private void LogPotentialIdorAttempt(int? customerId, int bookingId, string operation)
        {
            if (!customerId.HasValue)
            {
                return;
            }

            var bookingExists = db.THUEPHONGs.Any(b => b.MaThue == bookingId);
            if (!bookingExists)
            {
                return;
            }

            SecurityAuditLogger.Log("access_control", "idor_blocked", "warning", new Dictionary<string, object>
            {
                { "customerId", customerId.Value },
                { "targetBookingId", bookingId },
                { "operation", operation },
                { "ip", SecurityAuditLogger.GetClientIp(Request) }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}


