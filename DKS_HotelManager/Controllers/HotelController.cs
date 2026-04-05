using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Caching;
using DKS_HotelManager.Helpers;
using DKS_HotelManager.Models;
using DKS_HotelManager.Models.ViewModels;
using Newtonsoft.Json;

namespace DKS_HotelManager.Controllers
{
    public class HotelController : Controller
    {
        private const string PendingInvoiceSessionKey = "PendingInvoice";
        private const string PendingInvoiceCachePrefix = "PendingInvoice:";
        private DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();

        // GET: Hotel
        public ActionResult Index(string location = "", string sort = "", int guests = 0, DateTime? checkIn = null, DateTime? checkOut = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var hotelQuery = db.KHACHSANs
                .OrderBy(h => h.TenKS)
                .AsQueryable();

            var normalizedSearchTerm = !string.IsNullOrWhiteSpace(location) ? location.Trim() : null;
            if (!string.IsNullOrEmpty(normalizedSearchTerm))
            {
                var normalizedLower = normalizedSearchTerm.ToLower();
                hotelQuery = hotelQuery.Where(h => h.TenKS != null && h.TenKS.ToLower().Contains(normalizedLower));
            }

            var hotels = hotelQuery.ToList();

            var hotelIds = hotels.Select(h => h.MaKS).Distinct().ToList();

            var allRooms = db.PHONGs
                .Include(p => p.HINHANHs)
                .Where(p => hotelIds.Contains(p.MaKS))
                .ToList();

            var roomsByHotel = allRooms
                .GroupBy(p => p.MaKS)
                .ToDictionary(g => g.Key, g => g.ToList());

            var hotelsWithRooms = hotels
                .Where(h => roomsByHotel.TryGetValue(h.MaKS, out var hotelRooms) && hotelRooms.Any())
                .ToList();

            var hotelOptionList = hotelsWithRooms
                .Select(h => new HotelOptionViewModel
                {
                    HotelId = h.MaKS,
                    HotelName = h.TenKS,
                    Location = h.DiaDiem
                })
                .ToList();

            if (minPrice.HasValue || maxPrice.HasValue)
            {
                hotelsWithRooms = hotelsWithRooms
                    .Where(h =>
                    {
                        if (!roomsByHotel.TryGetValue(h.MaKS, out var hotelRooms))
                        {
                            return false;
                        }

                        return hotelRooms.Any(room =>
                            (!minPrice.HasValue || room.DGNgay >= minPrice.Value) &&
                            (!maxPrice.HasValue || room.DGNgay <= maxPrice.Value));
                    })
                    .ToList();
            }

            bool filterActive = minPrice.HasValue || maxPrice.HasValue;
            var hotelCards = hotelsWithRooms
                .Select(h =>
                {
                    roomsByHotel.TryGetValue(h.MaKS, out var rooms);
                    if (rooms == null)
                    {
                        rooms = new List<PHONG>();
                    }
                    var minRoomPrice = rooms.Any() ? rooms.Min(r => r.DGNgay) : (decimal?)null;
                    var maxRoomPrice = rooms.Any() ? rooms.Max(r => r.DGNgay) : (decimal?)null;

                    string thumbnail;
                    if (!string.IsNullOrWhiteSpace(h.HinhAnh))
                    {
                        thumbnail = h.HinhAnh.Trim();
                    }
                    else
                    {
                        var defaultImageIndex = ((h.MaKS - 1) % 25) + 1;
                        thumbnail = string.Format("ks{0:D2}.jpg", defaultImageIndex);
                    }

                    bool isHotelThumbnail = true;
                    var resolvedThumbnail = ResolveImagePath(thumbnail, isHotelImage: isHotelThumbnail);

                    var applicableRooms = filterActive
                        ? rooms.Where(room =>
                            (!minPrice.HasValue || room.DGNgay >= minPrice.Value) &&
                            (!maxPrice.HasValue || room.DGNgay <= maxPrice.Value))
                        : rooms;

                    var adjustedMin = applicableRooms.Any()
                        ? applicableRooms.Min(r => r.DGNgay)
                        : (decimal?)null;

                    return new HotelCardViewModel
                    {
                        HotelId = h.MaKS,
                        Name = h.TenKS,
                        Location = h.DiaDiem,
                        Description = string.IsNullOrWhiteSpace(h.MoTa) ? "Nơi nghỉ dưỡng cao cấp" : h.MoTa,
                        Thumbnail = resolvedThumbnail,
                        MinPrice = minRoomPrice,
                        MaxPrice = maxRoomPrice,
                        FilteredMinPrice = adjustedMin,
                        RoomCount = rooms.Count,
                        Badge = rooms.Any() ? $"{rooms.Count} loại phòng" : "Sắp mở cửa"
                    };
                })
                .ToList();

            switch (sort)
            {
                case "priceAsc":
                    hotelCards = hotelCards.OrderBy(c => c.MinPrice ?? decimal.MaxValue).ToList();
                    break;
                case "priceDesc":
                    hotelCards = hotelCards.OrderByDescending(c => c.MaxPrice ?? 0).ToList();
                    break;
                case "room":
                    hotelCards = hotelCards.OrderByDescending(c => c.RoomCount).ToList();
                    break;
            }

            var destinationsWithHotels = hotelsWithRooms
                .Where(h => !string.IsNullOrWhiteSpace(h.DiaDiem))
                .GroupBy(h => h.DiaDiem)
                .Select(g => new
                {
                    Location = g.Key,
                    FirstHotel = g.FirstOrDefault()
                })
                .OrderByDescending(x => x.FirstHotel != null ? 1 : 0)
                .Take(6)
                .ToList();

            var destinations = destinationsWithHotels
                .Select(x => x.Location)
                .ToList();

            var locationImageMap = destinationsWithHotels
                .Where(x => x.FirstHotel != null)
                .ToDictionary(
                    x => x.Location,
                    x => !string.IsNullOrWhiteSpace(x.FirstHotel.HinhAnh) 
                        ? ResolveImagePath(x.FirstHotel.HinhAnh.Trim(), isHotelImage: true)
                        : ResolveImagePath("ks01.jpg", isHotelImage: true)
                );

            ViewBag.LocationImageMap = locationImageMap;

            var roomTypes = db.LOAIPHONGs
                .OrderBy(l => l.TenLoai)
                .Select(l => l.TenLoai)
                .Where(l => l != null && l != "")
                .Take(6)
                .ToList();

            var pricePoints = hotelCards
                .SelectMany(c => new[] { c.MinPrice ?? 0, c.MaxPrice ?? 0 })
                .Where(v => v > 0)
                .ToList();

            var viewModel = new HotelSearchPageViewModel
            {
                HotelCards = hotelCards,
                Hotels = hotelOptionList,
                Destinations = destinations,
                RoomTypes = roomTypes,
                MinPrice = pricePoints.Any() ? pricePoints.Min() : 0,
                MaxPrice = pricePoints.Any() ? pricePoints.Max() : 0
            };

            ViewBag.Location = location;
            ViewBag.Guests = guests;
            ViewBag.CheckIn = checkIn;
            ViewBag.CheckOut = checkOut;
            ViewBag.CurrentSort = sort;
            ViewBag.FilterMinPrice = minPrice;
            ViewBag.FilterMaxPrice = maxPrice;

            return View(viewModel);
        }

