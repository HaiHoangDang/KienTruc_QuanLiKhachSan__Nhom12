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

namespace DKS_HotelManager.Controllers
{
    public class ChatBotController : Controller
    {
        private static readonly string AiServiceUrl = "http://127.0.0.1:8000";
        private readonly DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();

        // ── Trang chat (GET) ────────────────────────────────────────────────
        public async Task<ActionResult> Index()
        {
            var customerId = GetCustomerId();

            // Nếu đã đăng nhập → load lịch sử từ AI service
            if (customerId != "guest")
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);
                        var res = await client.GetAsync($"{AiServiceUrl}/history/{customerId}?limit=50");
                        if (res.IsSuccessStatusCode)
                        {
                            var body = await res.Content.ReadAsStringAsync();
                            dynamic data = JsonConvert.DeserializeObject(body);
                            ViewBag.ChatHistory = data?.messages ?? new List<object>();
                        }
                    }
                }
                catch { /* lịch sử không load được → hiển thị trống */ }
            }

            ViewBag.CustomerId = customerId;
            ViewBag.IsLoggedIn = customerId != "guest";
            return View();
        }

        // ── Gửi tin nhắn (POST) ─────────────────────────────────────────────
        [HttpPost]
        public async Task<ActionResult> Ask(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return Json(new { reply = "Nội dung câu hỏi đang trống." });

            var customerId = GetCustomerId();
            var isLoggedIn = customerId != "guest";
            var dbContext = BuildDatabaseContext(message, isLoggedIn);

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(120);

                    var payload = new
                    {
                        question = message,
                        customer_id = customerId,   // ← gửi customer_id để AI service lưu history
                        context_type = "summary",
                        db_context = dbContext
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{AiServiceUrl}/chat", content);
                    var body = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine($"[ChatBot] {response.StatusCode}: {body}");

                    if (!response.IsSuccessStatusCode)
                        return Json(new { reply = $"AI service lỗi ({(int)response.StatusCode}): {body}" });

                    dynamic result = JsonConvert.DeserializeObject(body);
                    string answer = result?.answer ?? "Không có câu trả lời.";
                    return Json(new { reply = answer, customerId });
                }
            }
            catch (TaskCanceledException)
            {
                return Json(new { reply = "AI service phản hồi quá lâu, vui lòng thử lại." });
            }
            catch (Exception ex)
            {
                return Json(new { reply = $"Lỗi kết nối: {ex.Message}" });
            }
        }

        // ── Xóa lịch sử chat (POST) ─────────────────────────────────────────
        [HttpPost]
        public async Task<ActionResult> ClearHistory()
        {
            var customerId = GetCustomerId();
            if (customerId == "guest")
                return Json(new { success = false, message = "Chưa đăng nhập." });

            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Delete,
                        $"{AiServiceUrl}/history/{customerId}");
                    await client.SendAsync(request);
                }
                return Json(new { success = true, message = "Đã xóa lịch sử chat." });
            }
            catch
            {
                return Json(new { success = false, message = "Không thể xóa lịch sử." });
            }
        }

        // ── Helper: lấy customer ID từ Session ─────────────────────────────
        private string GetCustomerId()
        {
            // Ưu tiên lấy MaKH (int), fallback sang Email nếu có
            if (Session["KhachHangId"] != null)
                return Session["KhachHangId"].ToString();

            if (Session["KhachHang"] is KHACHHANG kh && !string.IsNullOrEmpty(kh.Email))
                return $"kh_{kh.Email}";

            return "guest";
        }

        // ── Helper: xây DB context realtime ────────────────────────────────
        private string BuildDatabaseContext(string userMessage, bool isLoggedIn)
        {
            try
            {
                var lower = (userMessage ?? "").ToLowerInvariant();
                var sb = new StringBuilder();

                sb.AppendLine($"IS_LOGGED_IN={isLoggedIn.ToString().ToLower()}");

                // Thông tin tài khoản đang đăng nhập
                if (isLoggedIn && Session["KhachHang"] is KHACHHANG kh)
                {
                    sb.AppendLine($"CUSTOMER_NAME={kh.TKH}");
                    sb.AppendLine($"CUSTOMER_EMAIL={kh.Email}");
                }

                sb.AppendLine();
                sb.AppendLine("=== DỮ LIỆU THỰC TẾ TỪ DATABASE ===");

                // Danh sách khách sạn
                var hotels = db.KHACHSANs
                    .Select(k => new { k.MaKS, k.TenKS, k.DiaDiem })
                    .ToList();

                sb.AppendLine("\nKHÁCH SẠN:");
                foreach (var h in hotels)
                    sb.AppendLine($"• [{h.MaKS}] {h.TenKS} — {h.DiaDiem}");

                // Phòng trống
                var now = DateTime.Now;
                var bookedIds = db.THUEPHONGs
                    .Where(t => t.TrangThai != null
                        && !t.TrangThai.Contains("hủy")
                        && !t.TrangThai.Contains("trả phòng")
                        && (t.NgayVao <= now && t.NgayTra >= now
                            || t.TrangThai.Contains("đang")
                            || t.TrangThai.Contains("đặt")))
                    .Select(t => t.MaPhong)
                    .Distinct().ToList();

                var available = db.PHONGs
                    .Include("KHACHSAN")
                    .Include("LOAIPHONG")
                    .Where(p => !bookedIds.Contains(p.MaPhong))
                    .Select(p => new
                    {
                        TenPhong = p.TenPhong,
                        LoaiPhong = p.LOAIPHONG.TenLoai,
                        p.DGNgay,
                        KhachSan = p.KHACHSAN.TenKS,
                        DiaDiem = p.KHACHSAN.DiaDiem
                    })
                    .OrderBy(p => p.DGNgay)
                    .ToList();

                sb.AppendLine($"\nPHÒNG TRỐNG ({available.Count} phòng):");
                foreach (var r in available)
                {
                    sb.AppendLine($"• {r.TenPhong} — {r.LoaiPhong} — {r.DGNgay:N0}₫/đêm — {r.KhachSan}, {r.DiaDiem}");
                }

                // Dịch vụ (chỉ khi hỏi liên quan)
                if (lower.Contains("dịch vụ") || lower.Contains("service")
                    || lower.Contains("spa") || lower.Contains("ăn sáng"))
                {
                    var services = db.DICHVUs.OrderBy(d => d.TenDV).ToList();
                    sb.AppendLine($"\nDỊCH VỤ ({services.Count}):");
                    foreach (var s in services)
                        sb.AppendLine($"• {s.TenDV} — {s.DGDV:N0}₫");
                }

                //// Booking của khách đang đăng nhập
                //if (isLoggedIn && Session["KhachHangId"] != null
                //    && (lower.Contains("đặt phòng của tôi") || lower.Contains("lịch sử")
                //        || lower.Contains("booking của tôi") || lower.Contains("phòng tôi")))
                //{
                //    int khId = Convert.ToInt32(Session["KhachHangId"]);
                //    var myBookings = db.THUEPHONGs
                //      .Include("PHONG.KHACHSAN")
                //      .Where(t => t.MaKH == khId)
                //      .OrderByDescending(t => t.NgayDat)
                //      .Take(5).ToList();

                //    if (myBookings.Any())
                //    {
                //        sb.AppendLine("\nĐẶT PHÒNG CỦA BẠN (5 gần nhất):");
                //        foreach (var b in myBookings)
                //            sb.AppendLine($"• Phòng {b.PHONG?.SoPhong} tại {b.PHONG?.KHACHSAN?.TenKS} | Vào: {b.NgayVao:dd/MM/yyyy} | Ra: {b.NgayTra:dd/MM/yyyy} | Trạng thái: {b.TrangThai}");
                //    }
                //}

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"IS_LOGGED_IN={isLoggedIn.ToString().ToLower()}\n[Lỗi đọc database: {ex.Message}]";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
