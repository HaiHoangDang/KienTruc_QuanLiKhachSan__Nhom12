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
using DKS_HotelManager.Services.ApiClients;

namespace DKS_HotelManager.Controllers
{
    public class ChatBotController : Controller
    {
        private readonly DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();
        private readonly AiApiClient aiApiClient = new AiApiClient();

        // ─── GET: /ChatBot/Index — load trang chat riêng (nếu có) ────────────
        public async Task<ActionResult> Index()
        {
            var customerId = GetCustomerId();
            var isLoggedIn = customerId != "guest";

            ViewBag.CustomerId = customerId;
            ViewBag.IsLoggedIn = isLoggedIn;
            ViewBag.ChatHistory = new List<object>();

            if (isLoggedIn)
            {
                ViewBag.ChatHistory = await LoadHistoryAsync(customerId);
            }

            return View();
        }

        // ─── POST: /ChatBot/Ask — gửi tin nhắn ───────────────────────────────
        [HttpPost]
        public async Task<ActionResult> Ask(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { reply = "Nội dung câu hỏi đang trống." });
            }

            var customerId = GetCustomerId();
            var isLoggedIn = customerId != "guest";
            var kh = Session["KhachHang"] as KHACHHANG;
            var customerName = kh?.TKH ?? null;

            var dbContext = BuildDatabaseContext(message, isLoggedIn, kh);

            // Gửi đủ 5 field lên AI Service
            var payload = new
            {
                question = message,
                customer_id = customerId,
                customer_name = customerName,    // TÊN khách (để AI xưng hô đúng)
                is_logged_in = isLoggedIn,      // TRẠNG THÁI đăng nhập (một dòng, rõ ràng)
                db_context = dbContext
            };

            var result = await aiApiClient.AskAsync(payload);

