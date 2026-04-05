using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Areas.Staff.ViewModels;
using DKS_HotelManager.Helpers;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Areas.Staff.Controllers
{
    [StaffAuthorize]
    public class DeskBookingController : StaffBaseController
    {
        public ActionResult Index()
        {
            ViewBag.ActiveSection = "DeskBooking";
            var hotelId = GetSelectedHotelId();
            var model = BuildDeskBookingModel(hotelId);
            return View(model);
        }

        [HttpGet]
        public JsonResult SearchCustomer(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { items = new object[0] }, JsonRequestBehavior.AllowGet);
            }

            var matches = db.KHACHHANGs
                .Where(k => k.TKH.Contains(query))
                .OrderBy(k => k.TKH)
                .Take(6)
                .Select(k => new
                {
                    k.TKH,
                    k.SDT,
                    k.CMND_CCCD,
                    k.Email
                })
                .ToList();

            return Json(new { items = matches }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Prefix = "BookingInput")] RoomBookingInputModel input)
        {
            ViewBag.ActiveSection = "DeskBooking";
            var hotelId = GetSelectedHotelId();
            if (!hotelId.HasValue)
            {
                TempData["StaffError"] = "Vui lòng chọn chi nhánh trước khi thao tác.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                var model = BuildDeskBookingModel(hotelId);
                if (input != null)
                {
                    model.BookingInput = input;
                }
                return View("Index", model);
            }

            var room = db.PHONGs
                .Include(p => p.THUEPHONGs)
                .Include(p => p.LOAIPHONG)
                .FirstOrDefault(p => p.MaPhong == input.RoomId && p.MaKS == hotelId.Value);
            if (room == null)
            {
                TempData["StaffError"] = "Phòng không thuộc chi nhánh hiện tại.";
                return RedirectToAction("Index");
            }

            var latestBooking = room.THUEPHONGs
                .OrderByDescending(t => t.NgayDat ?? DateTime.MinValue)
                .FirstOrDefault();
            if (DetermineCategory(latestBooking, DateTime.Now) != RoomStatusCategory.Empty)
            {
                TempData["StaffError"] = "Phòng hiện không sẵn sàng để đặt.";
                return RedirectToAction("Index");
            }

            var checkIn = input.CheckIn ?? DateTime.Today;
            var checkOut = input.CheckOut ?? checkIn.AddDays(1);
            if (checkOut <= checkIn)
            {
                TempData["StaffError"] = "Ngày trả phải lớn hơn ngày nhận.";
                return RedirectToAction("Index");
            }

            var stayNights = (checkOut.Date - checkIn.Date).Days;
            if (stayNights < 1)
            {
                stayNights = 1;
            }

            var customer = ResolveCustomer(input);
            var depositAmount = CalculateDeskBookingDeposit(room.DGNgay, stayNights);
            input.Deposit = depositAmount;
            var booking = new THUEPHONG
            {
                MaThue = BookingIdHelper.GetNextBookingId(db),
                MaNV = GetCurrentStaffId() ?? 0,
                MaPhong = room.MaPhong,
                NgayDat = DateTime.Now,
                NgayVao = checkIn,
                NgayTra = checkOut,
                DatCoc = depositAmount,
                TrangThai = "Đang sử dụng",
                MaDatPhong = BuildBookingCode(room)
            };

            db.THUEPHONGs.Add(booking);
            db.SaveChanges();

            db.CTTHUEPHONGs.Add(new CTTHUEPHONG
            {
                MaThue = booking.MaThue,
                KHACH = customer.MKH,
                VaiTro = "Khách chính"
            });
            db.SaveChanges();

            StaffActivityTracker.RecordEvent(
                "Đặt phòng tại chỗ",
                GetCurrentStaffId(),
                GetCurrentStaffName(),
                hotelId,
                GetSelectedHotelName(),
                string.Format("Phòng {0} - {1}", room.TenPhong, customer.TKH));

            TempData["StaffSuccess"] = "Đặt phòng tại quầy thành công.";
            return RedirectToAction("Index");
        }

        private DeskBookingViewModel BuildDeskBookingModel(int? hotelId)
        {
            var model = new DeskBookingViewModel
            {
                SelectedHotelId = hotelId,
                SelectedHotelName = GetSelectedHotelName(),
                StaffName = GetCurrentStaffName()
            };

            if (!hotelId.HasValue)
            {
                return model;
            }

            var rooms = db.PHONGs
                .Include(p => p.LOAIPHONG)
                .Include(p => p.THUEPHONGs)
                .Where(p => p.MaKS == hotelId.Value)
                .OrderBy(p => p.MaPhong)
                .ToList();

            var availableRooms = rooms
                .Where(p =>
                {
                    var latest = p.THUEPHONGs.OrderByDescending(t => t.NgayDat ?? DateTime.MinValue).FirstOrDefault();
                    return DetermineCategory(latest, DateTime.Now) == RoomStatusCategory.Empty;
                })
                .ToList();

            model.RoomOptions = availableRooms
                .Select(p => new SelectListItem
                {
                    Value = p.MaPhong.ToString(),
                    Text = $"{p.TenPhong} - {p.LOAIPHONG?.TenLoai ?? "Phòng tiêu chuẩn"}"
                })
                .ToList();

            model.RoomOptionsWithRates = availableRooms
                .Select(p => new RoomOptionInfo
                {
                    RoomId = p.MaPhong,
                    Text = $"{p.TenPhong} - {p.LOAIPHONG?.TenLoai ?? "Phòng tiêu chuẩn"}",
                    RatePerNight = p.DGNgay
                })
                .ToList();

            return model;
        }

        private static decimal CalculateDeskBookingDeposit(decimal ratePerNight, int nights)
        {
            var total = ratePerNight * Math.Max(1, nights);
            return Math.Round(total * 0.3m, 0, MidpointRounding.AwayFromZero);
        }
    }
}
