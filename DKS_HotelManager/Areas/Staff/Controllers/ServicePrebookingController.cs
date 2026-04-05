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
    public class ServicePrebookingController : StaffBaseController
    {
        public ActionResult Index()
        {
            ViewBag.ActiveSection = "ServicePrebooking";
            var model = BuildServicePrebookingModel(GetSelectedHotelId());
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Schedule(ServicePrebookingInputModel input)
        {
            ViewBag.ActiveSection = "ServicePrebooking";
            var hotelId = GetSelectedHotelId();
            if (!hotelId.HasValue)
            {
                TempData["StaffError"] = "Vui lòng chọn chi nhánh trước khi thao tác.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                TempData["StaffError"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction("Index");
            }

            if (input.ScheduledDate.HasValue && input.ScheduledTime.HasValue)
            {
                input.ScheduledAt = input.ScheduledDate.Value.Date + input.ScheduledTime.Value;
            }
            else
            {
                TempData["StaffError"] = "Vui lòng nhập đầy đủ ngày và giờ sử dụng dịch vụ.";
                return RedirectToAction("Index");
            }

            var booking = db.THUEPHONGs
                .Include(b => b.PHONG)
                .FirstOrDefault(b => b.MaThue == input.BookingId && b.PHONG.MaKS == hotelId.Value);
            if (booking == null || DetermineCategory(booking, DateTime.Now) != RoomStatusCategory.Occupied)
            {
                TempData["StaffError"] = "Chỉ có thể đặt dịch vụ khi phòng đang được sử dụng.";
                return RedirectToAction("Index");
            }

            var service = db.DICHVUs.Find(input.ServiceId);
            if (service == null)
            {
                TempData["StaffError"] = "Dịch vụ không tồn tại.";
                return RedirectToAction("Index");
            }

            var usage = db.SDDICHVUs.FirstOrDefault(s => s.MaThue == booking.MaThue && s.DV == service.MaDV);
            var quantity = Math.Max(1, input.Quantity);
            if (usage == null)
            {
                usage = new SDDICHVU
                {
                    MaThue = booking.MaThue,
                    DV = service.MaDV,
                    SoLuot = quantity
                };
                db.SDDICHVUs.Add(usage);
            }
            else
            {
                usage.SoLuot = (usage.SoLuot ?? 0) + quantity;
                db.Entry(usage).State = EntityState.Modified;
            }

            db.SaveChanges();

            var scheduledTime = input.ScheduledAt?.ToString("HH:mm dd/MM/yyyy") ?? "chưa xác định";
            StaffActivityTracker.RecordEvent(
                "Đặt trước dịch vụ",
                GetCurrentStaffId(),
                GetCurrentStaffName(),
                hotelId,
                GetSelectedHotelName(),
                string.Format("{0} x{1} (Phòng {2}) - sử dụng lúc {3}", service.TenDV, quantity, booking.PHONG.TenPhong, scheduledTime));

            TempData["StaffSuccess"] = $"Đã đặt trước {service.TenDV} cho phòng {booking.PHONG.TenPhong} lúc {scheduledTime}.";
            return RedirectToAction("Index");
        }

        private ServicePrebookingViewModel BuildServicePrebookingModel(int? hotelId)
        {
            var model = new ServicePrebookingViewModel
            {
                SelectedHotelId = hotelId,
                SelectedHotelName = GetSelectedHotelName(),
                StaffName = GetCurrentStaffName(),
                Services = db.DICHVUs
                    .OrderBy(d => d.TenDV)
                    .Select(d => new ServiceOption
                    {
                        ServiceId = d.MaDV,
                        Name = d.TenDV,
                        Price = d.DGDV
                    })
                    .ToList()
            };

            if (!hotelId.HasValue)
            {
                return model;
            }

            var options = db.THUEPHONGs
                .Include(b => b.PHONG)
                .Where(b => b.PHONG.MaKS == hotelId.Value)
                .ToList()
                .Where(b => DetermineCategory(b, DateTime.Now) == RoomStatusCategory.Occupied)
                .Select(b => new SelectListItem
                {
                    Value = b.MaThue.ToString(),
                    Text = $"{b.PHONG.TenPhong} - {b.MaDatPhong}"
                })
                .ToList();

            model.BookingOptions = options;
            return model;
        }
    }
}
