using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using DKS_HotelManager.Models;
using DKS_HotelManager.Models.ViewModels;
using DKS_HotelManager.Helpers;


namespace DKS_HotelManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();

        public ActionResult Index()
        {

            string ResolveHotelImagePath(string imagePath)
            {
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    return "~/Images/ks01.jpg";
                }

                if (imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase) || imagePath.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    return imagePath;
                }

                if (imagePath.StartsWith("~") || imagePath.StartsWith("/"))
                {
                    return imagePath;
                }

                return $"~/Images/{imagePath.TrimStart('/')}";
            }

            var hotelCount = db.KHACHSANs.Count();
            var roomCount = db.PHONGs.Count();
            var bookingCount = db.THUEPHONGs.Count();
            var customerCount = db.KHACHHANGs.Count();

            var hotelOptions = db.KHACHSANs
                .OrderBy(h => h.TenKS)
                .Take(8)
                .Select(h => new HotelOptionViewModel
                {
                    HotelId = h.MaKS,
                    HotelName = h.TenKS,
                    Location = h.DiaDiem
                })
                .ToList();

            var destinationImages = new[]
            {
                "~/Images/hotel_3.jpg",
                "~/Images/hotel_4.jpg",
                "~/Images/hotel_5.jpg",
                "~/Images/hotel_1.jpg",
                "~/Images/hotel.jpg"
            };

            var cityGroups = db.KHACHSANs
                .Where(h => !string.IsNullOrEmpty(h.DiaDiem))
                .GroupBy(h => h.DiaDiem)
                .OrderByDescending(g => g.Count())
                .Take(destinationImages.Length)
                .ToList();

            var destinations = cityGroups
                .Select((group, index) => new DestinationCardViewModel
                {
                    Title = group.Key,
                    SubTitle = group.FirstOrDefault()?.TenKS ?? group.Key,
                    ImageUrl = destinationImages[index % destinationImages.Length]
                })
                .ToList();

            if (!destinations.Any())
            {
                destinations.AddRange(destinationImages.Take(3).Select((image, index) => new DestinationCardViewModel
                {
                    Title = $"City {index + 1}",
                    SubTitle = "Luxury destination",
                    ImageUrl = image
                }));
            }

            var bookingGroups = db.THUEPHONGs
                .GroupBy(p => p.MaPhong)
                .Select(g => new { RoomId = g.Key, Count = g.Count() })
                .ToDictionary(g => g.RoomId, g => g.Count);

            var hotelBookingCounts = db.THUEPHONGs
                .Include(tp => tp.PHONG)
                .Where(tp => tp.PHONG != null)
                .GroupBy(tp => tp.PHONG.MaKS)
                .Select(g => new { HotelId = g.Key, Count = g.Count() })
                .ToDictionary(g => g.HotelId, g => g.Count);

            var hotelRoomStats = db.PHONGs
                .GroupBy(p => p.MaKS)
                .Select(g => new
                {
                    HotelId = g.Key,
                    MinPrice = g.Min(p => p.DGNgay),
                    MaxPrice = g.Max(p => p.DGNgay),
                    RoomCount = g.Count()
                })
                .ToDictionary(g => g.HotelId, g => g);

            var highlightedRooms = db.PHONGs
                .Include(p => p.KHACHSAN)
                .Include(p => p.LOAIPHONG)
                .Include(p => p.HINHANHs)
                .Include(p => p.TIENICHes)
                .Include(p => p.TRANGTHAI_PHONG)
                .OrderByDescending(p => p.DGNgay)
                .Take(8)
                .ToList();

            var featuredRooms = highlightedRooms.Select(room =>
            {
                var latestStatus = room.TRANGTHAI_PHONG?
                    .OrderByDescending(status => status.NgayCapNhat)
                    .FirstOrDefault()?.TrangThai;

                bookingGroups.TryGetValue(room.MaPhong, out var bookedTimes);

                var roomImage = room.HINHANHs?
                    .OrderBy(img => img.MaHinh)
                    .FirstOrDefault()?.DuongDan;
                if (string.IsNullOrWhiteSpace(roomImage) && !string.IsNullOrWhiteSpace(room.TenPhong))
                {
                    roomImage = $"{room.TenPhong}.jpg";
                }

                return new RoomCardViewModel
                {
                    RoomId = room.MaPhong,
                    HotelId = room.MaKS,
                    RoomName = room.TenPhong,
                    HotelName = room.KHACHSAN?.TenKS ?? "Luxury Hotel",
                    Location = room.KHACHSAN?.DiaDiem ?? "Vietnam",
                    RoomType = room.LOAIPHONG?.TenLoai ?? "Standard",
                    PricePerNight = room.DGNgay,
                    Capacity = room.SucChua,
                    Area = room.DienTich ?? 0,
                    ImageUrl = RoomImageHelper.ResolveRoomImagePath(roomImage) ?? "~/Images/hotel_1.jpg",
                    Status = latestStatus,
                    AvailabilityBadge = string.IsNullOrEmpty(latestStatus) ? "Trống" : latestStatus,
                    BookingCount = bookedTimes,
                    Amenities = room.TIENICHes?
                        .Select(t => t.TenTI)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .Distinct()
                        .Take(3)
                        .ToList() ?? new List<string>()
                };
            }).ToList();

            var hotelRanking = db.KHACHSANs
                .OrderBy(h => h.TenKS)
                .ToList()
                .Select(h => new
                {
                    Hotel = h,
                    BookingCount = hotelBookingCounts.TryGetValue(h.MaKS, out var count) ? count : 0,
                    RoomStats = hotelRoomStats.TryGetValue(h.MaKS, out var roomStats) ? roomStats : null
                })
                .OrderByDescending(x => x.BookingCount)
                .ThenByDescending(x => x.RoomStats?.RoomCount ?? 0)
                .ThenBy(x => x.Hotel.TenKS)
                .Take(5)
                .ToList();

            var featuredHotels = hotelRanking
                .Select(entry =>
                {
                    var defaultImageIndex = ((entry.Hotel.MaKS - 1) % 25) + 1;
                    var hotelImage = !string.IsNullOrWhiteSpace(entry.Hotel.HinhAnh)
                        ? entry.Hotel.HinhAnh.Trim()
                        : $"ks{defaultImageIndex:D2}.jpg";
                    return new HotelCardViewModel
                    {
                        HotelId = entry.Hotel.MaKS,
                        Name = entry.Hotel.TenKS,
                        Location = entry.Hotel.DiaDiem,
                        Description = string.IsNullOrWhiteSpace(entry.Hotel.MoTa) ? "Trải nghiệm nghỉ dưỡng cao cấp" : entry.Hotel.MoTa,
                        Thumbnail = ResolveHotelImagePath(hotelImage),
                        MinPrice = entry.RoomStats?.MinPrice,
                        MaxPrice = entry.RoomStats?.MaxPrice,
                        RoomCount = entry.RoomStats?.RoomCount ?? 0,
                        Badge = entry.BookingCount > 0 ? $"{entry.BookingCount} lượt đặt" : "Đang cập nhật"
                    };
                })
                .ToList();

            if (!featuredHotels.Any())
            {
                featuredHotels.Add(new HotelCardViewModel
                {
                    HotelId = 0,
                    Name = "Chuỗi Luxury",
                    Location = "Vietnam",
                    Description = "Đang cập nhật khách sạn nổi bật",
                    Thumbnail = "~/Images/hotel.jpg",
                    Badge = "Sắp ra mắt",
                    RoomCount = 0
                });
            }

            var stats = new List<StatCardViewModel>
            {
                new StatCardViewModel { LabelKey = "HotelsCount", Value = hotelCount.ToString() },
                new StatCardViewModel { LabelKey = "RoomsCount", Value = roomCount.ToString() },
                new StatCardViewModel { LabelKey = "Bookings", Value = bookingCount.ToString() },
                new StatCardViewModel { LabelKey = "SatisfactionRate", Value = "98%" }
            };

            var viewModel = new HomePageViewModel
            {
                FeaturedRooms = featuredRooms,
                Destinations = destinations,
                Stats = stats,
                FeaturedHotels = featuredHotels,
                HotelOptions = hotelOptions
            };

            return View(viewModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [CustomerAuthorize]
        public ActionResult Comment()
        {
            var hotels = db.KHACHSANs
                .OrderBy(h => h.TenKS)
                .Select(h => new SelectListItem
                {
                    Value = h.MaKS.ToString(),
                    Text = h.TenKS
                })
                .ToList();

            hotels.Insert(0, new SelectListItem { Value = "", Text = "Chọn khách sạn (tùy chọn)" });

            var comments = db.BINHLUANs
                .Include(c => c.KHACHSAN)
                .Include(c => c.KHACHHANG)
                .OrderByDescending(c => c.NgayBL)
                .Take(30)
                .ToList()
                .Select(c => new ContactCommentItemViewModel
                {
                    CustomerName = !string.IsNullOrWhiteSpace(c.KHACHHANG?.TKH) ? c.KHACHHANG.TKH : "Khách",
                    HotelName = string.IsNullOrWhiteSpace(c.KHACHSAN?.TenKS) ? "Chuỗi Luxury" : c.KHACHSAN.TenKS,
                    Content = c.NoiDung,
                    CreatedAt = c.NgayBL
                })
                .ToList();

            var viewModel = new ContactCommentPageViewModel
            {
                HotelOptions = hotels,
                Comments = comments
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomerAuthorize]
        public ActionResult Comment(ContactCommentFormViewModel form)
        {
            var hotels = db.KHACHSANs
                .OrderBy(h => h.TenKS)
                .Select(h => new SelectListItem
                {
                    Value = h.MaKS.ToString(),
                    Text = h.TenKS
                })
                .ToList();
            hotels.Insert(0, new SelectListItem { Value = "", Text = "Chọn khách sạn (tùy chọn)" });

            int? customerId = null;
            if (Session["KhachHangId"] != null && int.TryParse(Session["KhachHangId"].ToString(), out var parsed))
            {
                customerId = parsed;
            }

            if (!customerId.HasValue)
            {
                TempData["AccountError"] = "Vui lòng đăng nhập trước khi gửi bình luận.";
                return RedirectToAction("Login", "Authentication", new { returnUrl = Url.Action("Comment", "Home") });
            }

            if (!ModelState.IsValid)
            {
                var commentsList = db.BINHLUANs
                    .Include(c => c.KHACHSAN)
                    .Include(c => c.KHACHHANG)
                    .OrderByDescending(c => c.NgayBL)
                    .Take(30)
                    .ToList()
                    .Select(c => new ContactCommentItemViewModel
                    {
                        CustomerName = !string.IsNullOrWhiteSpace(c.KHACHHANG?.TKH) ? c.KHACHHANG.TKH : "Khách",
                        HotelName = string.IsNullOrWhiteSpace(c.KHACHSAN?.TenKS) ? "Chuỗi Luxury" : c.KHACHSAN.TenKS,
                        Content = c.NoiDung,
                        CreatedAt = c.NgayBL
                    })
                    .ToList();

                var vm = new ContactCommentPageViewModel
                {
                    HotelOptions = hotels,
                    Form = form,
                    Comments = commentsList
                };
                return View(vm);
            }

            var comment = new BINHLUAN
            {
                MaKS = form.HotelId,
                MaPhong = null,
                MKH = customerId.Value,
                NoiDung = form.Content,
                NgayBL = DateTime.Now
            };
            db.BINHLUANs.Add(comment);
            db.SaveChanges();

            TempData["AccountSuccess"] = "Cảm ơn bạn đã gửi bình luận!";
            return RedirectToAction("Comment");
        }

        [CustomerAuthorize]
        public ActionResult Contact()
        {
            ViewBag.Title = LanguageHelper.Translate("Contact");
            return View(new ContactFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomerAuthorize]
        public ActionResult Contact(ContactFormViewModel form)
        {
            ViewBag.Title = LanguageHelper.Translate("Contact");

            if (!ModelState.IsValid)
            {
                return View(form);
            }

            var fullName = HttpUtility.HtmlEncode(form.FullName?.Trim() ?? "Quý khách");
            var email = form.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
                return View(form);
            }
            var phone = HttpUtility.HtmlEncode(form.Phone?.Trim() ?? "Không cung cấp");
            var subject = HttpUtility.HtmlEncode(form.Subject?.Trim() ?? "Liên hệ");
            var message = HttpUtility.HtmlEncode(form.Message?.Trim() ?? string.Empty).Replace("\n", "<br />");

            var body = $@"<p>Xin chào {fullName},</p>
<p>Quản lý đã nhận được báo cáo từ bạn. Dưới đây là thông tin bạn đã gửi:</p>
<ul>
    <li><strong>Họ tên:</strong> {fullName}</li>
    <li><strong>Email:</strong> {HttpUtility.HtmlEncode(email)}</li>
    <li><strong>Số điện thoại:</strong> {phone}</li>
    <li><strong>Chủ đề:</strong> {subject}</li>
</ul>
<p><strong>Nội dung:</strong></p>
<p>{message}</p>
<p>Chúng tôi sẽ phản hồi sớm nhất cho bạn.</p>";

            try
            {
                MailService.SendMail(email, "Xác nhận liên hệ - DKS Hotel", body);
                TempData["ContactSuccess"] = "Đã gửi liên hệ thành công. Vui lòng kiểm tra email để xác nhận.";
                return RedirectToAction("Contact");
            }
            catch
            {
                TempData["ContactError"] = "Không thể gửi email xác nhận lúc này. Vui lòng thử lại sau.";
                return View(form);
            }
        }

        public ActionResult Service()
        {
            return View();
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