        private string ResolveImagePath(string path, bool isHotelImage = false)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return isHotelImage ? "~/Images/ks01.jpg" : "~/Images/hotel_1.jpg";
            }

            var resolved = RoomImageHelper.ResolveRoomImagePath(path);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                return resolved;
            }

            return isHotelImage ? "~/Images/ks01.jpg" : "~/Images/hotel_1.jpg";
        }

        private string DetermineRoomImage(PHONG room)
        {
            if (room == null)
            {
                return null;
            }

            var image = room.HINHANHs?
                .Where(h => !string.IsNullOrEmpty(h.DuongDan))
                .OrderBy(h => h.MaHinh)
                .FirstOrDefault()?.DuongDan;

            if (!string.IsNullOrWhiteSpace(image))
            {
                return image;
            }

            if (!string.IsNullOrWhiteSpace(room.TenPhong))
            {
                return $"{room.TenPhong}.jpg";
            }

            return null;
        }

        public ActionResult Details(int? id,
            DateTime? checkIn = null,
            DateTime? checkOut = null,
            int guests = 1,
            string keyword = "",
            string roomType = "",
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? minCapacity = null)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            var hotelId = id.Value;
            var hotel = db.KHACHSANs.FirstOrDefault(h => h.MaKS == hotelId);
            if (hotel == null)
            {
                return HttpNotFound();
            }

            var rooms = db.PHONGs
                .Include(p => p.LOAIPHONG)
                .Include(p => p.HINHANHs)
                .Include(p => p.TIENICHes)
                .Where(p => p.MaKS == hotelId)
                .ToList();

            string heroImage = !string.IsNullOrWhiteSpace(hotel.HinhAnh) ? hotel.HinhAnh : "~/Images/hotel.jpg";
            heroImage = ResolveImagePath(heroImage, isHotelImage: true);

            var galleryImages = rooms
                .SelectMany(r => r.HINHANHs ?? new List<HINHANH>())
                .Select(h => h.DuongDan)
                .Where(src => !string.IsNullOrWhiteSpace(src))
                .Distinct()
                .Select(img => ResolveImagePath(img, isHotelImage: false))
                .Take(5)
                .ToList();

            if (!galleryImages.Any())
            {
                galleryImages.Add(heroImage);
            }

            var comments = db.BINHLUANs
                .Include(c => c.KHACHHANG)
                .Include(c => c.PHONG)
                .Where(c => c.MaKS == hotelId)
                .OrderByDescending(c => c.NgayBL)
                .Take(20)
                .ToList()
                .Select(c => new CommentViewModel
                {
                    CustomerName = !string.IsNullOrWhiteSpace(c.KHACHHANG?.TKH) ? c.KHACHHANG.TKH : "Khách ẩn danh",
                    RoomName = c.PHONG?.TenPhong,
                    Content = c.NoiDung,
                    CreatedAt = c.NgayBL
                })
                .ToList();

            var roomViewModels = rooms.Select(r =>
            {
                var firstImage = DetermineRoomImage(r);
                return new HotelRoomViewModel
                {
                    RoomId = r.MaPhong,
                    HotelId = r.MaKS,
                    Name = r.TenPhong,
                    RoomType = r.LOAIPHONG?.TenLoai ?? "Phòng",
                    Capacity = r.SucChua,
                    Area = r.DienTich ?? 0,
                    PricePerNight = r.DGNgay,
                    ImageUrl = ResolveImagePath(firstImage, isHotelImage: false),
                    Description = !string.IsNullOrEmpty(r.LOAIPHONG?.GhiChu) ? r.LOAIPHONG.GhiChu : "Phòng tiêu chuẩn thoải mái.",
                    Amenities = r.TIENICHes?.Select(t => t.TenTI).Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>()
                };
            }).ToList();

            var highlightRoom = roomViewModels.FirstOrDefault();
            var allRooms = db.PHONGs
                .Include(p => p.LOAIPHONG)
                .Include(p => p.HINHANHs)
                .Include(p => p.TIENICHes)
                .ToList();

            var allRoomViewModels = allRooms.Select(r =>
            {
                var firstImage = DetermineRoomImage(r);
                return new HotelRoomViewModel
                {
                    RoomId = r.MaPhong,
                    HotelId = r.MaKS,
                    Name = r.TenPhong,
                    RoomType = r.LOAIPHONG?.TenLoai ?? "Phòng",
                    Capacity = r.SucChua,
                    Area = r.DienTich ?? 0,
                    PricePerNight = r.DGNgay,
                    ImageUrl = ResolveImagePath(firstImage, isHotelImage: false),
                    Description = !string.IsNullOrEmpty(r.LOAIPHONG?.GhiChu) ? r.LOAIPHONG.GhiChu : "Phòng tiêu chuẩn thoải mái.",
                    Amenities = r.TIENICHes?.Select(t => t.TenTI).Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>()
                };
            }).ToList();

            var similarPricedRooms = new List<HotelRoomViewModel>();
            if (highlightRoom != null)
            {
                var highlightPrice = highlightRoom.PricePerNight;
                similarPricedRooms = allRoomViewModels
                    .Where(r => r.RoomId != highlightRoom.RoomId)
                    .OrderBy(r => Math.Abs(r.PricePerNight - highlightPrice))
                    .ThenBy(r => r.PricePerNight)
                    .Take(3)
                    .ToList();
            }

            IEnumerable<HotelRoomViewModel> filteredRooms = roomViewModels;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.Trim().ToLower();
                filteredRooms = filteredRooms.Where(r =>
                    (!string.IsNullOrEmpty(r.Name) && r.Name.ToLower().Contains(lowerKeyword)) ||
                    (!string.IsNullOrEmpty(r.Description) && r.Description.ToLower().Contains(lowerKeyword)));
            }

            if (!string.IsNullOrWhiteSpace(roomType))
            {
                filteredRooms = filteredRooms.Where(r => string.Equals(r.RoomType, roomType, StringComparison.OrdinalIgnoreCase));
            }

            if (minPrice.HasValue)
            {
                filteredRooms = filteredRooms.Where(r => r.PricePerNight >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                filteredRooms = filteredRooms.Where(r => r.PricePerNight <= maxPrice.Value);
            }

            if (minCapacity.HasValue)
            {
                filteredRooms = filteredRooms.Where(r => r.Capacity >= minCapacity.Value);
            }

            var filteredRoomList = filteredRooms.ToList();

            var roomTypes = roomViewModels.Select(r => r.RoomType)
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var services = db.DICHVUs
                .OrderBy(s => s.TenDV)
                .ToList()
                .Select(s => new ServiceOptionViewModel
                {
                    ServiceId = s.MaDV,
                    Name = s.TenDV,
                    Price = s.DGDV,
                    Description = $"Chi phí {s.DGDV:N0} VND / lượt"
                })
                .ToList();

            var roomOptions = roomViewModels.Select(r => new SelectListItem
            {
                Value = r.RoomId.ToString(),
                Text = string.Format("{0} - {1:N0} VND/dem", r.Name, r.PricePerNight)
            }).ToList();

            var bookingForm = new HotelBookingFormViewModel
            {
                HotelId = hotel.MaKS,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Guests = guests,
                RoomId = filteredRoomList.FirstOrDefault()?.RoomId
            };

            var viewModel = new HotelDetailViewModel
            {
                HotelId = hotel.MaKS,
                Name = hotel.TenKS,
                Location = hotel.DiaDiem,
                Description = hotel.MoTa,
                HeroImage = heroImage,
                Guests = guests,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Rooms = filteredRoomList,
                AllRooms = roomViewModels,
                RoomTypes = roomTypes,
                MinAvailablePrice = roomViewModels.Any() ? roomViewModels.Min(r => r.PricePerNight) : 0,
                MaxAvailablePrice = roomViewModels.Any() ? roomViewModels.Max(r => r.PricePerNight) : 0,
                Filter = new RoomFilterViewModel
                {
                    Keyword = keyword,
                    RoomType = roomType,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    MinCapacity = minCapacity
                },
                BookingForm = bookingForm,
                Services = services,
                RoomOptions = roomOptions,
                GalleryImages = galleryImages,
                Comments = comments
            };
            viewModel.SimilarRooms = similarPricedRooms;

            return View(viewModel);
        }

        [CustomerAuthorize]
        public ActionResult Book(int id, int? roomId = null, DateTime? checkIn = null, DateTime? checkOut = null, int guests = 1,
            string customerName = null, string phone = null, string address = null, string idNumber = null,
            string notes = null, int[] selectedServices = null)
        {
            var bookingForm = new HotelBookingFormViewModel
            {
                HotelId = id,
                RoomId = roomId,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Guests = guests > 0 ? guests : 1,
                CustomerName = customerName,
                Phone = phone,
                Address = address,
                IdNumber = idNumber,
                Notes = notes,
                SelectedServices = selectedServices?.Where(s => s > 0).Distinct().ToList() ?? new List<int>()
            };

            var viewModel = BuildHotelBookingPageViewModel(id, bookingForm);
            if (viewModel == null)
            {
                return HttpNotFound();
            }

            return View(viewModel);
        }

        private HotelBookingPageViewModel BuildHotelBookingPageViewModel(int hotelId, HotelBookingFormViewModel bookingForm)
        {
            var hotel = db.KHACHSANs.FirstOrDefault(h => h.MaKS == hotelId);
            if (hotel == null)
            {
                return null;
            }

            var rooms = db.PHONGs
                .Include(p => p.LOAIPHONG)
                .Include(p => p.HINHANHs)
                .Include(p => p.TIENICHes)
                .Where(p => p.MaKS == hotelId)
                .ToList();

            var heroImage = !string.IsNullOrWhiteSpace(hotel.HinhAnh) ? hotel.HinhAnh : "~/Images/hotel.jpg";
            heroImage = ResolveImagePath(heroImage, isHotelImage: true);

            var galleryImages = rooms
                .SelectMany(r => r.HINHANHs ?? new List<HINHANH>())
                .Select(h => h.DuongDan)
                .Where(src => !string.IsNullOrWhiteSpace(src))
                .Distinct()
                .Select(img => ResolveImagePath(img, isHotelImage: false))
                .Take(12)
                .ToList();

            if (!galleryImages.Any())
            {
                galleryImages.Add(heroImage);
            }

            var roomViewModels = rooms.Select(r => new HotelRoomViewModel
            {
                RoomId = r.MaPhong,
                Name = r.TenPhong,
                RoomType = r.LOAIPHONG?.TenLoai ?? "Phòng",
                Capacity = r.SucChua,
                Area = r.DienTich ?? 0,
                PricePerNight = r.DGNgay,
                ImageUrl = ResolveImagePath(r.HINHANHs?.Where(img => !string.IsNullOrEmpty(img.DuongDan)).Select(img => img.DuongDan).FirstOrDefault(), isHotelImage: false),
                Description = !string.IsNullOrEmpty(r.LOAIPHONG?.GhiChu) ? r.LOAIPHONG.GhiChu : "Phòng tiêu chuẩn thoải mái.",
                Amenities = r.TIENICHes?.Select(a => a.TenTI).Where(a => !string.IsNullOrEmpty(a)).ToList() ?? new List<string>()
            }).ToList();

            var services = db.DICHVUs
                .OrderBy(s => s.TenDV)
                .ToList()
                .Select(s => new ServiceOptionViewModel
                {
                    ServiceId = s.MaDV,
                    Name = s.TenDV,
                    Price = s.DGDV,
                    Description = $"Chi phi {s.DGDV:N0} VND / luot"
                })
                .ToList();

            var form = bookingForm ?? new HotelBookingFormViewModel();
            form.HotelId = hotel.MaKS;
            if (form.Guests <= 0)
            {
                form.Guests = 1;
            }

            var selectedRoomId = form.RoomId;
            if (!selectedRoomId.HasValue)
            {
                selectedRoomId = roomViewModels.FirstOrDefault()?.RoomId;
                form.RoomId = selectedRoomId;
            }

            form.SelectedServices = (form.SelectedServices ?? Enumerable.Empty<int>())
                .Where(s => s > 0)
                .Distinct()
                .ToList();

            var roomOptions = roomViewModels.Select(r => new SelectListItem
            {
                Value = r.RoomId.ToString(),
                Text = string.Format("{0} - {1:N0} VND/dem", r.Name, r.PricePerNight),
                Selected = selectedRoomId.HasValue && selectedRoomId.Value == r.RoomId
            }).ToList();

            return new HotelBookingPageViewModel
            {
                HotelId = hotel.MaKS,
                Name = hotel.TenKS,
                Location = hotel.DiaDiem,
                Description = hotel.MoTa,
                HeroImage = heroImage,
                BookingForm = form,
                RoomOptions = roomOptions,
                Services = services,
                Guests = form.Guests,
                GalleryImages = galleryImages
            };
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomerAuthorize]
        public ActionResult ReviewBooking([Bind(Prefix = "BookingForm")] HotelBookingFormViewModel bookingForm)
        {
            var bookingView = BuildHotelBookingPageViewModel(bookingForm.HotelId, bookingForm);
            if (bookingView == null)
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                return View("Book", bookingView);
            }

            if (!bookingForm.CheckIn.HasValue || !bookingForm.CheckOut.HasValue ||
                bookingForm.CheckIn.Value.Date >= bookingForm.CheckOut.Value.Date)
            {
                ModelState.AddModelError(string.Empty, "Ngày trả phòng phải sau ngày nhận phòng.");
                return View("Book", bookingView);
            }

            var hotel = db.KHACHSANs.FirstOrDefault(h => h.MaKS == bookingForm.HotelId);
            if (hotel == null)
            {
                return HttpNotFound();
            }

            var room = db.PHONGs.FirstOrDefault(r => r.MaPhong == bookingForm.RoomId && r.MaKS == bookingForm.HotelId);
            if (room == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy phòng phù hợp với yêu cầu đặt.");
                return View("Book", bookingView);
            }

            var checkInDate = bookingForm.CheckIn.Value.Date;
            var checkOutDate = bookingForm.CheckOut.Value.Date;
            if (HasRoomConflict(room.MaPhong, checkInDate, checkOutDate))
            {
                ModelState.AddModelError(string.Empty, "Phòng đang được đặt trong khoảng thời gian này.");
                return View("Book", bookingView);
            }

            var invoiceModel = BuildBookingInvoice(bookingForm, room, hotel);
            return View("ReviewBooking", invoiceModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomerAuthorize]
        public ActionResult BookRoom([Bind(Prefix = "BookingForm")] HotelBookingFormViewModel bookingForm)
        {
            if (!ModelState.IsValid)
            {
                TempData["BookingError"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m)));
                return RedirectToBookingForm(bookingForm);
            }

            var buildResult = TryBuildBookingInvoice(bookingForm);
            if (!buildResult.Success)
            {
                TempData["BookingError"] = buildResult.ErrorMessage ?? "Ngày trả phòng phải sau ngày nhận phòng.";
                return RedirectToBookingForm(bookingForm);
            }

            var persistResult = PersistBooking(bookingForm, buildResult.Invoice, false);
            if (!persistResult.Success)
            {
                TempData["BookingError"] = persistResult.ErrorMessage ?? "Không thể xử lý đặt phòng.";
                return RedirectToBookingForm(bookingForm);
            }

            TempData["BookingSuccess"] = $"Đặt phòng thành công! Đã nhận {buildResult.Invoice.DepositAmount:N0}₫ đặt cọc.";
            return RedirectToBookingForm(bookingForm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomerAuthorize]
        public ActionResult PayWithVnPay([Bind(Prefix = "BookingForm")] HotelBookingFormViewModel bookingForm)
        {
            var buildResult = TryBuildBookingInvoice(bookingForm);
            if (!buildResult.Success)
            {
                TempData["BookingError"] = buildResult.ErrorMessage ?? "Không thể khởi tạo thanh toán.";
                return RedirectToBookingForm(bookingForm);
            }

            var invoice = buildResult.Invoice;

            var vnpUrl = ConfigurationManager.AppSettings["VnpPayUrl"];
            var returnUrl = ConfigurationManager.AppSettings["VnpReturnUrl"];
            var tmnCode = ConfigurationManager.AppSettings["VnpTmnCode"];
            var hashSecret = SecurityConfig.GetSecret("VnpHashSecret", "DKS_VNP_HASH_SECRET");

            if (string.IsNullOrWhiteSpace(vnpUrl) || string.IsNullOrWhiteSpace(returnUrl) ||
                string.IsNullOrWhiteSpace(tmnCode) || string.IsNullOrWhiteSpace(hashSecret))
            {
                TempData["BookingError"] = "VNPAY configuration is missing.";
                SecurityAuditLogger.Log("payment", "vnpay_missing_config", "error");
                return RedirectToBookingForm(bookingForm);
            }

            var pay = new PayLib();
            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmnCode);
            pay.AddRequestData("vnp_Amount", ((long)Math.Round(invoice.DepositAmount * 100, 0, MidpointRounding.AwayFromZero)).ToString());
            pay.AddRequestData("vnp_BankCode", string.Empty);
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress());
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", BuildVnPayOrderInfo(invoice));
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            var txnRef = Guid.NewGuid().ToString("N").Substring(0, 10);
            pay.AddRequestData("vnp_TxnRef", txnRef);

            var paymentUrl = pay.CreateRequestUrl(vnpUrl, hashSecret);

            StorePendingInvoice(txnRef, invoice);
            SecurityAuditLogger.Log("payment", "vnpay_redirect_created", "info", new Dictionary<string, object>
            {
                { "customerId", Session["KhachHangId"]?.ToString() ?? "guest" },
                { "amount", invoice.DepositAmount },
                { "txnRef", txnRef },
                { "ip", SecurityAuditLogger.GetClientIp(Request) }
            });

            return Redirect(paymentUrl);
        }

        [HttpGet]
        public ActionResult VnPayReturn()
        {
            return HandleVnPayCallback(Request.QueryString);
        }

        [HttpGet]
        public ActionResult PaymentConfirm()
        {
            return HandleVnPayCallback(Request.QueryString);
        }

        private ActionResult HandleVnPayCallback(NameValueCollection query)
        {
            var pay = new PayLib();
            var hashSecret = SecurityConfig.GetSecret("VnpHashSecret", "DKS_VNP_HASH_SECRET");

            foreach (string key in query)
            {
                if (!string.IsNullOrWhiteSpace(key) && key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                {
                    pay.AddResponseData(key, query[key]);
                }
            }

            if (string.IsNullOrWhiteSpace(hashSecret))
            {
                TempData["BookingError"] = "VNPAY verification secret is missing.";
                SecurityAuditLogger.Log("payment", "vnpay_callback_missing_secret", "error");
                return RedirectToAction("Index");
            }

            var signatureValid = pay.ValidateSignature(hashSecret);
            var responseCode = pay.GetResponseData("vnp_ResponseCode");
            var txnRef = pay.GetResponseData("vnp_TxnRef");
            if (!signatureValid || !string.Equals(responseCode, "00", StringComparison.Ordinal))
            {
                SecurityAuditLogger.Log("payment", "vnpay_callback_invalid", "warning", new Dictionary<string, object>
                {
                    { "signatureValid", signatureValid },
                    { "responseCode", responseCode },
                    { "txnRef", txnRef },
                    { "ip", SecurityAuditLogger.GetClientIp(Request) }
                });
                TempData["BookingError"] = "VNPAY transaction is invalid or failed.";
                ClearPendingInvoice(txnRef);
                return RedirectToAction("Index");
            }

            var orderInfo = pay.GetResponseData("vnp_OrderInfo");
            var invoice = GetPendingInvoice(txnRef) ?? BuildInvoiceFromOrderInfo(orderInfo);

            if (invoice == null)
            {
                TempData["BookingError"] = "Cannot find booking details for payment confirmation.";
                ClearPendingInvoice(txnRef);
                return RedirectToAction("Index");
            }

            var persistResult = PersistBooking(invoice.BookingForm, invoice, true);
            ClearPendingInvoice(txnRef);

            if (!persistResult.Success)
            {
                SecurityAuditLogger.Log("payment", "vnpay_callback_persist_failed", "error", new Dictionary<string, object>
                {
                    { "txnRef", txnRef },
                    { "error", persistResult.ErrorMessage ?? "unknown" }
                });
                ViewBag.PaymentMessage = persistResult.ErrorMessage ?? "Payment completed but booking persistence failed.";
                return View("ReviewBooking", invoice);
            }

            SecurityAuditLogger.Log("payment", "vnpay_callback_success", "info", new Dictionary<string, object>
            {
                { "txnRef", txnRef },
                { "transactionNo", pay.GetResponseData("vnp_TransactionNo") },
                { "amount", pay.GetResponseData("vnp_Amount") }
            });

            ViewBag.PaymentSuccess = true;
            ViewBag.PaymentMessage = "Thanh toan VNPAY thanh cong.";
            ViewBag.VnpTransactionNo = pay.GetResponseData("vnp_TransactionNo");
            ViewBag.VnpTxnRef = txnRef;
            ViewBag.VnpAmount = pay.GetResponseData("vnp_Amount");
            ViewBag.VnpPayDate = pay.GetResponseData("vnp_PayDate");
            return View("PaymentConfirm", invoice);
        }

        private decimal TongTien()
        {
            // TODO: replace with real total calculation
            return 0M;
        }

        private HotelBookingInvoiceViewModel BuildBookingInvoice(HotelBookingFormViewModel bookingForm, PHONG room, KHACHSAN hotel)
        {
            var checkInDate = bookingForm.CheckIn.Value.Date;
            var checkOutDate = bookingForm.CheckOut.Value.Date;
            var nights = (checkOutDate - checkInDate).Days;
            var roomTotal = room.DGNgay * nights;
            var selectedServiceIds = (bookingForm.SelectedServices ?? new List<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            var services = db.DICHVUs.Where(s => selectedServiceIds.Contains(s.MaDV)).ToList();
            var servicesTotal = services.Sum(s => s.DGDV);
            var depositAmount = Math.Round((roomTotal + servicesTotal) * 0.3m, 0, MidpointRounding.AwayFromZero);

            bookingForm.SelectedServices = selectedServiceIds;
            bookingForm.Deposit = depositAmount;

            return new HotelBookingInvoiceViewModel
            {
                HotelId = hotel.MaKS,
                HotelName = hotel.TenKS,
                HotelLocation = hotel.DiaDiem,
                RoomName = room.TenPhong,
                RoomPricePerNight = room.DGNgay,
                Nights = nights,
                RoomTotal = roomTotal,
                ServicesTotal = servicesTotal,
                DepositAmount = depositAmount,
                Services = services.Select(s => new ServiceInvoiceItem
                {
                    ServiceId = s.MaDV,
                    Name = s.TenDV,
                    Price = s.DGDV
                }).ToList(),
                BookingForm = bookingForm
            };
        }

        private (bool Success, string ErrorMessage, HotelBookingInvoiceViewModel Invoice) TryBuildBookingInvoice(HotelBookingFormViewModel bookingForm)
        {
            if (!bookingForm.CheckIn.HasValue || !bookingForm.CheckOut.HasValue || bookingForm.CheckIn.Value.Date >= bookingForm.CheckOut.Value.Date)
            {
                return (false, "Ngày trả phòng phải sau ngày nhận phòng.", null);
            }

            if (!bookingForm.RoomId.HasValue)
            {
                return (false, "Phòng không hợp lệ.", null);
            }

            var hotel = db.KHACHSANs.FirstOrDefault(h => h.MaKS == bookingForm.HotelId);
            if (hotel == null)
            {
                return (false, "Không tìm thấy khách sạn.", null);
            }

            var room = db.PHONGs.FirstOrDefault(r => r.MaPhong == bookingForm.RoomId.Value && r.MaKS == bookingForm.HotelId);
            if (room == null)
            {
                return (false, "Không tìm thấy phòng phù hợp với yêu cầu đặt.", null);
            }

            var checkInDate = bookingForm.CheckIn.Value.Date;
            var checkOutDate = bookingForm.CheckOut.Value.Date;
            if (HasRoomConflict(room.MaPhong, checkInDate, checkOutDate))
            {
                return (false, "Phòng đang được đặt trong khoảng thời gian này.", null);
            }

            var invoice = BuildBookingInvoice(bookingForm, room, hotel);
            return (true, null, invoice);
        }

        private (bool Success, string ErrorMessage, THUEPHONG Booking) PersistBooking(HotelBookingFormViewModel bookingForm, HotelBookingInvoiceViewModel invoiceModel, bool depositPaid)
        {
            if (!bookingForm.RoomId.HasValue)
            {
                return (false, "Phòng không hợp lệ.", null);
            }

            if (!bookingForm.CheckIn.HasValue || !bookingForm.CheckOut.HasValue)
            {
                return (false, "Ngày nhận/trả phòng không hợp lệ.", null);
            }

            var employee = db.NHANVIENs.FirstOrDefault();
            if (employee == null)
            {
                return (false, "Chưa có nhân viên nào được cấu hình tiếp nhận đặt phòng.", null);
            }

            int? sessionCustomerId = null;
            if (Session["KhachHangId"] is int customerId)
            {
                sessionCustomerId = customerId;
            }
            KHACHHANG customer = null;

            if (sessionCustomerId.HasValue)
            {
                customer = db.KHACHHANGs.FirstOrDefault(c => c.MKH == sessionCustomerId.Value);
            }

            if (customer == null && !string.IsNullOrWhiteSpace(bookingForm.Phone))
            {
                var normalizedPhone = bookingForm.Phone.Trim();
                customer = db.KHACHHANGs.FirstOrDefault(c => c.SDT == normalizedPhone);
            }

            if (customer == null && !string.IsNullOrWhiteSpace(bookingForm.Email))
            {
                var normalizedEmail = bookingForm.Email.Trim();
                customer = db.KHACHHANGs.FirstOrDefault(c => c.Email == normalizedEmail);
            }

            if (customer == null && !string.IsNullOrWhiteSpace(bookingForm.IdNumber))
            {
                var normalizedIdNumber = bookingForm.IdNumber.Trim();
                customer = db.KHACHHANGs.FirstOrDefault(c => c.CMND_CCCD == normalizedIdNumber);
            }

            if (customer == null)
            {
                customer = new KHACHHANG
                {
                    MKH = CustomerIdHelper.GetNextCustomerId(db),
                    TKH = bookingForm.CustomerName,
                    DiaChi = bookingForm.Address,
                    SDT = string.IsNullOrWhiteSpace(bookingForm.Phone) ? null : bookingForm.Phone.Trim(),
                    CMND_CCCD = string.IsNullOrWhiteSpace(bookingForm.IdNumber) ? null : bookingForm.IdNumber.Trim(),
                    Email = string.IsNullOrWhiteSpace(bookingForm.Email) ? null : bookingForm.Email.Trim()
                };
                db.KHACHHANGs.Add(customer);
            }
            else
            {
                customer.TKH = bookingForm.CustomerName;
                customer.DiaChi = bookingForm.Address;
                customer.SDT = string.IsNullOrWhiteSpace(bookingForm.Phone) ? customer.SDT : bookingForm.Phone.Trim();
                customer.CMND_CCCD = string.IsNullOrWhiteSpace(bookingForm.IdNumber) ? customer.CMND_CCCD : bookingForm.IdNumber.Trim();
                if (!string.IsNullOrWhiteSpace(bookingForm.Email))
                {
                    customer.Email = bookingForm.Email.Trim();
                }
                db.Entry(customer).State = EntityState.Modified;
            }

            db.SaveChanges();

            Session["KhachHang"] = customer;
            Session["KhachHangId"] = customer.MKH;
            Session["KhachHangTen"] = customer.TKH;

            var booking = new THUEPHONG
            {
                MaThue = BookingIdHelper.GetNextBookingId(db),
                MaNV = employee.MaNV,
                MaPhong = bookingForm.RoomId.Value,
                NgayDat = DateTime.Now,
                NgayVao = bookingForm.CheckIn,
                NgayTra = bookingForm.CheckOut,
                DatCoc = invoiceModel.DepositAmount,
                MaDatPhong = $"DP{DateTime.Now:yyyyMMddHHmmss}",
                TrangThai = depositPaid ? "Đã đặt cọc" : "Đang đặt phòng"
            };

            db.THUEPHONGs.Add(booking);
            db.SaveChanges();

            if (depositPaid)
            {
                var payment = new THANHTOAN
                {
                    MaTT = PaymentHelper.GetNextThanhToanId(db),
                    MaThue = booking.MaThue,
                    HinhThucTT = "VNPAY",
                    ThanhTien = invoiceModel.DepositAmount,
                    NgayTT = DateTime.Now
                };
                db.THANHTOANs.Add(payment);
            }

            db.CTTHUEPHONGs.Add(new CTTHUEPHONG
            {
                MaThue = booking.MaThue,
                KHACH = customer.MKH,
                VaiTro = "KhĂ¡ch chĂ­nh"
            });

            if (bookingForm.SelectedServices != null && bookingForm.SelectedServices.Any())
            {
                foreach (var serviceId in bookingForm.SelectedServices.Distinct())
                {
                    if (db.DICHVUs.Any(s => s.MaDV == serviceId))
                    {
                        db.SDDICHVUs.Add(new SDDICHVU
                        {
                            MaThue = booking.MaThue,
                            DV = serviceId,
                            SoLuot = 1
                        });
                    }
                }
            }

            db.SaveChanges();

            var recipientEmail = !string.IsNullOrWhiteSpace(bookingForm.Email)
                ? bookingForm.Email.Trim()
                : customer.Email;

            if (!string.IsNullOrWhiteSpace(recipientEmail))
            {
                try
                {
                    var invoiceBody = BuildInvoiceEmailBody(invoiceModel);
                    MailService.SendMail(recipientEmail, $"Hóa đơn đặt phòng {invoiceModel.HotelName}", invoiceBody);
                }
                catch
                {
                    // Ignore email failures
                }
            }

            return (true, null, booking);
        }

        private bool HasRoomConflict(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var checkInDate = checkIn.Date;
            var checkOutDate = checkOut.Date;
            return db.THUEPHONGs
                .Where(b => b.MaPhong == roomId && b.NgayVao.HasValue && b.NgayTra.HasValue)
                .Any(b =>
                    DbFunctions.TruncateTime(b.NgayVao) < checkOutDate &&
                    DbFunctions.TruncateTime(b.NgayTra) > checkInDate);
        }

        private HotelBookingInvoiceViewModel BuildInvoiceFromOrderInfo(string orderInfo)
        {
            if (string.IsNullOrWhiteSpace(orderInfo)) return null;
            var parts = orderInfo.Split('|');
            if (parts.Length < 5) return null;
            if (!int.TryParse(parts[0], out var hotelId)) return null;
            if (!int.TryParse(parts[1], out var roomId)) return null;
            var checkIn = TryParseDate(parts[2]);
            var checkOut = TryParseDate(parts[3]);
            if (!decimal.TryParse(parts[4], NumberStyles.Number, CultureInfo.InvariantCulture, out var depositAmount)) return null;

            var bookingForm = new HotelBookingFormViewModel
            {
                HotelId = hotelId,
                RoomId = roomId,
                CheckIn = checkIn,
                CheckOut = checkOut,
                SelectedServices = new List<int>()
            };
            bookingForm.Deposit = depositAmount;

            var hotel = db.KHACHSANs.FirstOrDefault(h => h.MaKS == hotelId);
            var room = db.PHONGs.FirstOrDefault(r => r.MaPhong == roomId && r.MaKS == hotelId);
            if (hotel == null || room == null)
            {
                return null;
            }

            return BuildBookingInvoice(bookingForm, room, hotel);
        }

        private string BuildVnPayOrderInfo(HotelBookingInvoiceViewModel invoice)
        {
            var checkIn = invoice.BookingForm.CheckIn?.ToString("yyyyMMddHHmmss") ?? string.Empty;
            var checkOut = invoice.BookingForm.CheckOut?.ToString("yyyyMMddHHmmss") ?? string.Empty;
            var roomId = invoice.BookingForm.RoomId?.ToString() ?? string.Empty;
            return $"{invoice.HotelId}|{roomId}|{checkIn}|{checkOut}|{invoice.DepositAmount.ToString(CultureInfo.InvariantCulture)}";
        }

        private void StorePendingInvoice(string txnRef, HotelBookingInvoiceViewModel invoice)
        {
            if (invoice == null)
            {
                return;
            }

            Session[PendingInvoiceSessionKey] = invoice;

            if (!string.IsNullOrWhiteSpace(txnRef))
            {
                HttpRuntime.Cache.Insert(
                    PendingInvoiceCachePrefix + txnRef,
                    invoice,
                    null,
                    DateTime.UtcNow.AddMinutes(30),
                    Cache.NoSlidingExpiration);
            }
        }

        private HotelBookingInvoiceViewModel GetPendingInvoice(string txnRef)
        {
            var invoice = Session[PendingInvoiceSessionKey] as HotelBookingInvoiceViewModel;
            if (invoice != null)
            {
                return invoice;
            }

            if (string.IsNullOrWhiteSpace(txnRef))
            {
                return null;
            }

            return HttpRuntime.Cache[PendingInvoiceCachePrefix + txnRef] as HotelBookingInvoiceViewModel;
        }

        private void ClearPendingInvoice(string txnRef)
        {
            Session.Remove(PendingInvoiceSessionKey);

            if (!string.IsNullOrWhiteSpace(txnRef))
            {
                HttpRuntime.Cache.Remove(PendingInvoiceCachePrefix + txnRef);
            }
        }

        private DateTime? TryParseDate(string value)
        {
            if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return null;
        }

        private string BuildInvoiceEmailBody(HotelBookingInvoiceViewModel invoice)
        {
            var checkInDate = invoice.BookingForm.CheckIn?.ToString("dd/MM/yyyy") ?? "-";
            var checkOutDate = invoice.BookingForm.CheckOut?.ToString("dd/MM/yyyy") ?? "-";
            var servicesList = invoice.Services.Any()
                ? "<ul>" + string.Concat(invoice.Services.Select(s => $"<li>{HttpUtility.HtmlEncode(s.Name)}: {s.Price:N0}đ</li>")) + "</ul>"
                : "<p>Không có dịch vụ bổ sung.</p>";

            var customerName = HttpUtility.HtmlEncode(invoice.BookingForm.CustomerName ?? "Quý khách");

            return $@"<p>Xin chào {customerName},</p>
<p>Cảm ơn bạn đã đặt phòng tại<strong>{HttpUtility.HtmlEncode(invoice.HotelName)}</strong>.</p>
<p><strong>Thời gian:</strong> {checkInDate} - {checkOutDate} ({invoice.Nights} đêm)</p>
<p><strong>Phòng:</strong> {HttpUtility.HtmlEncode(invoice.RoomName)}</p>
<p><strong>Địa điểm:</strong> {HttpUtility.HtmlEncode(invoice.HotelLocation)}</p>
<p><strong>Chi tiết dịch vụ:</strong></p>
{servicesList}
<ul>
    <li>Giá phòng: {invoice.RoomTotal:N0}₫</li>
    <li>Dịch vụ: {invoice.ServicesTotal:N0}₫</li>
    <li><strong>Tổng cộng:</strong> {invoice.GrandTotal:N0}₫</li>
    <li><strong>Đặt cọc 30%:</strong> {invoice.DepositAmount:N0}₫</li>
</ul>
<p>Vui lòng giữ email này để kiểm tra lại thông tin. Mọi thắc mắc xin liên hệ lễ tân.</p>
<p>Trân trọng,<br />Đội ngũ DKS Hotel Manager</p>";
        }

        private ActionResult RedirectToBookingForm(HotelBookingFormViewModel bookingForm)
        {
            return RedirectToAction("Book", new
            {
                id = bookingForm.HotelId,
                roomId = bookingForm.RoomId,
                checkIn = bookingForm.CheckIn,
                checkOut = bookingForm.CheckOut,
                guests = bookingForm.Guests,
                email = bookingForm.Email,
                customerName = bookingForm.CustomerName,
                phone = bookingForm.Phone,
                address = bookingForm.Address,
                idNumber = bookingForm.IdNumber,
                notes = bookingForm.Notes,
                selectedServices = bookingForm.SelectedServices
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




