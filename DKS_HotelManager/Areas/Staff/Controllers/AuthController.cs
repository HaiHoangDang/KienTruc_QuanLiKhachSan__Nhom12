using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Helpers;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Areas.Staff.Controllers
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
            var lockState = LoginSecurityService.CheckLoginAllowed($"staff:{normalizedUsername}", clientIp);
            if (!lockState.Allowed)
            {
                ModelState.AddModelError("", $"Tai khoan tam khoa. Thu lai sau {lockState.RetryAfterSeconds} giay.");
                SecurityAuditLogger.Log("auth_staff", "login_blocked", "warning", new System.Collections.Generic.Dictionary<string, object>
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
                var failState = LoginSecurityService.RecordFailure($"staff:{normalizedUsername}", clientIp);
                ModelState.AddModelError("", "Tai khoan khong ton tai.");
                SecurityAuditLogger.Log("auth_staff", "login_failed_unknown_user", "warning", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "username", normalizedUsername },
                    { "ip", clientIp },
                    { "locked", !failState.Allowed }
                });
                return View();
            }

            if (!PasswordHasher.VerifyPassword(staff.MatKhau, password, out var needsRehash))
            {
                var failState = LoginSecurityService.RecordFailure($"staff:{normalizedUsername}", clientIp);
                ModelState.AddModelError("", "Mat khau khong dung.");
                SecurityAuditLogger.Log("auth_staff", "login_failed_wrong_password", "warning", new System.Collections.Generic.Dictionary<string, object>
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

            LoginSecurityService.ResetFailures($"staff:{normalizedUsername}", clientIp);

            Session["AdminUser"] = staff;
            Session["AdminId"] = staff.MaNV;
            Session["AdminName"] = staff.HoTen;
            StaffActivityTracker.RecordEvent("Nhan vien le tan dang nhap", staff.MaNV, staff.HoTen);
            SecurityAuditLogger.Log("auth_staff", "login_success", "info", new System.Collections.Generic.Dictionary<string, object>
            {
                { "staffId", staff.MaNV },
                { "username", normalizedUsername },
                { "ip", clientIp }
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Operations", new { area = "Staff" });
    }

    public ActionResult Logout()
    {
        int? staffId = null;
        var staffIdObj = Session["AdminId"];
        if (staffIdObj is int)
        {
            staffId = (int)staffIdObj;
        }
        else if (staffIdObj is int?)
        {
            staffId = (int?)staffIdObj;
        }

        var staffName = Session["AdminName"] as string;

        if (staffId.HasValue || !string.IsNullOrWhiteSpace(staffName))
        {
            StaffActivityTracker.RecordEvent("NhĂ¢n viĂªn lá»… tĂ¢n Ä‘Äƒng xuáº¥t", staffId, staffName);
        }

        Session.Clear();
        Session.Abandon();
        return RedirectToAction("Login");
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

