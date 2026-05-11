using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DKS_HotelManager.Models;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
namespace DKS_HotelManager.Controllers
{
    public class ChatBotController : Controller
    {
        private static readonly string AiServiceUrl = "http://127.0.0.1:8000/chat";
        private readonly DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();

        [HttpPost]
        public async Task<ActionResult> Ask(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return Json(new { reply = "Nội dung câu hỏi đang trống." });

            // ── Xây context DB thực tế ─────────────────────────────────────
            string dbContext = BuildDatabaseContext(message);

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(120);

                    // Gửi cả câu hỏi lẫn context DB sang AI service
                    var payload = new
                    {
                        question = message,
                        context_type = "summary",
                        db_context = dbContext   // AI service sẽ nhận thêm field này
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(AiServiceUrl, content);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Log để debug (xem trong Output của Visual Studio)
                    System.Diagnostics.Debug.WriteLine($"[ChatBot] Status: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"[ChatBot] Response: {responseBody}");

                    if (!response.IsSuccessStatusCode)
                    {
                        // Trả về lỗi thực sự để dễ debug
                        return Json(new { reply = $"AI service lỗi ({(int)response.StatusCode}): {responseBody}" });
                    }

                    dynamic result = JsonConvert.DeserializeObject(responseBody);
                    string answer = result?.answer ?? "Không có câu trả lời.";
                    // ===== AI yêu cầu đặt phòng =====
                    if (answer.Contains("[BOOKING_REQUEST]"))
                    {
                        var roomMatch = Regex.Match(answer, @"ROOM_NAME=(.*)");
                        var checkinMatch = Regex.Match(answer, @"CHECKIN=(.*)");
                        var checkoutMatch = Regex.Match(answer, @"CHECKOUT=(.*)");

                        if (roomMatch.Success)
                        {
                            string roomName = roomMatch.Groups[1].Value.Trim();

                            var room = db.PHONGs
                                .FirstOrDefault(p => p.TenPhong == roomName);

                            if (room == null)
                            {
                                return Json(new
                                {
                                    reply = $"Không tìm thấy phòng {roomName}."
                                });
                            }

                            DateTime? checkin = null;
                            DateTime? checkout = null;

                            if (checkinMatch.Success)
                                checkin = DateTime.Parse(checkinMatch.Groups[1].Value.Trim());

                            if (checkoutMatch.Success)
                                checkout = DateTime.Parse(checkoutMatch.Groups[1].Value.Trim());

                            // ===== GỌI BOOKING SERVICE =====
                            using (var bookingClient = new HttpClient())
                            {
                                bookingClient.Timeout = TimeSpan.FromSeconds(30);

                                // JWT token từ session/login
                                string token = Session["JWT"]?.ToString();

                                if (!string.IsNullOrEmpty(token))
                                {
                                    bookingClient.DefaultRequestHeaders.Authorization =
                                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                                }

                                var bookingPayload = new
                                {
                                    maPhong = room.MaPhong,
                                    ngayVao = checkin,
                                    ngayTra = checkout
                                };

                                var bookingJson = JsonConvert.SerializeObject(bookingPayload);

                                var bookingContent = new StringContent(
                                    bookingJson,
                                    Encoding.UTF8,
                                    "application/json"
                                );

                                var bookingResponse = await bookingClient.PostAsync(
                                    "http://localhost:5050/api/booking",
                                    bookingContent
                                );

                                var bookingResult = await bookingResponse.Content.ReadAsStringAsync();

                                if (bookingResponse.IsSuccessStatusCode)
                                {
                                    return Json(new
                                    {
                                        reply = $"Đặt phòng {roomName} thành công! 🎉"
                                    });
                                }
                                else
                                {
                                    return Json(new
                                    {
                                        reply = $"Không thể đặt phòng: {bookingResult}"
                                    });
                                }
                            }
                        }
                    }
                    return Json(new { reply = answer });
                }
            }
            catch (TaskCanceledException)
            {
                return Json(new { reply = "AI service mất quá nhiều thời gian phản hồi (timeout). Vui lòng thử lại." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatBot] Exception: {ex.Message}");
                return Json(new { reply = $"Lỗi kết nối: {ex.Message}" });
            }
        }

        /// <summary>
        /// Truy vấn DB thực tế và xây dựng context cho AI.
        /// AI sẽ dùng dữ liệu này để trả lời phòng trống, đặt phòng, v.v.
        /// </summary>
        private string BuildDatabaseContext(string userMessage)
        {
            try
            {
                var lower = (userMessage ?? "").ToLowerInvariant();
                var sb = new StringBuilder();
                sb.AppendLine("=== DỮ LIỆU THỰC TẾ TỪ DATABASE (cập nhật realtime) ===\n");
                // ===== THÔNG TIN ĐĂNG NHẬP =====
                bool isLoggedIn = Session["KhachHang"] != null;

                sb.AppendLine($"IS_LOGGED_IN={(isLoggedIn ? "true" : "false")}");

                if (isLoggedIn)
                {
                    var kh = Session["KhachHang"] as KHACHHANG;

                    if (kh != null)
                    {
                        sb.AppendLine($"CURRENT_CUSTOMER_NAME={kh.TKH}");
                        sb.AppendLine($"CURRENT_CUSTOMER_EMAIL={kh.Email}");
                    }
                }
                // ── 1. Danh sách khách sạn ──────────────────────────────────
                var hotels = db.KHACHSANs
                    .Select(k => new { k.MaKS, k.TenKS, k.DiaDiem, k.MoTa })
                    .ToList();

                sb.AppendLine("## KHÁCH SẠN TRONG HỆ THỐNG:");
                foreach (var h in hotels)
                    sb.AppendLine($"- [{h.MaKS}] {h.TenKS} | Địa điểm: {h.DiaDiem} | Mô tả: {h.MoTa}");

                // ── 2. Phòng trống (không có booking active) ────────────────
                var now = DateTime.Now;

                var bookedRoomIds = db.THUEPHONGs
                    .Where(t =>
                        t.TrangThai != null &&
                        !t.TrangThai.Contains("hủy") &&
                        !t.TrangThai.Contains("trả phòng") &&
                        (
                            (t.NgayVao <= now && t.NgayTra >= now) ||
                            t.TrangThai.Contains("đang") ||
                            t.TrangThai.Contains("đặt")
                        )
                    )
                    .Select(t => t.MaPhong)
                    .Distinct()
                    .ToList();

                var availableRooms = db.PHONGs
    .Include("LOAIPHONG")
    .Where(p => !bookedRoomIds.Contains(p.MaPhong))
    .Select(p => new
    {
        p.MaPhong,
        TenPhong = p.TenPhong,  
        LoaiPhong = p.LOAIPHONG.TenLoai,
        p.DGNgay,
        KhachSan = p.KHACHSAN.TenKS,
        DiaDiem = p.KHACHSAN.DiaDiem
    })
    .OrderBy(p => p.DGNgay)
    .ToList();

                sb.AppendLine($"\n## PHÒNG TRỐNG HIỆN TẠI ({availableRooms.Count} phòng):");
                foreach (var r in availableRooms.Take(20))
                {
                    sb.AppendLine(
                        $"- {r.TenPhong} | " +
                        $"Loại: {r.LoaiPhong} | " +
                        $"Giá: {r.DGNgay:N0}₫/đêm | " +
                        $"Khách sạn: {r.KhachSan} ({r.DiaDiem})"
                    );
                }

                if (availableRooms.Count > 20)
                    sb.AppendLine($"  ... và {availableRooms.Count - 20} phòng trống khác.");

                // ── 3. Thống kê nhanh ────────────────────────────────────────
                var totalRooms = db.PHONGs.Count();
                sb.AppendLine($"\n## THỐNG KÊ:");
                sb.AppendLine($"- Tổng số phòng: {totalRooms}");
                sb.AppendLine($"- Phòng trống: {availableRooms.Count}");
                sb.AppendLine($"- Phòng đang sử dụng/đặt trước: {totalRooms - availableRooms.Count}");

                // Giá min/max
                if (availableRooms.Any())
                {
                    sb.AppendLine($"- Giá phòng trống: từ {availableRooms.Min(r => r.DGNgay):N0}₫ đến {availableRooms.Max(r => r.DGNgay):N0}₫/đêm");
                }

                // ── 4. Booking gần đây (nếu user hỏi về đặt phòng) ──────────
                if (lower.Contains("đặt phòng") || lower.Contains("booking") ||
                    lower.Contains("đang ở") || lower.Contains("lịch sử"))
                {
                    var recentBookings = db.THUEPHONGs
                        .Include("PHONG.KHACHSAN")
                        .Include("KHACHHANG")
                        .OrderByDescending(t => t.NgayDat)
                        .Take(10)
                        .ToList();

                    foreach (var b in recentBookings)
                    {
                        var room = b.PHONG?.TenPhong ?? "?";
                        var hotel = b.PHONG?.KHACHSAN?.TenKS ?? "?";

                        sb.AppendLine(
                            $"- Phòng {room} tại {hotel} | " +
                            $"Vào: {b.NgayVao:dd/MM/yyyy} | " +
                            $"Ra: {b.NgayTra:dd/MM/yyyy} | " +
                            $"Trạng thái: {b.TrangThai}"
                        );
                    }
                }

                // ── 5. Dịch vụ ───────────────────────────────────────────────
                if (lower.Contains("dịch vụ") || lower.Contains("service"))
                {
                    var services = db.DICHVUs.OrderBy(d => d.TenDV).ToList();
                    sb.AppendLine($"\n## DỊCH VỤ ({services.Count} dịch vụ):");
                    foreach (var s in services)
                        sb.AppendLine($"- {s.TenDV}: {s.DGDV:N0}₫");
                }

                // ── 6. Hướng dẫn đặt phòng cho AI ───────────────────────────
                sb.AppendLine("\n=== HƯỚNG DẪN CHO AI ===\n" +
    "Khi người dùng muốn ĐẶT PHÒNG, hãy:\n" +
    "1. Hỏi họ muốn ở khách sạn nào, loại phòng nào, ngày vào/ra\n" +
    "2. Dựa vào danh sách PHÒNG TRỐNG ở trên để gợi ý phòng phù hợp\n" +
    "3. Xác nhận thông tin rồi hướng dẫn họ đặt qua giao diện hoặc gọi API đặt phòng\n" +
    "4. KHÔNG được tự đặt phòng - chỉ tư vấn và hướng dẫn\n" +
    "Khi người dùng hỏi PHÒNG TRỐNG, dùng danh sách ở trên.\n");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"[Lỗi đọc database: {ex.Message}]";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}


//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Globalization;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web.Mvc;
//using DKS_HotelManager.Helpers;
//using DKS_HotelManager.Models;
//using Newtonsoft.Json;

//namespace DKS_HotelManager.Controllers
//{
//    public class ChatBotController : Controller
//    {
//        private static readonly string FastApiUrl = "http://127.0.0.1:8000/chat";

//        [HttpPost]

//        public async Task<ActionResult> Ask(string message)
//        {
//            if (string.IsNullOrWhiteSpace(message))
//            {
//                return Json(new { reply = "Nội dung câu hỏi đang trống." });
//            }

//            try
//            {
//                using (var client = new HttpClient())
//                {
//                    var apiUrl = "http://127.0.0.1:8000/chat/";
//                    var payload = new { message = message };
//                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
//                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

//                    var response = await client.PostAsync(apiUrl, content);
//                    var responseBody = await response.Content.ReadAsStringAsync();

//                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
//                    {
//                        return Json(new { reply = "Không tìm thấy API chatbot (404)." });
//                    }

//                    if (!response.IsSuccessStatusCode)
//                    {
//                        return Json(new { reply = "Hệ thống chatbot hiện chưa phản hồi đúng." });
//                    }

//                    return Content(responseBody, "application/json");
//                }
//            }
//            catch
//            {
//                return Json(new { reply = "Đã xảy ra lỗi khi kết nối tới chatbot." });
//            }
//        }

//        public class ChatBotRequest
//        {
//            public string Message { get; set; }
//        }
//    }
//}





/*private readonly DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();
private const int MaxMessageLength = 500;
private static readonly TimeSpan ChatRateWindow = TimeSpan.FromMinutes(1);
private static readonly TimeSpan ChatBlockDuration = TimeSpan.FromMinutes(3);
private const int ChatMaxRequestsPerWindow = 20;

[HttpPost]
[AllowAnonymous]
public JsonResult Ask(string message)
{
    var clientIp = SecurityAuditLogger.GetClientIp(Request);
    var customerId = Session?["KhachHangId"]?.ToString() ?? "guest";
    var throttleKey = $"chat:{clientIp}:{customerId}";
    var throttleResult = SecurityRateLimiter.CheckAndIncrement(
        throttleKey,
        ChatMaxRequestsPerWindow,
        ChatRateWindow,
        ChatBlockDuration);

    if (!throttleResult.IsAllowed)
    {
        Response.StatusCode = 429;
        SecurityAuditLogger.Log("chatbot", "rate_limit_block", "warning", new Dictionary<string, object>
        {
            { "ip", clientIp },
            { "customerId", customerId },
            { "retryAfterSeconds", throttleResult.RetryAfterSeconds }
        });
        return Json(new
        {
            success = false,
            reply = $"Ban da gui qua nhanh. Vui long thu lai sau {throttleResult.RetryAfterSeconds} giay."
        });
    }

    var safeMessage = SanitizeMessage(message);
    if (safeMessage.Length == 0)
    {
        return Json(new { success = false, reply = "Vui long nhap noi dung can ho tro." });
    }

    if (safeMessage.Length > MaxMessageLength)
    {
        Response.StatusCode = 400;
        SecurityAuditLogger.Log("chatbot", "message_too_long", "warning", new Dictionary<string, object>
        {
            { "ip", clientIp },
            { "customerId", customerId },
            { "messageLength", safeMessage.Length }
        });
        return Json(new
        {
            success = false,
            reply = $"Tin nhan vuot qua gioi han {MaxMessageLength} ky tu. Vui long rut gon noi dung."
        });
    }

    try
    {
        var reply = BuildReply(safeMessage);
        SecurityAuditLogger.Log("chatbot", "ask_success", "info", new Dictionary<string, object>
        {
            { "ip", clientIp },
            { "customerId", customerId },
            { "messageLength", safeMessage.Length }
        });
        return Json(new { success = true, reply });
    }
    catch (Exception ex)
    {
        SecurityAuditLogger.Log("chatbot", "ask_error", "error", new Dictionary<string, object>
        {
            { "ip", clientIp },
            { "customerId", customerId },
            { "error", ex.Message }
        });
        Response.StatusCode = 500;
        return Json(new { success = false, reply = "He thong chatbot tam thoi gian doan, vui long thu lai sau." });
    }
}

private string SanitizeMessage(string message)
{
    if (string.IsNullOrWhiteSpace(message))
    {
        return string.Empty;
    }

    var cleaned = new string(message.Where(c => !char.IsControl(c) || c == '\n' || c == '\r' || c == '\t').ToArray());
    return cleaned.Trim();
}

private string BuildReply(string message)
{
    var normalized = (message ?? string.Empty).Trim();
    if (normalized.Length == 0)
    {
        return "Xin hãy nhập câu hỏi để mình có thể hỗ trợ bạn nhé.";
    }

    var lower = normalized.ToLowerInvariant();
    var searchKey = NormalizeForComparison(lower);

    var hotelSummaries = db.KHACHSANs
        .Select(k => new HotelSummary
        {
            HotelId = k.MaKS,
            Name = k.TenKS,
            Location = k.DiaDiem,
            RoomCount = k.PHONGs.Count(),
            MinPrice = k.PHONGs.Select(p => (decimal?)p.DGNgay).Min() ?? 0m,
            MaxPrice = k.PHONGs.Select(p => (decimal?)p.DGNgay).Max() ?? 0m
        })
        .ToList();
    var cheapestHotel = hotelSummaries.Where(h => h.MinPrice > 0).OrderBy(h => h.MinPrice).FirstOrDefault();
    var mostRoomsHotel = hotelSummaries.OrderByDescending(h => h.RoomCount).FirstOrDefault();
    var bookingCounts = db.THUEPHONGs
        .Include("PHONG.KHACHSAN")
        .Where(t => t.PHONG != null && t.PHONG.KHACHSAN != null)
        .GroupBy(t => t.PHONG.KHACHSAN.MaKS)
        .Select(g => new
        {
            HotelId = g.Key,
            HotelName = g.FirstOrDefault().PHONG.KHACHSAN.TenKS,
            Location = g.FirstOrDefault().PHONG.KHACHSAN.DiaDiem,
            Count = g.Count()
        })
        .OrderByDescending(x => x.Count)
        .ToList();
    var mostBookedHotel = bookingCounts.FirstOrDefault();

    var hotels = db.KHACHSANs
        .Select(k => new
        {
            k.TenKS,
            k.DiaDiem,
            Description = k.MoTa ?? "Khách sạn cao cấp",
            RoomCount = k.PHONGs.Count()
        })
        .ToList();

    var services = db.DICHVUs
        .OrderBy(d => d.TenDV)
        .Take(3)
        .ToList();

    var priceRange = db.PHONGs.Any()
        ? (min: db.PHONGs.Min(p => p.DGNgay), max: db.PHONGs.Max(p => p.DGNgay))
        : (min: 0m, max: 0m);

    var now = DateTime.Now;
    var totalRooms = db.PHONGs.Count();

    var latestBookingsPerRoom = db.THUEPHONGs
        .Include(t => t.PHONG)
        .GroupBy(t => t.MaPhong)
        .Select(g => g
            .OrderByDescending(b => b.NgayVao ?? b.NgayDat)
            .FirstOrDefault())
        .ToList();

    var busyRoomCount = latestBookingsPerRoom
        .Where(b => IsRoomBusy(b, now))
        .Select(b => b.MaPhong)
        .Distinct()
        .Count();

    var availableRooms = Math.Max(totalRooms - busyRoomCount, 0);

    var replies = new List<string>();
    var locationHandled = TryBuildLocationReply(searchKey, hotelSummaries, out var locationReply);

    if (ContainsAny(lower, "khách sạn đang hoạt động", "liệt kê các khách sạn", "liệt kê khách sạn", "đang hoạt động"))
    {
        var activeHotels = hotelSummaries.Where(h => h.RoomCount > 0).ToList();
        if (!activeHotels.Any())
        {
            replies.Add("Chuỗi DKS hiện đang chuẩn bị mở thêm cơ sở, bạn vui lòng kiểm tra lại sau nhé.");
        }
        else
        {
            var examples = activeHotels.Take(4)
                .Select(h => $"{h.Name}{(string.IsNullOrEmpty(h.Location) ? "" : $" ({h.Location})")}")
                .ToArray();
            replies.Add($"Chuỗi đang vận hành {activeHotels.Count} khách sạn với các cơ sở tiêu biểu như {string.Join(", ", examples)}.");
        }
    }

    var allServices = db.DICHVUs.OrderBy(d => d.TenDV).ToList();
    var totalServices = allServices.Count;
    var cheapestService = allServices.OrderBy(s => s.DGDV).FirstOrDefault();
    var mostExpensiveService = allServices.OrderByDescending(s => s.DGDV).FirstOrDefault();

    if (ContainsAny(lower, "dịch vụ", "bao nhiêu dịch vụ", "có bao nhiêu dịch vụ"))
    {
        if (totalServices > 0)
        {
            var minServicePrice = allServices.Min(s => s.DGDV);
            var maxServicePrice = allServices.Max(s => s.DGDV);
            replies.Add(
                $"Hiện tại hệ thống có {totalServices} dịch vụ. " +
                $"Giá dịch vụ dao động từ khoảng {FormatPrice(minServicePrice)} đến {FormatPrice(maxServicePrice)}."
            );
        }
        else
        {
            replies.Add("Hiện tại hệ thống chưa ghi nhận dịch vụ nào, bạn vui lòng thử lại sau.");
        }
    }

    var regionHandled = TryBuildRegionReply(searchKey, hotelSummaries, out var regionReply);

    if (locationHandled && !string.IsNullOrWhiteSpace(locationReply))
    {
        replies.Add(locationReply);
    }

    if (regionHandled && !string.IsNullOrWhiteSpace(regionReply))
    {
        replies.Add(regionReply);
    }

    if (ContainsAny(lower, "rẻ nhất", "giá thấp nhất", "giá rẻ nhất") && cheapestHotel != null)
    {
        replies.Add($"Khách sạn có giá thấp nhất là {cheapestHotel.Name} tại {cheapestHotel.Location ?? "địa điểm chưa xác định"}, giá từ {FormatPrice(cheapestHotel.MinPrice)} mỗi đêm.");
    }

    if (ContainsAny(lower, "khách sạn", "chuỗi", "tên", "địa điểm", "mô tả"))
    {
        var locations = hotels.Select(h => h.DiaDiem).Distinct().Where(d => !string.IsNullOrEmpty(d)).ToList();
        var locationSummary = locations.Any()
            ? $"tọa lạc tại {string.Join(", ", locations.Take(3))}"
            : "phân bổ rộng khắp các điểm đến trọng yếu";

        var hotelList = string.Join(" | ",
            hotels.Select(h => $"{h.TenKS} ({h.RoomCount} phòng)")
                  .Take(3));

        replies.Add($"Chuỗi DKS hiện có {hotels.Count} khách sạn sang trọng {locationSummary}. Những cơ sở vừa được cập nhật: {hotelList}.");
    }

    if (ContainsAny(lower, "phòng trống", "phòng còn trống", "còn phòng", "tình trạng phòng", "đặt phòng"))
    {
        replies.Add(
            $"Hiện toàn hệ thống có tổng cộng {totalRooms} phòng, trong đó khoảng {availableRooms} phòng đang trống " +
            $"(không bao gồm những phòng đã đặt trước, đã đặt cọc hoặc đang sử dụng). " +
            $"Số phòng đang được giữ cho khách (đang ở, đã đặt trước hoặc đã thanh toán): {busyRoomCount}."
        );
    }

    if (ContainsAny(lower, "dịch vụ", "giá dịch vụ", "tên dịch vụ"))
    {
        if (allServices.Any())
        {
            var featuredServices = allServices
                .OrderByDescending(s => s.DGDV)
                .Take(3)
                .Select(s => $"{s.TenDV}: {s.DGDV.ToString("N0")}₫")
                .ToList();

            replies.Add("Một số dịch vụ tiêu biểu: " + string.Join(" / ", featuredServices) + ".");
        }
        else
        {
            replies.Add("Hiện chưa có dịch vụ nào được cập nhật trong hệ thống.");
        }
    }

    if (ContainsAny(lower, "dịch vụ") &&
        ContainsAny(lower, "rẻ nhất", "giá thấp nhất", "rẻ nhất là gì"))
    {
        if (cheapestService != null)
        {
            replies.Add(
                $"Dịch vụ có giá thấp nhất hiện tại là {cheapestService.TenDV} với khoảng {cheapestService.DGDV.ToString("N0")}₫ một lần sử dụng."
            );
        }
    }

    if (ContainsAny(lower, "dịch vụ") &&
        ContainsAny(lower, "đắt nhất", "cao nhất", "giá cao nhất", "mắc nhất"))
    {
        if (mostExpensiveService != null)
        {
            replies.Add(
                $"Dịch vụ có giá cao nhất hiện tại là {mostExpensiveService.TenDV} với khoảng {mostExpensiveService.DGDV.ToString("N0")}₫ một lần sử dụng."
            );
        }
    }

    if (ContainsAny(lower, "đặt cọc", "tiền cọc", "cọc", "giữ chỗ", "deposit"))
    {
        replies.Add(
            "Tiền đặt cọc tại hệ thống DKS thường vào khoảng 30% tổng số tiền phòng và dịch vụ của cả kỳ lưu trú, " +
            "với mức tối thiểu khoảng 5.000₫. Nếu bạn hủy phòng sát giờ nhận phòng hoặc không đến nhận phòng, " +
            "tiền cọc thường sẽ không được hoàn lại; nếu cần đổi ngày hoặc điều chỉnh đặt phòng, bạn nên liên hệ sớm với lễ tân để được hỗ trợ linh hoạt."
        );
    }

    if (ContainsAny(lower, "hủy phòng", "hủy đặt phòng", "chính sách hủy", "hủy có mất cọc"))
    {
        replies.Add(
            "Bạn có thể hủy phòng trước thời điểm nhận phòng tối thiểu 24 giờ để tránh ảnh hưởng đến quyền lợi. " +
            "Tiền đặt cọc dùng để giữ chỗ thường không được hoàn lại khi hủy, nhưng trong một số trường hợp đặc biệt khách sạn có thể hỗ trợ dời lịch."
        );
    }

    if (ContainsAny(lower, "thanh toán", "trả tiền", "hình thức thanh toán", "cách thanh toán", "payment"))
    {
        replies.Add(
            "Hiện tại hệ thống hỗ trợ thanh toán trực tuyến qua VNPAY và thanh toán trực tiếp bằng tiền mặt tại quầy lễ tân. " +
            "Một số chi nhánh cũng có thể hỗ trợ chuyển khoản ngân hàng; bạn có thể trao đổi thêm với lễ tân khi làm thủ tục."
        );
    }

    if (ContainsAny(lower, "giá phòng", "giá tiền", "chi phí") && priceRange.min > 0)
    {
        replies.Add($"Giá phòng dao động từ {priceRange.min.ToString("N0")}₫ đến {priceRange.max.ToString("N0")}₫ mỗi đêm, phụ thuộc dạng phòng và thời điểm đặt.");
    }

    if (ContainsAny(lower, "nhiều phòng nhất", "phòng nhiều nhất", "số phòng nhiều nhất") && mostRoomsHotel != null)
    {
        replies.Add($"Khách sạn nhiều phòng nhất hiện tại là {mostRoomsHotel.Name}, cung cấp {mostRoomsHotel.RoomCount} phòng.");
    }

    if (ContainsAny(lower, "đặt nhiều nhất", "nhiều người đặt", "đặt phòng nhiều nhất") && mostBookedHotel != null)
    {
        replies.Add($"Khách sạn được đặt nhiều nhất là {mostBookedHotel.HotelName} với {mostBookedHotel.Count} lượt đặt gần đây.");
    }

    if (ContainsAny(lower, "tài khoản", "thông tin tài khoản", "đăng nhập", "tài khoản này"))
    {
        replies.Add(GetAccountInfoMessage());
    }

    var faqList = GetFaqTriggers().ToList();
    var faqSummary = ContainsAny(lower,
            "faq", "câu hỏi thường gặp", "thắc mắc thường gặp", "thắc mắc phổ biến", "câu hỏi phổ biến", "hỏi đáp")
        ? BuildFaqSummary(faqList)
        : null;

    if (!string.IsNullOrWhiteSpace(faqSummary))
    {
        replies.Add(faqSummary);
    }

    var faqReplies = GetFaqReplies(lower, faqList);
    replies.AddRange(faqReplies);

    if (replies.Count == 0)
    {
        replies.Add("Mình là trợ lý ảo chuỗi khách sạn DKS, có thể giúp bạn về thông tin khách sạn, phòng trống, dịch vụ và ưu đãi. Bạn cứ hỏi gì nhé!");
    }

    return string.Join(" ", replies);
}

private bool IsRoomBusy(THUEPHONG booking, DateTime now)
{
    if (booking == null)
    {
        return false;
    }

    var status = (booking.TrangThai ?? string.Empty).ToLowerInvariant();
    if (status.Contains("hủy") || status.Contains("huy") ||
        status.Contains("đã trả") || status.Contains("tra phong") || status.Contains("trả phòng"))
    {
        return false;
    }
    if (status.Contains("đang") || status.Contains("dang") ||
        status.Contains("đặt") || status.Contains("dat") ||
        status.Contains("chờ") || status.Contains("cho") ||
        status.Contains("thanh toán") || status.Contains("da thanh toan") ||
        status.Contains("đã thanh toán"))
    {
        return true;
    }
    var checkIn = booking.NgayVao;
    var checkOut = booking.NgayTra;
    if (checkIn.HasValue && checkOut.HasValue)
    {
        if (checkIn.Value <= now && checkOut.Value >= now)
        {
            return true;
        }
    }

    return false;
}
private string NormalizeForComparison(string input)
{
    if (string.IsNullOrWhiteSpace(input))
    {
        return string.Empty;
    }

    var normalized = input.Normalize(NormalizationForm.FormD);
    var builder = new StringBuilder();
    foreach (var c in normalized)
    {
        var category = CharUnicodeInfo.GetUnicodeCategory(c);
        if (category != UnicodeCategory.NonSpacingMark)
        {
            builder.Append(c);
        }
    }

    return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
}

private bool TryBuildLocationReply(string searchKey, List<HotelSummary> hotels, out string reply)
{
    reply = null;
    if (string.IsNullOrWhiteSpace(searchKey) || hotels == null || hotels.Count == 0)
    {
        return false;
    }

    var matches = hotels
        .Where(h => !string.IsNullOrWhiteSpace(h.Location))
        .Select(h => new
        {
            Hotel = h,
            NormalizedLocation = NormalizeForComparison(h.Location)
        })
        .Where(x => !string.IsNullOrWhiteSpace(x.NormalizedLocation) && searchKey.Contains(x.NormalizedLocation))
        .Select(x => x.Hotel)
        .ToList();

    if (!matches.Any())
    {
        return false;
    }

    var displayed = matches.Take(5).ToList();
    var hotelDetails = displayed
        .Select(h => $"{h.Name}{(h.MinPrice > 0 ? $" ({FormatPrice(h.MinPrice)})" : string.Empty)}")
        .ToList();

    var moreCount = matches.Count - displayed.Count;
    var moreSuffix = moreCount > 0 ? $" và {moreCount} cơ sở khác" : string.Empty;
    var locationName = displayed.First().Location ?? matches.First().Location ?? "vị trí được nhắc";

    reply = $"Tại {locationName} có {matches.Count} khách sạn: {string.Join(", ", hotelDetails)}{moreSuffix}.";

    return true;
}

private string FormatPrice(decimal value)
{
    return value > 0 ? value.ToString("N0") + "₫" : "đang cập nhật";
}

private bool TryBuildRegionReply(string searchKey, List<HotelSummary> hotels, out string reply)
{
    reply = null;
    if (string.IsNullOrWhiteSpace(searchKey) || hotels == null || hotels.Count == 0)
    {
        return false;
    }

    string region = null;
    if (searchKey.Contains("mien bac") || searchKey.Contains("mienbac") || searchKey.Contains("bac bo"))
    {
        region = "Bắc";
    }
    else if (searchKey.Contains("mien trung") || searchKey.Contains("mientrung") || searchKey.Contains("trung bo"))
    {
        region = "Trung";
    }
    else if (searchKey.Contains("mien nam") || searchKey.Contains("miennam") || searchKey.Contains("nam bo"))
    {
        region = "Nam";
    }

    if (region == null)
    {
        return false;
    }

    var regionMap = new Dictionary<string, string[]>
    {
        {
            "Bắc",
            new[]
            {
                "ha noi", "hanoi", "hai phong", "haiphong",
                "quang ninh", "ha long", "halong", "ninh binh"
            }
        },
        {
            "Trung",
            new[]
            {
                "da nang", "danang", "hue", "hoi an", "hoian",
                "quang nam", "quang ngai", "nha trang", "nhatrang"
            }
        },
        {
            "Nam",
            new[]
            {
                "ho chi minh", "hochiminh", "tp hcm", "tphcm", "sai gon", "saigon",
                "can tho", "cantho", "vung tau", "vungtau", "phan thiet", "phanthiet", "phu quoc", "phuquoc"
            }
        }
    };

    if (!regionMap.TryGetValue(region, out var regionTokens) || regionTokens == null || regionTokens.Length == 0)
    {
        return false;
    }

    var matches = hotels
        .Where(h => !string.IsNullOrWhiteSpace(h.Location))
        .Select(h => new
        {
            Hotel = h,
            NormalizedLocation = NormalizeForComparison(h.Location)
        })
        .Where(x => !string.IsNullOrWhiteSpace(x.NormalizedLocation) &&
                    regionTokens.Any(token => x.NormalizedLocation.Contains(token)))
        .Select(x => x.Hotel)
        .ToList();

    if (!matches.Any())
    {
        reply = $"Hiện tại hệ thống chưa có khách sạn nào được gắn rõ ràng vào khu vực miền {region}. Bạn có thể thử tìm theo tên thành phố cụ thể như Hà Nội, Đà Nẵng hoặc TP. Hồ Chí Minh.";
        return true;
    }

    var displayed = matches.Take(5).ToList();
    var hotelDetails = displayed
        .Select(h => $"{h.Name}{(string.IsNullOrWhiteSpace(h.Location) ? string.Empty : $" ({h.Location})")}")
        .ToList();

    var moreCount = matches.Count - displayed.Count;
    var moreSuffix = moreCount > 0 ? $" và {moreCount} cơ sở khác" : string.Empty;

    reply = $"Ở khu vực miền {region} hiện có {matches.Count} khách sạn trong chuỗi DKS, gồm: {string.Join(", ", hotelDetails)}{moreSuffix}.";

    return true;
}

private string GetAccountInfoMessage()
{
    if (Session["KhachHang"] is KHACHHANG customer)
    {
        var pieces = new List<string>();
        if (!string.IsNullOrWhiteSpace(customer.TKH))
        {
            pieces.Add($"Tên: {customer.TKH}");
        }
        if (!string.IsNullOrWhiteSpace(customer.Email))
        {
            pieces.Add($"Email: {customer.Email}");
        }
        if (!string.IsNullOrWhiteSpace(customer.SDT))
        {
            pieces.Add($"SĐT: {customer.SDT}");
        }
        if (!string.IsNullOrWhiteSpace(customer.DiaChi))
        {
            pieces.Add($"Địa chỉ: {customer.DiaChi}");
        }

        if (!pieces.Any())
        {
            return "Bạn đang đăng nhập, nhưng thông tin cá nhân chưa đầy đủ trong hệ thống.";
        }

        return "Thông tin tài khoản: " + string.Join("; ", pieces) + ".";
    }

    return "Bạn chưa đăng nhập, vui lòng đăng nhập để mình có thể xem thông tin tài khoản.";
}

private IEnumerable<string> GetFaqReplies(string lower, IEnumerable<(string[] Triggers, string Answer)> faqList)
{
    var replies = new List<string>();
    if (faqList == null)
    {
        return replies;
    }

    foreach (var faq in faqList)
    {
        if (ContainsAny(lower, faq.Triggers))
        {
            replies.Add(faq.Answer);
        }
    }

    return replies;
}

private string BuildFaqSummary(IEnumerable<(string[] Triggers, string Answer)> faqList)
{
    if (faqList == null)
    {
        return null;
    }

    var topics = faqList
        .Select(faq => faq.Triggers.FirstOrDefault())
        .Where(trigger => !string.IsNullOrWhiteSpace(trigger))
        .Select(ToFriendlyTopic)
        .Where(topic => !string.IsNullOrWhiteSpace(topic))
        .Distinct()
        .ToList();

    if (!topics.Any())
    {
        return null;
    }

    return $"Các câu hỏi thường gặp: {string.Join(", ", topics)}. Gõ một chủ đề trong danh sách để xem câu trả lời chi tiết.";
}

private string ToFriendlyTopic(string keyword)
{
    if (string.IsNullOrWhiteSpace(keyword))
    {
        return string.Empty;
    }

    var cleaned = keyword.Replace("-", " ").Trim();
    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cleaned);
}

private IEnumerable<(string[] Triggers, string Answer)> GetFaqTriggers()
{
    return new[]
    {
        (new[] { "check-in", "giờ nhận phòng", "mấy giờ nhận phòng" }, "Giờ nhận phòng lúc 14h, trả phòng trước 12h để nhân viên có thời gian dọn dẹp."),
        (new[] { "hủy phòng", "đổi phòng", "chính sách hủy" }, "Bạn có thể hủy phòng 24h trước ngày nhận phòng để hạn chế phát sinh chi phí; lưu ý tiền đặt cọc dùng để giữ chỗ thường không được hoàn lại."),
        (new[] { "wifi", "wi-fi" }, "Wifi tốc độ cao miễn phí khắp khu vực khách sạn."),
        (new[] { "đậu xe", "bãi đậu xe" }, "Khách sạn cung cấp bãi đậu xe an toàn, miễn phí cho khách lưu trú."),
        (new[] { "dịch vụ spa", "spa" }, "Spa cao cấp mở cửa từ 8h đến 21h, bạn có thể đặt lịch trước qua lễ tân."),
        (new[] { "ăn sáng", "bữa sáng" }, "Bữa sáng buffet được phục vụ tại nhà hàng chính từ 6h30 đến 8h sáng.")
    };
}

private bool ContainsAny(string source, params string[] keywords)
{
    return keywords.Any(keyword => source.Contains(keyword));
}

private class HotelSummary
{
    public int HotelId { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public int RoomCount { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}

protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        db.Dispose();
    }

    base.Dispose(disposing);
}*/