            return Json(new
            {
                reply = result.Answer,
                customerId = customerId,
                isLoggedIn = isLoggedIn
            });
        }

        // ─── GET: /ChatBot/LoadHistory — widget gọi khi mở ──────────────────
        [HttpGet]
        public async Task<ActionResult> LoadHistory()
        {
            var customerId = GetCustomerId();

            if (customerId == "guest")
            {
                return Json(new { messages = new List<object>() }, JsonRequestBehavior.AllowGet);
            }

            var messages = await LoadHistoryAsync(customerId);
            return Json(new { messages }, JsonRequestBehavior.AllowGet);
        }

        // ─── POST: /ChatBot/ClearHistory ─────────────────────────────────────
        [HttpPost]
        public async Task<ActionResult> ClearHistory()
        {
            var customerId = GetCustomerId();

            if (customerId == "guest")
            {
                return Json(new { success = false, message = "Chưa đăng nhập." });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var req = new HttpRequestMessage(
                        HttpMethod.Delete,
                        GatewayConfig.BaseUrl + "/api/ai/history/" + customerId
                    );
                    await client.SendAsync(req);
                }

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Không thể xóa lịch sử." });
            }
        }

        // ─── Helper: lấy customer_id từ session ──────────────────────────────
        private string GetCustomerId()
        {
            if (Session["KhachHangId"] != null)
            {
                return "kh_" + Session["KhachHangId"].ToString();
            }

            var kh = Session["KhachHang"] as KHACHHANG;

            if (kh != null && !string.IsNullOrEmpty(kh.Email))
            {
                return "kh_" + kh.Email;
            }

            return "guest";
        }

        // ─── Helper: gọi AI Service lấy lịch sử ─────────────────────────────
        private async Task<List<object>> LoadHistoryAsync(string customerId)
        {
            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                {
                    var url = GatewayConfig.BaseUrl + "/api/ai/history/" + customerId + "?limit=50";
                    var res = await client.GetAsync(url);

                    if (res.IsSuccessStatusCode)
                    {
                        var body = await res.Content.ReadAsStringAsync();
                        dynamic data = JsonConvert.DeserializeObject(body);
                        var msgs = data?.messages;

                        if (msgs != null)
                        {
                            var list = new List<object>();
                            foreach (var m in msgs)
                            {
                                list.Add(new
                                {
                                    role = (string)m.role,
                                    content = (string)m.content,
                                    created_at = (string)m.created_at
                                });
                            }
                            return list;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatBot] LoadHistory error: {ex.Message}");
            }

            return new List<object>();
        }

        // ─── Helper: build db_context — CHỈ 1 dòng IS_LOGGED_IN ─────────────
        private string BuildDatabaseContext(string userMessage, bool isLoggedIn, KHACHHANG kh)
        {
            try
            {
                var lower = (userMessage ?? "").ToLowerInvariant();
                var sb = new StringBuilder();

                // ── Trạng thái đăng nhập (CHỈ 1 dòng, không lặp) ──
                sb.AppendLine($"IS_LOGGED_IN={isLoggedIn.ToString().ToLower()}");

                if (isLoggedIn && kh != null)
                {
                    sb.AppendLine($"CUSTOMER_NAME={kh.TKH}");
                    if (Session["KhachHangId"] != null)
                        sb.AppendLine($"CUSTOMER_ID=kh_{Session["KhachHangId"]}");
                }

                sb.AppendLine();

                // ── Danh sách khách sạn ──
                var hotels = db.KHACHSANs
                    .Select(k => new { k.TenKS, k.DiaDiem })
                    .Take(10)
                    .ToList();

                sb.AppendLine("KHÁCH SẠN:");
                foreach (var h in hotels)
                    sb.AppendLine($"• {h.TenKS} — {h.DiaDiem}");

                // ── Phòng trống (khi hỏi liên quan) ──
                bool askRoom = lower.Contains("phòng") || lower.Contains("giá")
                    || lower.Contains("trống") || lower.Contains("đặt")
                    || lower.Contains("book");

                if (askRoom)
                {
                    var now = DateTime.Now;

                    var bookedIds = db.THUEPHONGs
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

                    var available = db.PHONGs
                        .Include("KHACHSAN")
                        .Include("LOAIPHONG")
                        .Where(p => !bookedIds.Contains(p.MaPhong))
                        .Select(p => new
                        {
                            TenPhong = p.TenPhong,
                            LoaiPhong = p.LOAIPHONG.TenLoai,
                            p.DGNgay,
                            KhachSan = p.KHACHSAN.TenKS
                        })
                        .OrderBy(p => p.DGNgay)
                        .Take(15)
                        .ToList();

                    sb.AppendLine($"\nPHÒNG TRỐNG ({available.Count} phòng hiển thị):");
                    foreach (var r in available)
                        sb.AppendLine($"• {r.TenPhong} — {r.LoaiPhong} — {r.DGNgay:N0}₫/đêm — {r.KhachSan}");
                }

                // ── Dịch vụ ──
                if (lower.Contains("dịch vụ") || lower.Contains("spa") || lower.Contains("ăn sáng"))
                {
                    var services = db.DICHVUs.OrderBy(d => d.TenDV).Take(10).ToList();
                    sb.AppendLine("\nDỊCH VỤ:");
                    foreach (var s in services)
                        sb.AppendLine($"• {s.TenDV} — {s.DGDV:N0}₫");
                }

                // ── Lịch sử đặt phòng của khách ──
                bool askMyBooking = lower.Contains("đặt phòng của tôi")
                    || lower.Contains("booking của tôi")
                    || lower.Contains("phòng tôi đang")
                    || lower.Contains("lịch sử đặt");

                if (isLoggedIn && askMyBooking && Session["KhachHangId"] != null)
                {
                    int khId = Convert.ToInt32(Session["KhachHangId"]);

                    var myBookings = (
                        from ct in db.CTTHUEPHONGs
                        join tp in db.THUEPHONGs.Include("PHONG.KHACHSAN")
                            on ct.MaThue equals tp.MaThue
                        where ct.KHACH == khId
                        orderby tp.NgayDat descending
                        select new
                        {
                            TenPhong = tp.PHONG.TenPhong,
                            TenKS = tp.PHONG.KHACHSAN.TenKS,
                            tp.NgayVao,
                            tp.NgayTra,
                            tp.TrangThai,
                            ct.VaiTro
                        }
                    ).Take(5).ToList();

                    if (myBookings.Any())
                    {
                        sb.AppendLine("\nĐẶT PHÒNG CỦA BẠN (5 gần nhất):");
                        foreach (var b in myBookings)
                            sb.AppendLine(
                                $"• {b.TenPhong} tại {b.TenKS} | " +
                                $"Vào: {b.NgayVao:dd/MM/yyyy} | " +
                                $"Ra: {b.NgayTra:dd/MM/yyyy} | " +
                                $"Trạng thái: {b.TrangThai} | " +
                                $"Vai trò: {b.VaiTro}"
                            );
                    }
                    else
                    {
                        sb.AppendLine("\nBạn chưa có đặt phòng nào.");
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"IS_LOGGED_IN={isLoggedIn.ToString().ToLower()}\n[Lỗi DB: {ex.Message}]";
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
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web.Mvc;
//using DKS_HotelManager.Models;
//using Newtonsoft.Json;
//using DKS_HotelManager.Services.ApiClients;
//namespace DKS_HotelManager.Controllers
//{
//    public class ChatBotController : Controller
//    {
//        //private static readonly string AiServiceUrl = "http://127.0.0.1:8000";
//        //private static readonly string AiServiceUrl = "http://localhost:6000/api/ai/chat";
//        private readonly DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();
//        private readonly AiApiClient aiApiClient = new AiApiClient();
//        public async Task<ActionResult> Index()
//        {
//            var customerId = GetCustomerId();
//            var isLoggedIn = customerId != "guest";

//            ViewBag.CustomerId = customerId;
//            ViewBag.IsLoggedIn = isLoggedIn;
//            ViewBag.ChatHistory = new List<dynamic>();

//            if (!isLoggedIn)
//            {
//                return View();
//            }

//            try
//            {
//                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
//                {
//                    //var res = await client.GetAsync($"{AiServiceUrl}/history/{customerId}?limit=50");
//                    var res = await client.GetAsync(GatewayConfig.BaseUrl + "/api/ai/history/" + customerId);
//                    if (res.IsSuccessStatusCode)
//                    {
//                        var body = await res.Content.ReadAsStringAsync();
//                        dynamic data = JsonConvert.DeserializeObject(body);
//                        ViewBag.ChatHistory = data?.messages ?? new List<object>();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine($"[ChatBot] Load history error: {ex.Message}");
//            }

//            return View();
//        }

//        //[HttpPost]
//        //public async Task<ActionResult> Ask(string message)
//        //{
//        //    if (string.IsNullOrWhiteSpace(message))
//        //    {
//        //        return Json(new { reply = "Nội dung câu hỏi đang trống." });
//        //    }

//        //    var customerId = GetCustomerId();
//        //    var isLoggedIn = customerId != "guest";
//        //    var dbContext = BuildDatabaseContext(message, isLoggedIn);

//        //    try
//        //    {
//        //        using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(120) })
//        //        {
//        //            var payload = new
//        //            {
//        //                question = message,
//        //                customer_id = customerId,
//        //                db_context = dbContext
//        //            };

//        //            var json = JsonConvert.SerializeObject(payload);
//        //            var content = new StringContent(json, Encoding.UTF8, "application/json");
//        //            var response = await client.PostAsync($"{AiServiceUrl}/chat", content);
//        //            var body = await response.Content.ReadAsStringAsync();

//        //            System.Diagnostics.Debug.WriteLine($"[ChatBot] {response.StatusCode}: {body}");

//        //            if (!response.IsSuccessStatusCode)
//        //            {
//        //                return Json(new { reply = $"AI service lỗi ({(int)response.StatusCode}): {body}" });
//        //            }

//        //            dynamic result = JsonConvert.DeserializeObject(body);
//        //            string answer = result?.answer != null ? (string)result.answer : "Không có câu trả lời.";

//        //            return Json(new { reply = answer });
//        //        }
//        //    }
//        //    catch (TaskCanceledException)
//        //    {
//        //        return Json(new { reply = "AI phản hồi quá lâu, vui lòng thử lại." });
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        return Json(new { reply = $"Lỗi kết nối: {ex.Message}" });
//        //    }
//        //}
//        [HttpPost]
//        public async Task<ActionResult> Ask(string message)
//        {
//            if (string.IsNullOrWhiteSpace(message))
//            {
//                return Json(new { reply = "Nội dung câu hỏi đang trống." });
//            }

//            var customerId = GetCustomerId();
//            var isLoggedIn = customerId != "guest";
//            var dbContext = BuildDatabaseContext(message, isLoggedIn);

//            var payload = new
//            {
//                question = message,
//                customer_id = customerId,
//                context_type = "summary",
//                db_context = dbContext
//            };

//            var result = await aiApiClient.AskAsync(payload);

//            if (!result.Success)
//            {
//                return Json(new
//                {
//                    reply = result.Answer
//                });
//            }

//            return Json(new
//            {
//                reply = result.Answer,
//                customerId = customerId
//            });
//        }

//        //[HttpPost]
//        //public async Task<ActionResult> ClearHistory()
//        //{
//        //    var customerId = GetCustomerId();

//        //    if (customerId == "guest")
//        //    {
//        //        return Json(new { success = false, message = "Chưa đăng nhập." });
//        //    }

//        //    try
//        //    {
//        //        using (var client = new HttpClient())
//        //        {
//        //            var req = new HttpRequestMessage(HttpMethod.Delete, $"{AiServiceUrl}/history/{customerId}");
//        //            await client.SendAsync(req);
//        //        }

//        //        return Json(new { success = true });
//        //    }
//        //    catch
//        //    {
//        //        return Json(new { success = false, message = "Không thể xóa lịch sử." });
//        //    }
//        //}
//        [HttpPost]
//        public async Task<ActionResult> ClearHistory()
//        {
//            var customerId = GetCustomerId();

//            if (customerId == "guest")
//            {
//                return Json(new { success = false, message = "Chưa đăng nhập." });
//            }

//            try
//            {
//                using (var client = new HttpClient())
//                {
//                    var req = new HttpRequestMessage(
//                        HttpMethod.Delete,
//                        GatewayConfig.BaseUrl + "/api/ai/history/" + customerId
//                    );

//                    await client.SendAsync(req);
//                }

//                return Json(new { success = true });
//            }
//            catch
//            {
//                return Json(new { success = false, message = "Không thể xóa lịch sử." });
//            }
//        }

//        private string GetCustomerId()
//        {
//            if (Session["KhachHangId"] != null)
//            {
//                return "kh_" + Session["KhachHangId"].ToString();
//            }

//            var kh = Session["KhachHang"] as KHACHHANG;

//            if (kh != null && !string.IsNullOrEmpty(kh.Email))
//            {
//                return "kh_" + kh.Email;
//            }

//            return "guest";
//        }

//        private string BuildDatabaseContext(string userMessage, bool isLoggedIn)
//        {
//            try
//            {
//                var lower = (userMessage ?? "").ToLowerInvariant();
//                var sb = new StringBuilder();

//                sb.AppendLine($"IS_LOGGED_IN={isLoggedIn.ToString().ToLower()}");

//                var kh = Session["KhachHang"] as KHACHHANG;

//                if (isLoggedIn && kh != null)
//                {
//                    sb.AppendLine($"CUSTOMER_NAME={kh.TKH}");
//                }

//                sb.AppendLine();

//                var hotels = db.KHACHSANs
//                    .Select(k => new
//                    {
//                        k.TenKS,
//                        k.DiaDiem
//                    })
//                    .Take(10)
//                    .ToList();

//                sb.AppendLine("KHÁCH SẠN:");

//                foreach (var h in hotels)
//                {
//                    sb.AppendLine($"• {h.TenKS} — {h.DiaDiem}");
//                }

//                var askRoom =
//                    lower.Contains("phòng") ||
//                    lower.Contains("giá") ||
//                    lower.Contains("trống") ||
//                    lower.Contains("đặt");

//                if (askRoom)
//                {
//                    var now = DateTime.Now;

//                    var bookedIds = db.THUEPHONGs
//                        .Where(t =>
//                            t.TrangThai != null &&
//                            !t.TrangThai.Contains("hủy") &&
//                            !t.TrangThai.Contains("trả phòng") &&
//                            (
//                                (t.NgayVao <= now && t.NgayTra >= now) ||
//                                t.TrangThai.Contains("đang") ||
//                                t.TrangThai.Contains("đặt")
//                            )
//                        )
//                        .Select(t => t.MaPhong)
//                        .Distinct()
//                        .ToList();

//                    var available = db.PHONGs
//                        .Include("KHACHSAN")
//                        .Include("LOAIPHONG")
//                        .Where(p => !bookedIds.Contains(p.MaPhong))
//                        .Select(p => new
//                        {
//                            TenPhong = p.TenPhong,
//                            LoaiPhong = p.LOAIPHONG.TenLoai,
//                            p.DGNgay,
//                            KhachSan = p.KHACHSAN.TenKS
//                        })
//                        .OrderBy(p => p.DGNgay)
//                        .Take(15)
//                        .ToList();

//                    sb.AppendLine($"\nPHÒNG TRỐNG ({available.Count} phòng hiển thị):");

//                    foreach (var r in available)
//                    {
//                        sb.AppendLine($"• {r.TenPhong} — {r.LoaiPhong} — {r.DGNgay:N0}₫/đêm — {r.KhachSan}");
//                    }
//                }

//                if (lower.Contains("dịch vụ") || lower.Contains("spa") || lower.Contains("ăn sáng"))
//                {
//                    var services = db.DICHVUs
//                        .OrderBy(d => d.TenDV)
//                        .Take(10)
//                        .ToList();

//                    sb.AppendLine("\nDỊCH VỤ:");

//                    foreach (var s in services)
//                    {
//                        sb.AppendLine($"• {s.TenDV} — {s.DGDV:N0}₫");
//                    }
//                }

//                var askMyBooking =
//                    lower.Contains("đặt phòng của tôi") ||
//                    lower.Contains("booking của tôi") ||
//                    lower.Contains("phòng tôi đang") ||
//                    lower.Contains("lịch sử đặt");

//                if (isLoggedIn && askMyBooking && Session["KhachHangId"] != null)
//                {
//                    int khId = Convert.ToInt32(Session["KhachHangId"]);

//                    var myBookings = (
//                        from ct in db.CTTHUEPHONGs
//                        join tp in db.THUEPHONGs.Include("PHONG.KHACHSAN")
//                            on ct.MaThue equals tp.MaThue
//                        where ct.KHACH == khId
//                        orderby tp.NgayDat descending
//                        select new
//                        {
//                            TenPhong = tp.PHONG.TenPhong,
//                            TenKS = tp.PHONG.KHACHSAN.TenKS,
//                            tp.NgayVao,
//                            tp.NgayTra,
//                            tp.TrangThai,
//                            ct.VaiTro
//                        }
//                    )
//                    .Take(5)
//                    .ToList();

//                    if (myBookings.Any())
//                    {
//                        sb.AppendLine("\nĐẶT PHÒNG CỦA BẠN (5 gần nhất):");

//                        foreach (var b in myBookings)
//                        {
//                            sb.AppendLine(
//                                $"• {b.TenPhong} tại {b.TenKS} | " +
//                                $"Vào: {b.NgayVao:dd/MM/yyyy} | " +
//                                $"Ra: {b.NgayTra:dd/MM/yyyy} | " +
//                                $"Trạng thái: {b.TrangThai} | " +
//                                $"Vai trò: {b.VaiTro}"
//                            );
//                        }
//                    }
//                    else
//                    {
//                        sb.AppendLine("\nBạn chưa có đặt phòng nào.");
//                    }
//                }

//                return sb.ToString();
//            }
//            catch (Exception ex)
//            {
//                return $"IS_LOGGED_IN={isLoggedIn.ToString().ToLower()}\n[Lỗi DB: {ex.Message}]";
//            }
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                db.Dispose();
//            }

//            base.Dispose(disposing);
//        }
//    }
//}