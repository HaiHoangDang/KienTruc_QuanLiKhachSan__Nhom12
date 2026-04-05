using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Helpers;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Areas.Admin.Controllers
{
    public class AuthController : Controller
    {
        private readonly DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();

        public ActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lĂ²ng Ä‘iá»n Ä‘áº§y Ä‘á»§ thĂ´ng tin Ä‘Äƒng nháº­p.");
                return View();
            }

                        var normalizedUsername = username.Trim();
            var clientIp = SecurityAuditLogger.GetClientIp(Request);
            var lockState = LoginSecurityService.CheckLoginAllowed($"admin:{normalizedUsername}", clientIp);
            if (!lockState.Allowed)
            {
                ModelState.AddModelError("", $"Tai khoan tam khoa. Thu lai sau {lockState.RetryAfterSeconds} giay.");
                SecurityAuditLogger.Log("auth_admin", "login_blocked", "warning", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "username", normalizedUsername },
                    { "ip", clientIp },
                    { "retryAfterSeconds", lockState.RetryAfterSeconds }
                });
                return View();
            }

            var staff = db.NHANVIENs.FirstOrDefault(n => n.TenDN == normalizedUsername);
            if (staff == null)
            {
                var failState = LoginSecurityService.RecordFailure($"admin:{normalizedUsername}", clientIp);
                ModelState.AddModelError("", "Tai khoan khong ton tai.");
                SecurityAuditLogger.Log("auth_admin", "login_failed_unknown_user", "warning", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "username", normalizedUsername },
                    { "ip", clientIp },
                    { "locked", !failState.Allowed }
                });
                return View();
            }

            if (!PasswordHasher.VerifyPassword(staff.MatKhau, password, out var needsRehash))
            {
                var failState = LoginSecurityService.RecordFailure($"admin:{normalizedUsername}", clientIp);
                ModelState.AddModelError("", "Mat khau khong dung.");
                SecurityAuditLogger.Log("auth_admin", "login_failed_wrong_password", "warning", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "staffId", staff.MaNV },
                    { "username", normalizedUsername },
                    { "ip", clientIp },
                    { "locked", !failState.Allowed }
                });
                return View();
            }

            if (needsRehash)
            {
                staff.MatKhau = PasswordHasher.HashPassword(password);
                db.SaveChanges();
            }

            LoginSecurityService.ResetFailures($"admin:{normalizedUsername}", clientIp);

            Session["AdminUser"] = staff;
            Session["AdminId"] = staff.MaNV;
            Session["AdminName"] = staff.HoTen;
            StaffActivityTracker.RecordEvent("Dang nhap he thong", staff.MaNV, staff.HoTen);
            SecurityAuditLogger.Log("auth_admin", "login_success", "info", new System.Collections.Generic.Dictionary<string, object>
            {
                { "staffId", staff.MaNV },
                { "username", normalizedUsername },
                { "ip", clientIp }
            });

            return RedirectAfterLogin(returnUrl, staff);
        }

        public ActionResult Logout()
        {
            if (Session["AdminUser"] is NHANVIEN staff)
            {
                StaffActivityTracker.RecordEvent("ÄÄƒng xuáº¥t há»‡ thá»‘ng", staff.MaNV, staff.HoTen);
            }

            Session.Remove("AdminUser");
            Session.Remove("AdminId");
            Session.Remove("AdminName");
            return RedirectToAction("Login");
        }

        private ActionResult RedirectAfterLogin(string returnUrl, NHANVIEN staff)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (IsStaffRole(staff))
            {
                return RedirectToAction("Index", "Operations", new { area = "Staff" });
            }

            return RedirectToAction("Index", "Dashboard");
        }

        private bool IsStaffRole(NHANVIEN staff)
        {
            if (staff == null)
            {
                return false;
            }

            var role = (staff.ChucVu ?? string.Empty).ToLowerInvariant();
            return role.Contains("lá»… tĂ¢n") || role.Contains("le tan") || role.Contains("nhĂ¢n viĂªn") || role.Contains("staff");
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

