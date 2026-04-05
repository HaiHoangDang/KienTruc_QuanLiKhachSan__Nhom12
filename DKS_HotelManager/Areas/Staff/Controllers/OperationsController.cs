using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Areas.Staff.ViewModels;
using DKS_HotelManager.Helpers;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Areas.Staff.Controllers
{
    [StaffAuthorize]
    public class OperationsController : StaffBaseController
    {

        public ActionResult Index(string customerSearch = "")
        {
            ViewBag.ActiveSection = "Overview";
            var hotelId = GetSelectedHotelId();
            var model = BuildOperationsModel(hotelId, customerSearch);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectHotel(int hotelId)
        {
            var hotel = db.KHACHSANs.Find(hotelId);
            if (hotel == null)
            {
                TempData["StaffError"] = "Chi nhánh không tồn tại.";
                return RedirectToAction("Index");
            }

            Session[HotelSessionKey] = hotelId;
            StaffActivityTracker.RecordEvent("Chọn chi nhánh lễ tân", GetCurrentStaffId(), GetCurrentStaffName(), hotelId, hotel.TenKS);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private StaffOperationsViewModel BuildOperationsModel(int? hotelId, string customerSearch)
        {
            var hotels = db.KHACHSANs
                .OrderBy(h => h.TenKS)
                .Select(h => new SelectListItem
                {
                    Value = h.MaKS.ToString(),
                    Text = h.TenKS,
                    Selected = hotelId.HasValue && h.MaKS == hotelId.Value
                })
                .ToList();

            var model = new StaffOperationsViewModel
            {
                SelectedHotelId = hotelId,
                SelectedHotelName = GetSelectedHotelName(),
                HotelOptions = hotels,
                StaffName = GetCurrentStaffName(),
                ServerTime = DateTime.Now,
                CustomerSearchQuery = customerSearch
            };

            model.BookingInput.CheckIn = DateTime.Today;
            model.BookingInput.CheckOut = DateTime.Today.AddDays(1);
            model.PaymentInput.PaymentMethod = "Tiền mặt";

            if (!hotelId.HasValue)
            {
                return model;
            }

            var rooms = db.PHONGs
                .Where(p => p.MaKS == hotelId.Value)
                .Include(p => p.THUEPHONGs.Select(t => t.CTTHUEPHONGs.Select(ct => ct.KHACHHANG)))
                .ToList();

            model.RoomOptions = rooms
                .OrderBy(p => p.MaPhong)
                .Select(p => new SelectListItem
                {
                    Value = p.MaPhong.ToString(),
                    Text = p.TenPhong
                })
                .ToList();

            var now = DateTime.Now;
            var roomTiles = new List<RoomStatusTile>();
            var summaryCounts = new Dictionary<RoomStatusCategory, int>
            {
                [RoomStatusCategory.Empty] = 0,
                [RoomStatusCategory.Reserved] = 0,
                [RoomStatusCategory.Occupied] = 0,
                [RoomStatusCategory.Paid] = 0,
                [RoomStatusCategory.CheckedOut] = 0
            };

            var checkInOptions = new List<SelectListItem>();
            var activeGuests = new List<GuestStayInfo>();

            foreach (var room in rooms.OrderBy(r => r.MaPhong))
            {
                var latestBooking = room.THUEPHONGs.OrderByDescending(t => t.NgayDat ?? DateTime.MinValue).FirstOrDefault();
                var tile = BuildRoomTile(room, latestBooking, now);
                roomTiles.Add(tile);

                if (summaryCounts.ContainsKey(tile.Category))
                {
                    summaryCounts[tile.Category]++;
                }

                if (tile.Category == RoomStatusCategory.Reserved && latestBooking != null)
                {
                    checkInOptions.Add(new SelectListItem
                    {
                        Value = latestBooking.MaThue.ToString(),
                    Text = room.TenPhong + " - " + (latestBooking.MaDatPhong ?? latestBooking.MaThue.ToString())
                    });
                }

                if (tile.Category == RoomStatusCategory.Occupied)
                {
                    activeGuests.Add(CreateGuestInfo(latestBooking, room));
                }
            }

            model.RoomTiles = roomTiles;
            model.StatusSummaries = new List<RoomStatusSummary>
            {
                new RoomStatusSummary { Label = "Tất cả", Category = RoomStatusCategory.All, Count = rooms.Count },
                new RoomStatusSummary { Label = "Trống", Category = RoomStatusCategory.Empty, Count = summaryCounts[RoomStatusCategory.Empty] },
                new RoomStatusSummary { Label = "Đã đặt trước", Category = RoomStatusCategory.Reserved, Count = summaryCounts[RoomStatusCategory.Reserved] },
                new RoomStatusSummary { Label = "Đang sử dụng", Category = RoomStatusCategory.Occupied, Count = summaryCounts[RoomStatusCategory.Occupied] },
                new RoomStatusSummary { Label = "Đã thanh toán", Category = RoomStatusCategory.Paid, Count = summaryCounts[RoomStatusCategory.Paid] },
                new RoomStatusSummary { Label = "Đã trả phòng", Category = RoomStatusCategory.CheckedOut, Count = summaryCounts[RoomStatusCategory.CheckedOut] }
            };

            model.CheckInBookingOptions = checkInOptions;
            model.ServiceBookingOptions = activeGuests.Select(g => new SelectListItem
            {
                Value = g.BookingId.ToString(),
                Text = g.RoomName + " - " + g.GuestName
            }).ToList();
            model.PaymentBookingOptions = model.ServiceBookingOptions;
            model.ActiveGuests = activeGuests;
            model.Services = db.DICHVUs
                .OrderBy(d => d.TenDV)
                .Select(d => new ServiceOption
                {
                    ServiceId = d.MaDV,
                    Name = d.TenDV,
                    Price = d.DGDV
                })
                .ToList();
            model.CustomerSearchResults = SearchCustomers(customerSearch).ToList();

            return model;
        }

        private RoomStatusTile BuildRoomTile(PHONG room, THUEPHONG booking, DateTime now)
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

        private GuestStayInfo CreateGuestInfo(THUEPHONG booking, PHONG room)
        {
            return new GuestStayInfo
            {
                RoomId = room.MaPhong,
                BookingId = booking.MaThue,
                RoomName = room.TenPhong,
                GuestName = GetGuestName(booking),
                BookingCode = booking.MaDatPhong,
                CheckIn = booking.NgayVao,
                CheckOut = booking.NgayTra,
                Status = booking.TrangThai
            };
        }

    }
}
