using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Areas.Staff.ViewModels;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Areas.Staff.Controllers
{
    public abstract class StaffBaseController : Controller
    {
        protected readonly DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();
        protected const string HotelSessionKey = "StaffSelectedHotelId";

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.HotelSelectorModel = BuildHotelSelectorModel();
            ViewBag.StaffDisplayName = GetCurrentStaffName();
        }

        protected HotelSelectorViewModel BuildHotelSelectorModel()
        {
            var selectedHotelId = GetSelectedHotelId();
            var hotels = db.KHACHSANs
                .OrderBy(h => h.TenKS)
                .Select(h => new SelectListItem
                {
                    Value = h.MaKS.ToString(),
                    Text = h.TenKS,
                    Selected = selectedHotelId.HasValue && h.MaKS == selectedHotelId.Value
                })
                .ToList();

            return new HotelSelectorViewModel
            {
                SelectedHotelId = selectedHotelId,
                Hotels = hotels
            };
        }

        protected int? GetSelectedHotelId()
        {
            if (Session[HotelSessionKey] is int id)
            {
                return id;
            }

            if (Session[HotelSessionKey] != null && int.TryParse(Session[HotelSessionKey].ToString(), out var parsed))
            {
                return parsed;
            }

            return null;
        }

        protected string GetSelectedHotelName()
        {
            var hotelId = GetSelectedHotelId();
            if (!hotelId.HasValue)
            {
                return null;
            }

            return db.KHACHSANs.Where(h => h.MaKS == hotelId.Value).Select(h => h.TenKS).FirstOrDefault();
        }

        protected int? GetCurrentStaffId()
        {
            if (Session["AdminId"] is int staffId)
            {
                return staffId;
            }

            if (Session["AdminUser"] is NHANVIEN staff)
            {
                return staff.MaNV;
            }

            return null;
        }

        protected string GetCurrentStaffName()
        {
            if (Session["AdminUser"] is NHANVIEN staff)
            {
                return staff.HoTen;
            }

            return "Nhân viên lễ tân";
        }

        protected decimal CalculateDue(THUEPHONG booking)
        {
            var roomRate = booking.PHONG != null ? booking.PHONG.DGNgay : 0m;
            var checkIn = booking.NgayVao ?? DateTime.Now;
            var checkOut = booking.NgayTra ?? checkIn.AddDays(1);
            var nights = Math.Max(1, (int)(checkOut.Date - checkIn.Date).TotalDays);
            var roomCharge = nights * roomRate;
            var servicesTotal = CalculateServicesTotal(booking.MaThue);
            var deposit = booking.DatCoc ?? 0m;
            var total = roomCharge + servicesTotal - deposit;
            return Math.Max(total, 0);
        }

        protected decimal CalculateEstimatedTotal(THUEPHONG booking)
        {
            if (booking == null)
            {
                return 0m;
            }

            var roomRate = booking.PHONG != null ? booking.PHONG.DGNgay : 0m;
            var checkIn = booking.NgayVao ?? DateTime.Now;
            var checkOut = booking.NgayTra ?? checkIn.AddDays(1);
            var nights = Math.Max(1, (int)(checkOut.Date - checkIn.Date).TotalDays);
            var roomCharge = nights * roomRate;
            var servicesTotal = CalculateServicesTotal(booking.MaThue);
            return Math.Max(roomCharge + servicesTotal, 0m);
        }

        protected decimal CalculateServicesTotal(int bookingId)
        {
            return db.Database.SqlQuery<decimal?>(
                    @"SELECT SUM(ISNULL(s.SoLuot, 0) * ISNULL(d.DGDV, 0))
                      FROM SDDICHVU s
                      JOIN DICHVU d ON s.DV = d.MaDV
                      WHERE s.MaThue = @p0", bookingId)
                .SingleOrDefault() ?? 0m;
        }

        protected RoomStatusCategory DetermineCategory(THUEPHONG booking, DateTime now)
        {
            if (booking == null)
            {
                return RoomStatusCategory.Empty;
            }

            var status = (booking.TrangThai ?? string.Empty).ToLowerInvariant();
            var checkIn = booking.NgayVao;
            var checkOut = booking.NgayTra;

            if (status.Contains("đã trả") || status.Contains("trả phòng"))
            {
                if (checkOut.HasValue && checkOut.Value.AddMinutes(30) > now)
                {
                    return RoomStatusCategory.CheckedOut;
                }

                return RoomStatusCategory.Empty;
            }

            if (status.Contains("đã thanh toán"))
            {
                return IsPaymentFresh(booking, now) ? RoomStatusCategory.Paid : RoomStatusCategory.Empty;
            }

            if (status.Contains("chờ"))
            {
                return RoomStatusCategory.Reserved;
            }

            if (status.Contains("đang đặt"))
            {
                return RoomStatusCategory.Reserved;
            }

            if (status.Contains("đặt"))
            {
                return RoomStatusCategory.Reserved;
            }

            if (status.Contains("đang"))
            {
                return RoomStatusCategory.Occupied;
            }

            if (status.Contains("hủy") || status.Contains("huy"))
            {
                return RoomStatusCategory.Empty;
            }

            if (checkIn.HasValue && checkOut.HasValue && checkIn.Value <= now && checkOut.Value >= now)
            {
                return RoomStatusCategory.Occupied;
            }

            if (checkIn.HasValue && checkIn.Value > now)
            {
                return RoomStatusCategory.Reserved;
            }

            if (checkOut.HasValue && checkOut.Value.AddMinutes(30) > now)
            {
                return RoomStatusCategory.CheckedOut;
            }

            return RoomStatusCategory.Empty;
        }

        protected bool IsPaymentFresh(THUEPHONG booking, DateTime now)
        {
            if (booking == null)
            {
                return false;
            }

            var lastPaymentTime = db.THANHTOANs
                .Where(t => t.MaThue == booking.MaThue && t.NgayTT.HasValue)
                .OrderByDescending(t => t.NgayTT)
                .Select(t => t.NgayTT.Value)
                .FirstOrDefault();

            if (lastPaymentTime == default)
            {
                return false;
            }

            return lastPaymentTime.AddMinutes(5) >= now;
        }

        protected RoomStatusTile BuildRoomTile(PHONG room, THUEPHONG booking, DateTime now)
        {
            var category = DetermineCategory(booking, now);
            var guestName = booking != null ? GetGuestName(booking) : null;
            var detail = "Sẵn sàng";
            var checkInText = booking != null && booking.NgayVao.HasValue ? booking.NgayVao.Value.ToString("dd/MM HH:mm") : "chưa rõ";
            var checkOutText = booking != null && booking.NgayTra.HasValue ? booking.NgayTra.Value.ToString("dd/MM HH:mm") : "chưa rõ";

            switch (category)
            {
                case RoomStatusCategory.Reserved:
                    detail = "Đặt trước: " + checkInText;
                    break;
                case RoomStatusCategory.Occupied:
                    var guestLabel = !string.IsNullOrWhiteSpace(guestName) ? guestName : "khách";
                    detail = "Khách " + guestLabel + " - " + checkInText;
                    break;
                case RoomStatusCategory.Paid:
                    detail = "Đã thanh toán vào " + (booking != null && booking.NgayTra.HasValue ? booking.NgayTra.Value.ToString("dd/MM") : "chưa rõ");
                    break;
                case RoomStatusCategory.CheckedOut:
                    detail = "Đã trả phòng lúc " + checkOutText;
                    break;
            }

            string badge;
            switch (category)
            {
                case RoomStatusCategory.Empty:
                    badge = "Trống";
                    break;
                case RoomStatusCategory.Reserved:
                    badge = "Đã đặt trước";
                    break;
                case RoomStatusCategory.Occupied:
                    badge = "Đang sử dụng";
                    break;
                case RoomStatusCategory.Paid:
                    badge = "Đã thanh toán";
                    break;
                case RoomStatusCategory.CheckedOut:
                    badge = "Đã trả phòng";
                    break;
                default:
                    badge = "Trống";
                    break;
            }

            return new RoomStatusTile
            {
                RoomId = room.MaPhong,
                RoomName = room.TenPhong,
                Category = category,
                BadgeText = badge,
                Detail = detail
            };
        }

        protected GuestStayInfo CreateGuestInfo(THUEPHONG booking, PHONG room)
        {
            return new GuestStayInfo
            {
                BookingId = booking.MaThue,
                RoomId = room.MaPhong,
                RoomName = room.TenPhong,
                GuestName = GetGuestName(booking),
                BookingCode = booking.MaDatPhong,
                CheckIn = booking.NgayVao,
                CheckOut = booking.NgayTra,
                Status = booking.TrangThai
            };
        }

        protected IEnumerable<CustomerRecord> SearchCustomers(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<CustomerRecord>();
            }

            query = query.Trim();
            return db.KHACHHANGs
                .Where(k => k.TKH.Contains(query) || k.CMND_CCCD.Contains(query))
                .OrderBy(k => k.TKH)
                .Take(10)
                .Select(k => new CustomerRecord
                {
                    Id = k.MKH,
                    FullName = k.TKH,
                    Phone = k.SDT,
                    IdentityNumber = k.CMND_CCCD,
                    Email = k.Email
                })
                .ToList();
        }

        protected string BuildBookingCode(PHONG room)
        {
            return string.Format("DP{0:yyyyMMddHHmmss}{1}", DateTime.Now, room.MaPhong);
        }

        protected KHACHHANG ResolveCustomer(RoomBookingInputModel input)
        {
            var identity = (input.IdentityNumber ?? string.Empty).Trim();
            var phone = (input.Phone ?? string.Empty).Trim();

            KHACHHANG customer = null;
            if (!string.IsNullOrEmpty(identity))
            {
                customer = db.KHACHHANGs.FirstOrDefault(k => k.CMND_CCCD == identity);
            }

            if (customer == null && !string.IsNullOrEmpty(phone))
            {
                customer = db.KHACHHANGs.FirstOrDefault(k => k.SDT == phone);
            }

            if (customer != null)
            {
                return customer;
            }

            var newId = db.KHACHHANGs.Any() ? db.KHACHHANGs.Max(k => k.MKH) + 1 : 1;
            var usernameBase = "user" + newId;
            var username = usernameBase;
            var counter = 1;
            while (db.KHACHHANGs.Any(k => k.TenDN == username))
            {
                username = usernameBase + "_" + counter;
                counter++;
            }

            customer = new KHACHHANG
            {
                MKH = newId,
                TKH = string.IsNullOrWhiteSpace(input.CustomerName) ? "Khách " + newId : input.CustomerName,
                SDT = phone,
                CMND_CCCD = identity,
                DiaChi = "Đăng ký trực tiếp tại quầy",
                TenDN = username,
                MatKhau = Guid.NewGuid().ToString("N").Substring(0, 8)
            };

            db.KHACHHANGs.Add(customer);
            db.SaveChanges();
            return customer;
        }

        protected string GetGuestName(THUEPHONG booking)
        {
            if (booking == null || booking.CTTHUEPHONGs == null)
            {
                return "Khách vãng lai";
            }

            var ct = booking.CTTHUEPHONGs.FirstOrDefault();
            if (ct == null || ct.KHACHHANG == null)
            {
                return "Khách vãng lai";
            }

            return !string.IsNullOrWhiteSpace(ct.KHACHHANG.TKH) ? ct.KHACHHANG.TKH : "Khách vãng lai";
        }
    }
}
