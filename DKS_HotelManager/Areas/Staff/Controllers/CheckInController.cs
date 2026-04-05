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
    public class CheckInController : StaffBaseController
    {
        public ActionResult Index()
        {
            ViewBag.ActiveSection = "CheckIn";
            var model = BuildCheckInView(GetSelectedHotelId());
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirm(CheckInInputModel input)
        {
            ViewBag.ActiveSection = "CheckIn";
            var hotelId = GetSelectedHotelId();
            if (!hotelId.HasValue)
            {
                TempData["StaffError"] = "Vui lòng chọn chi nhánh trước khi thao tác.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                TempData["StaffError"] = "Vui lòng chọn booking để nhận phòng.";
                return RedirectToAction("Index");
            }

            var booking = db.THUEPHONGs
                .Include(b => b.PHONG)
                .FirstOrDefault(b => b.MaThue == input.BookingId && b.PHONG.MaKS == hotelId.Value);
            if (booking == null || DetermineCategory(booking, DateTime.Now) != RoomStatusCategory.Reserved)
            {
                TempData["StaffError"] = "Booking không hợp lệ để nhận phòng.";
                return RedirectToAction("Index");
            }

            booking.NgayVao = DateTime.Now;
            booking.TrangThai = "Đang sử dụng";
            db.Entry(booking).State = EntityState.Modified;
            db.SaveChanges();

            StaffActivityTracker.RecordEvent(
                "Nhận phòng đã đặt trước",
                GetCurrentStaffId(),
                GetCurrentStaffName(),
                hotelId,
                GetSelectedHotelName(),
                string.Format("Phòng {0} - {1}", booking.PHONG.TenPhong, booking.MaDatPhong));

            TempData["StaffSuccess"] = "Khách đã được nhận phòng.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateReservation(CheckInReservationEditModel input)
        {
            ViewBag.ActiveSection = "CheckIn";
            var hotelId = GetSelectedHotelId();
            if (!hotelId.HasValue)
            {
                TempData["StaffError"] = "Vui lòng chọn chi nhánh trước khi thao tác.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                TempData["StaffError"] = "Thông tin chỉnh sửa không hợp lệ.";
                return RedirectToAction("Index");
            }

            if (input.CheckIn.HasValue && input.CheckOut.HasValue &&
                input.CheckIn.Value.Date >= input.CheckOut.Value.Date)
            {
                TempData["StaffError"] = "Ngày nhận phải trước ngày trả.";
                return RedirectToAction("Index");
            }

            var booking = db.THUEPHONGs
                .Include(b => b.PHONG)
                .FirstOrDefault(b => b.MaThue == input.BookingId && b.PHONG.MaKS == hotelId.Value);
            if (booking == null)
            {
                TempData["StaffError"] = "Không tìm thấy booking phù hợp.";
                return RedirectToAction("Index");
            }

            booking.NgayVao = input.CheckIn;
            booking.NgayTra = input.CheckOut;
            booking.DatCoc = input.Deposit;
            db.Entry(booking).State = EntityState.Modified;
            db.SaveChanges();

            TempData["StaffSuccess"] = "Thông tin booking đã được cập nhật.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeRoom(ChangeRoomInputModel input)
        {
            ViewBag.ActiveSection = "CheckIn";
            var hotelId = GetSelectedHotelId();
            if (!hotelId.HasValue)
            {
                TempData["StaffError"] = "Vui lòng chọn chi nhánh trước khi thao tác.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                TempData["StaffError"] = "Vui lòng chọn phòng mới để đổi.";
                return RedirectToAction("Index");
            }

            var booking = db.THUEPHONGs
                .Include(b => b.PHONG)
                .FirstOrDefault(b => b.MaThue == input.BookingId && b.PHONG.MaKS == hotelId.Value);
            if (booking == null)
            {
                TempData["StaffError"] = "Không tìm thấy booking phù hợp.";
                return RedirectToAction("Index");
            }

            var targetRoom = db.PHONGs
                .Include(r => r.THUEPHONGs)
                .FirstOrDefault(r => r.MaPhong == input.TargetRoomId && r.MaKS == hotelId.Value);
            if (targetRoom == null)
            {
                TempData["StaffError"] = "Không tìm thấy phòng để đổi.";
                return RedirectToAction("Index");
            }

            if (targetRoom.MaPhong == booking.MaPhong)
            {
                TempData["StaffError"] = "Vui lòng chọn phòng khác với phòng hiện tại.";
                return RedirectToAction("Index");
            }

            var now = DateTime.Now;
            var latestTargetBooking = targetRoom.THUEPHONGs
                .OrderByDescending(t => t.NgayDat ?? DateTime.MinValue)
                .FirstOrDefault(t => t.MaThue != booking.MaThue);
            if (latestTargetBooking != null)
            {
                var category = DetermineCategory(latestTargetBooking, now);
                if (category == RoomStatusCategory.Reserved || category == RoomStatusCategory.Occupied)
                {
                    TempData["StaffError"] = "Phòng mới hiện không sẵn sàng.";
                    return RedirectToAction("Index");
                }
            }

            var oldRoomName = booking.PHONG?.TenPhong ?? "phòng hiện tại";
            booking.MaPhong = targetRoom.MaPhong;
            db.Entry(booking).State = EntityState.Modified;
            db.SaveChanges();

            TempData["StaffSuccess"] = $"Khách đã được chuyển từ {oldRoomName} sang {targetRoom.TenPhong}.";
            return RedirectToAction("Index");
        }

        private CheckInViewModel BuildCheckInView(int? hotelId)
        {
            var model = new CheckInViewModel
            {
                SelectedHotelId = hotelId,
                SelectedHotelName = GetSelectedHotelName(),
                StaffName = GetCurrentStaffName()
            };

            if (!hotelId.HasValue)
            {
                return model;
            }

            var now = DateTime.Now;
            var reservedBookings = db.THUEPHONGs
                .Include(b => b.PHONG)
                .Include(b => b.CTTHUEPHONGs.Select(ct => ct.KHACHHANG))
                .Where(b => b.PHONG.MaKS == hotelId.Value)
                .OrderByDescending(b => b.NgayDat)
                .ToList()
                .Where(b => DetermineCategory(b, now) == RoomStatusCategory.Reserved)
                .ToList();

            model.BookingOptions = reservedBookings
                .Select(b => new SelectListItem
                {
                    Value = b.MaThue.ToString(),
                    Text = $"{b.PHONG.TenPhong} - {b.MaDatPhong}"
                })
                .ToList();

            model.ReservedBookings = reservedBookings
                .Select(b => new CheckInBookingInfo
                {
                    BookingId = b.MaThue,
                    RoomCode = b.PHONG.MaPhong.ToString(),
                    RoomName = b.PHONG.TenPhong,
                    GuestName = GetGuestName(b),
                    BookingCode = b.MaDatPhong,
                    BookingTime = b.NgayDat,
                    CheckIn = b.NgayVao,
                    CheckOut = b.NgayTra,
                    EstimatedTotal = CalculateEstimatedTotal(b),
                    Deposit = b.DatCoc ?? 0m,
                    RoomRate = b.PHONG.DGNgay,
                    Status = b.TrangThai ?? "Đã đặt"
                })
                .OrderBy(b => b.BookingTime ?? b.CheckIn ?? DateTime.Now)
                .ToList();

            var availableRooms = db.PHONGs
                .Include(p => p.THUEPHONGs)
                .Where(p => p.MaKS == hotelId.Value)
                .ToList()
                .Where(room =>
                {
                    var latestBooking = room.THUEPHONGs
                        .OrderByDescending(t => t.NgayDat ?? DateTime.MinValue)
                        .FirstOrDefault();
                    var category = DetermineCategory(latestBooking, now);
                    return category == RoomStatusCategory.Empty || category == RoomStatusCategory.CheckedOut;
                })
                .Select(room => new SelectListItem
                {
                    Value = room.MaPhong.ToString(),
                    Text = room.TenPhong
                })
                .ToList();

            model.AvailableRoomOptions = availableRooms;
            return model;
        }
    }
}
