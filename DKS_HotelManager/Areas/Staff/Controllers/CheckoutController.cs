using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Areas.Staff.ViewModels;
using DKS_HotelManager.Helpers;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Areas.Staff.Controllers
{
    [StaffAuthorize]
    public class CheckoutController : StaffBaseController
    {
        public ActionResult Index()
        {
            ViewBag.ActiveSection = "Checkout";
            var hotelId = GetSelectedHotelId();
            var hotelName = GetSelectedHotelName();
            var staffName = GetCurrentStaffName();
            var now = DateTime.Now;
            var bookingsQuery = db.THUEPHONGs
                .Include(b => b.PHONG.KHACHSAN)
                .Include(b => b.CTTHUEPHONGs.Select(c => c.KHACHHANG))
                .Include(b => b.SDDICHVUs.Select(s => s.DICHVU))
                .AsQueryable();

            if (hotelId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.PHONG != null && b.PHONG.MaKS == hotelId.Value);
            }

            bookingsQuery = bookingsQuery.OrderByDescending(b => b.NgayDat);

            var bookings = bookingsQuery
                .ToList()
                .Where(b => DetermineCategory(b, now) == RoomStatusCategory.Occupied)
                .ToList();

            var cards = bookings.Select(b =>
            {
                var (roomTotal, servicesTotal, deposit) = CalculateBookingTotals(b);
                var total = roomTotal + servicesTotal;
                return new StaffCheckoutCardViewModel
                {
                    BookingId = b.MaThue,
                    ReferenceNumber = b.MaDatPhong,
                    HotelName = b.PHONG?.KHACHSAN?.TenKS ?? "Chuá»—i DKS",
                    RoomName = b.PHONG?.TenPhong ?? "PhĂ²ng chÆ°a xĂ¡c Ä‘á»‹nh",
                    GuestName = b.CTTHUEPHONGs.FirstOrDefault(c => c.VaiTro == "KhĂ¡ch chĂ­nh")?.KHACHHANG?.TKH ?? "KhĂ¡ch chĂ­nh",
                    CheckIn = b.NgayVao,
                    CheckOut = b.NgayTra,
                    DepositAmount = deposit,
                    TotalAmount = total,
                    RemainingBalance = Math.Max(total - deposit, 0),
                    OutstandingDue = deposit
                };
            }).ToList();

            var model = new StaffCheckoutViewModel
            {
                Cards = cards,
                AlertMessage = TempData["StaffPaymentMessage"] as string,
                SelectedHotelName = hotelName,
                StaffName = staffName,
                TotalRemainingBalance = cards.Sum(c => c.RemainingBalance),
                TotalOutstandingBalance = cards.Sum(c => c.OutstandingDue)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayCash(int bookingId)
        {
            var booking = db.THUEPHONGs.FirstOrDefault(b => b.MaThue == bookingId);
            if (booking == null)
            {
                return HttpNotFound();
            }

            var (_, _, deposit) = CalculateBookingTotals(booking);

            var existing = db.THANHTOANs.FirstOrDefault(t => t.MaThue == bookingId && t.HinhThucTT == "Tiá»n máº·t" && t.ThanhTien == deposit);
            if (existing == null)
            {
                db.THANHTOANs.Add(new THANHTOAN
                {
                    MaTT = PaymentHelper.GetNextThanhToanId(db),
                    MaThue = bookingId,
                    HinhThucTT = "Tiá»n máº·t",
                    ThanhTien = deposit,
                    NgayTT = DateTime.Now
                });
            }

            booking.DatCoc = deposit;
            booking.TrangThai = "ÄĂ£ thanh toĂ¡n";
            db.Entry(booking).State = EntityState.Modified;
            db.SaveChanges();

            TempData["StaffPaymentMessage"] = $"Thanh toĂ¡n tiá»n máº·t thĂ nh cĂ´ng cho Ä‘Æ¡n {booking.MaDatPhong}.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayWithVnPay(int bookingId)
        {
            var booking = db.THUEPHONGs
                .Include(b => b.PHONG.KHACHSAN)
                .Include(b => b.SDDICHVUs.Select(s => s.DICHVU))
                .FirstOrDefault(b => b.MaThue == bookingId);

            if (booking == null)
            {
                TempData["StaffPaymentMessage"] = "KhĂ´ng tĂ¬m tháº¥y Ä‘áº·t phĂ²ng Ä‘á»ƒ thanh toĂ¡n.";
                return RedirectToAction("Index");
            }

            var (roomTotal, servicesTotal, deposit) = CalculateBookingTotals(booking);
            var total = roomTotal + servicesTotal;
            var remaining = Math.Max(total - deposit, 0);

            var vnpUrl = ConfigurationManager.AppSettings["VnpPayUrl"];
            var returnUrl = ConfigurationManager.AppSettings["VnpReturnUrlStaff"];
            var tmnCode = ConfigurationManager.AppSettings["VnpTmnCode"];
            var hashSecret = SecurityConfig.GetSecret("VnpHashSecret", "DKS_VNP_HASH_SECRET");

            if (string.IsNullOrWhiteSpace(vnpUrl) || string.IsNullOrWhiteSpace(returnUrl) ||
                string.IsNullOrWhiteSpace(tmnCode) || string.IsNullOrWhiteSpace(hashSecret))
            {
                TempData["StaffPaymentMessage"] = "VNPAY config is missing for staff.";
                SecurityAuditLogger.Log("payment_staff", "vnpay_missing_config", "error");
                return RedirectToAction("Index");
            }

            var pay = new PayLib();
            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmnCode);
            var chargeAmount = remaining > 0 ? remaining : 1000m;
            pay.AddRequestData("vnp_Amount", ((long)Math.Round(chargeAmount * 100, 0, MidpointRounding.AwayFromZero)).ToString());
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress());
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", BuildOrderInfo(bookingId));
            pay.AddRequestData("vnp_OrderType", "billpayment");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", Guid.NewGuid().ToString("N").Substring(0, 10));

            var paymentUrl = pay.CreateRequestUrl(vnpUrl, hashSecret);

            Session["StaffPendingBooking"] = new StaffCheckoutCardViewModel
            {
                BookingId = booking.MaThue,
                ReferenceNumber = booking.MaDatPhong,
                HotelName = booking.PHONG?.KHACHSAN?.TenKS,
                RoomName = booking.PHONG?.TenPhong,
                GuestName = booking.CTTHUEPHONGs.FirstOrDefault(c => c.VaiTro == "KhĂ¡ch chĂ­nh")?.KHACHHANG?.TKH,
                CheckIn = booking.NgayVao,
                CheckOut = booking.NgayTra,
                DepositAmount = deposit,
                TotalAmount = total,
                RemainingBalance = remaining,
                OutstandingDue = remaining
            };
            SecurityAuditLogger.Log("payment_staff", "vnpay_redirect_created", "info", new Dictionary<string, object>
            {
                { "bookingId", booking.MaThue },
                { "remaining", remaining },
                { "ip", SecurityAuditLogger.GetClientIp(Request) }
            });

            return Redirect(paymentUrl);
        }

        [AllowAnonymous]
        public ActionResult PaymentConfirm()
        {
            var pay = new PayLib();
            var hashSecret = SecurityConfig.GetSecret("VnpHashSecret", "DKS_VNP_HASH_SECRET");
            foreach (string key in Request.QueryString)
            {
                if (!string.IsNullOrWhiteSpace(key) && key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                {
                    pay.AddResponseData(key, Request.QueryString[key]);
                }
            }

            if (string.IsNullOrWhiteSpace(hashSecret))
            {
                TempData["StaffPaymentMessage"] = "Missing VNPAY verification secret.";
                SecurityAuditLogger.Log("payment_staff", "vnpay_callback_missing_secret", "error");
                Session.Remove("StaffPendingBooking");
                return RedirectToAction("Index");
            }

            var signatureValid = pay.ValidateSignature(hashSecret);
            var responseCode = pay.GetResponseData("vnp_ResponseCode");
            if (!signatureValid || !string.Equals(responseCode, "00", StringComparison.Ordinal))
            {
                SecurityAuditLogger.Log("payment_staff", "vnpay_callback_invalid", "warning", new Dictionary<string, object>
                {
                    { "signatureValid", signatureValid },
                    { "responseCode", responseCode },
                    { "txnRef", pay.GetResponseData("vnp_TxnRef") },
                    { "ip", SecurityAuditLogger.GetClientIp(Request) }
                });
                TempData["StaffPaymentMessage"] = "VNPAY transaction is invalid or failed.";
                Session.Remove("StaffPendingBooking");
                return RedirectToAction("Index");
            }

            var orderInfo = pay.GetResponseData("vnp_OrderInfo");
            var model = BuildPaymentConfirmViewModel(orderInfo);
            if (model == null)
            {
                TempData["StaffPaymentMessage"] = "KhĂ´ng tĂ¬m tháº¥y thĂ´ng tin thanh toĂ¡n.";
                Session.Remove("StaffPendingBooking");
                return RedirectToAction("Index");
            }

            var payResult = FinalizePayment(model, pay);
            Session.Remove("StaffPendingBooking");

            if (!payResult)
            {
                TempData["StaffPaymentMessage"] = "Thanh toĂ¡n thĂ nh cĂ´ng nhÆ°ng khĂ´ng thá»ƒ lÆ°u.";
                SecurityAuditLogger.Log("payment_staff", "vnpay_finalize_failed", "error", new Dictionary<string, object>
                {
                    { "txnRef", pay.GetResponseData("vnp_TxnRef") },
                    { "bookingId", model.BookingId }
                });
                return RedirectToAction("Index");
            }

            model.TransactionNo = pay.GetResponseData("vnp_TransactionNo");
            model.TxnRef = pay.GetResponseData("vnp_TxnRef");
            model.PayDate = pay.GetResponseData("vnp_PayDate");
            SecurityAuditLogger.Log("payment_staff", "vnpay_success", "info", new Dictionary<string, object>
            {
                { "txnRef", model.TxnRef },
                { "transactionNo", model.TransactionNo },
                { "bookingId", model.BookingId }
            });

            return View(model);
        }

        private (decimal roomTotal, decimal servicesTotal, decimal deposit) CalculateBookingTotals(THUEPHONG booking)
        {
            var nights = 1;
            if (booking.NgayVao.HasValue && booking.NgayTra.HasValue)
            {
                nights = (booking.NgayTra.Value.Date - booking.NgayVao.Value.Date).Days;
                if (nights <= 0)
                {
                    nights = 1;
                }
            }

            var roomRate = booking.PHONG?.DGNgay ?? 0m;
            var roomTotal = roomRate * nights;

            var servicesTotal = booking.SDDICHVUs?
                .Sum(n => (n.SoLuot ?? 1) * (n.DICHVU?.DGDV ?? 0m)) ?? 0m;

            var deposit = Math.Round((roomTotal + servicesTotal) * 0.3m, 0, MidpointRounding.AwayFromZero);
            if (deposit <= 0)
            {
                deposit = 5000;
            }

            return (roomTotal, servicesTotal, deposit);
        }

        private string BuildOrderInfo(int bookingId)
        {
            return bookingId.ToString(CultureInfo.InvariantCulture);
        }

        private StaffPaymentConfirmViewModel BuildPaymentConfirmViewModel(string orderInfo)
        {
            var pending = Session["StaffPendingBooking"] as StaffCheckoutCardViewModel;
            int bookingId;

            if (!string.IsNullOrWhiteSpace(orderInfo))
            {
                int.TryParse(orderInfo, out bookingId);
            }
            else
            {
                bookingId = pending?.BookingId ?? 0;
            }

            if (bookingId <= 0)
            {
                return null;
            }

            var booking = db.THUEPHONGs
                .Include(b => b.PHONG.KHACHSAN)
                .Include(b => b.SDDICHVUs.Select(s => s.DICHVU))
                .Include(b => b.CTTHUEPHONGs.Select(c => c.KHACHHANG))
                .FirstOrDefault(b => b.MaThue == bookingId);

            if (booking == null)
            {
                return null;
            }

            var (roomTotal, servicesTotal, deposit) = CalculateBookingTotals(booking);
            var remaining = pending?.RemainingBalance ?? Math.Max(roomTotal + servicesTotal - deposit, 0);
            if (pending == null)
            {
                var guestName = booking.CTTHUEPHONGs.FirstOrDefault(c => c.VaiTro == "KhĂ¡ch chĂ­nh")?.KHACHHANG?.TKH ?? "KhĂ¡ch";
                pending = new StaffCheckoutCardViewModel
                {
                    BookingId = booking.MaThue,
                    ReferenceNumber = booking.MaDatPhong,
                    HotelName = booking.PHONG?.KHACHSAN?.TenKS,
                    RoomName = booking.PHONG?.TenPhong,
                    GuestName = guestName,
                    CheckIn = booking.NgayVao,
                    CheckOut = booking.NgayTra,
                    DepositAmount = deposit,
                    TotalAmount = roomTotal + servicesTotal,
                    RemainingBalance = Math.Max(roomTotal + servicesTotal - deposit, 0)
                };
            }

            return new StaffPaymentConfirmViewModel
            {
                BookingId = pending.BookingId,
                ReferenceNumber = pending.ReferenceNumber,
                HotelName = pending.HotelName,
                RoomName = pending.RoomName,
                GuestName = pending.GuestName,
                CheckIn = pending.CheckIn,
                CheckOut = pending.CheckOut,
                DepositAmount = deposit,
                RoomTotal = roomTotal,
                ServicesTotal = servicesTotal,
                TotalAmount = roomTotal + servicesTotal,
                RemainingBefore = Math.Max(roomTotal + servicesTotal - deposit, 0),
                PaidAmount = remaining
            };
        }

        private bool FinalizePayment(StaffPaymentConfirmViewModel model, PayLib pay)
        {
            var booking = db.THUEPHONGs
                .Include(b => b.SDDICHVUs.Select(s => s.DICHVU))
                .Include(b => b.PHONG.KHACHSAN)
                .FirstOrDefault(b => b.MaThue == model.BookingId);

            if (booking == null)
            {
                return false;
            }

            var (roomTotal, servicesTotal, deposit) = CalculateBookingTotals(booking);
            var total = roomTotal + servicesTotal;
            var paidAmount = model.PaidAmount > 0 ? model.PaidAmount : Math.Max(total - deposit, 0);

            db.THANHTOANs.Add(new THANHTOAN
            {
                MaTT = PaymentHelper.GetNextThanhToanId(db),
                MaThue = booking.MaThue,
                HinhThucTT = "VNPAY",
                ThanhTien = paidAmount,
                NgayTT = DateTime.Now
            });

            booking.DatCoc = Math.Max(total, booking.DatCoc ?? 0);
            booking.TrangThai = "ÄĂ£ thanh toĂ¡n";
            db.Entry(booking).State = EntityState.Modified;
            db.SaveChanges();

            return true;
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

