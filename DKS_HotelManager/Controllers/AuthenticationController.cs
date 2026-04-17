using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DKS_HotelManager.Helpers;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using DKS_HotelManager.Models;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace DKS_HotelManager.Controllers
{
    public class AuthenticationController : Controller
    {
        private DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();
        private const string PendingRegistrationKey = "PendingRegistrationData";
        private const string RegistrationCodeKey = "RegistrationConfirmationCode";
        private const string RegistrationCodeExpiryKey = "RegistrationConfirmationExpiry";

        // GET: Authentication/Login
        public ActionResult Login(string returnUrl = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        public ActionResult GoogleLogin(string returnUrl = "")
        {
            var googleClientId = SecurityConfig.GetSecret("GoogleClientId", "DKS_GOOGLE_CLIENT_ID");
            if (string.IsNullOrWhiteSpace(googleClientId))
            {
                TempData["LoginError"] = "Đăng nhập Google không thành công";
                SecurityAuditLogger.Log("auth", "google_login_missing_client_id", "warning");
                return RedirectToAction("Login");
            }

            var redirectUri = Url.Action("LoginGoogle", "Authentication", null, Request?.Url?.Scheme ?? "http");
            var safeReturnUrl = GetSafeLocalUrl(returnUrl, "google_login_state_rejected");
            var response = GenerateGoogleAuthUrl(googleClientId, redirectUri, safeReturnUrl);
            return Redirect(response);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, bool rememberMe = false, string returnUrl = "")
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var httpClient = new HttpClient();

            var json = JsonConvert.SerializeObject(new
            {
                Email = model.Username,
                Password = model.Password
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(
                "http://localhost:5199/api/auth/login",
                content
            );

            //var errorBody = await response.Content.ReadAsStringAsync();
            //if (!response.IsSuccessStatusCode)
            //{
            //    ModelState.AddModelError("", errorBody);
            //    return View(model);
            //}

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Sai thông tin đăng nhập");
                return View(model);
            }
            var resultString = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(resultString);
            Session["token"] = result.token;
            Session["KhachHangTen"] = model.Username;
            return RedirectToLocalOrDefault(returnUrl, "login_return_url_rejected");
            //var khachHang = db.KHACHHANGs.FirstOrDefault(k => k.TenDN == normalizedUsername);

            //if (khachHang == null)
            //{
            //    var failState = LoginSecurityService.RecordFailure(normalizedUsername, clientIp);
            //    ModelState.AddModelError("", "Tai khoan khong ton tai.");
            //    SecurityAuditLogger.Log("auth", "login_failed_unknown_user", "warning", new Dictionary<string, object>
            //    {
            //        { "username", normalizedUsername },
            //        { "ip", clientIp },
            //        { "locked", !failState.Allowed },
            //        { "retryAfterSeconds", failState.RetryAfterSeconds }
            //    });
            //    return View(model);
            //}

            //if (!PasswordHasher.VerifyPassword(khachHang.MatKhau, model.Password, out var needsRehash))
            //{
            //    var failState = LoginSecurityService.RecordFailure(normalizedUsername, clientIp);
            //    ModelState.AddModelError("", "Mat khau khong dung.");
            //    SecurityAuditLogger.Log("auth", "login_failed_wrong_password", "warning", new Dictionary<string, object>
            //    {
            //        { "username", normalizedUsername },
            //        { "customerId", khachHang.MKH },
            //        { "ip", clientIp },
            //        { "locked", !failState.Allowed },
            //        { "retryAfterSeconds", failState.RetryAfterSeconds }
            //    });
            //    return View(model);
            //}

            //if (needsRehash)
            //{
            //    khachHang.MatKhau = PasswordHasher.HashPassword(model.Password);
            //    db.Entry(khachHang).State = EntityState.Modified;
            //    db.SaveChanges();
            //    SecurityAuditLogger.Log("auth", "password_rehashed_on_login", "info", new Dictionary<string, object>
            //    {
            //        { "username", normalizedUsername },
            //        { "customerId", khachHang.MKH }
            //    });
            //}

            //LoginSecurityService.ResetFailures(normalizedUsername, clientIp);

            //Session["KhachHang"] = khachHang;
            //Session["KhachHangId"] = khachHang.MKH;
            //Session["KhachHangTen"] = khachHang.TKH;

            //SecurityAuditLogger.Log("auth", "login_success", "info", new Dictionary<string, object>
            //{
            //    { "username", normalizedUsername },
            //    { "customerId", khachHang.MKH },
            //    { "ip", clientIp }
            //});
            //if (rememberMe)
            //{
            //    HttpCookie cookie = new HttpCookie("RememberMe");
            //    cookie.Value = khachHang.MKH.ToString();
            //    cookie.Expires = DateTime.Now.AddDays(30);
            //    cookie.HttpOnly = true;
            //    cookie.Secure = Request?.IsSecureConnection ?? false;
            //    cookie.SameSite = SameSiteMode.Lax;
            //    Response.Cookies.Add(cookie);
            //}

            //return RedirectToLocalOrDefault(returnUrl, "login_return_url_rejected");
        }

        public async Task<ActionResult> LoginGoogle(string code, string state, string error = null)
        {
            if (!string.IsNullOrEmpty(error))
            {
                TempData["LoginError"] = "Khong the xac thuc voi Google.";
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Login");
            }

            var googleClientId = SecurityConfig.GetSecret("GoogleClientId", "DKS_GOOGLE_CLIENT_ID");
            var googleClientSecret = SecurityConfig.GetSecret("GoogleClientSecret", "DKS_GOOGLE_CLIENT_SECRET");
            if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
            {
                TempData["LoginError"] = "Dang nhap Google chua duoc cau hinh day du.";
                SecurityAuditLogger.Log("auth", "google_login_missing_config", "warning");
                return RedirectToAction("Login");
            }

            var redirectUri = Url.Action("LoginGoogle", "Authentication", null, Request?.Url?.Scheme ?? "http");
            string accessToken;
            try
            {
                accessToken = await ExchangeCodeForTokenAsync(code, googleClientId, redirectUri, googleClientSecret);
            }
            catch (Exception ex)
            {
                TempData["LoginError"] = $"Loi xac thuc Google: {ex.Message}";
                return RedirectToAction("Login");
            }

            dynamic user;
            try
            {
                user = await GetGoogleUserInfoAsync(accessToken);
            }
            catch (Exception ex)
            {
                TempData["LoginError"] = $"Khong the lay thong tin Google: {ex.Message}";
                return RedirectToAction("Login");
            }

            string email = user?.email;
            string name = user?.name ?? user?.given_name ?? "KhĂ¡ch";

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["LoginError"] = "Google khong tra ve email hop le.";
                return RedirectToAction("Login");
            }

            var customer = db.KHACHHANGs.FirstOrDefault(k => k.Email == email);
            if (customer == null)
            {
                TempData["LoginError"] = "Email Google cua ban chua duoc dang ky trong he thong.";
                SecurityAuditLogger.Log("auth", "google_login_email_not_registered", "warning", new Dictionary<string, object>
                {
                    { "email", email }
                });
                return RedirectToAction("Login");
            }

            Session["KhachHang"] = customer;
            Session["KhachHangId"] = customer.MKH;
            Session["KhachHangTen"] = customer.TKH;
            SecurityAuditLogger.Log("auth", "google_login_success", "info", new Dictionary<string, object>
            {
                { "customerId", customer.MKH },
                { "email", email }
            });

            return RedirectToLocalOrDefault(state, "google_login_state_rejected");
        }

        // GET: Authentication/Register
        public ActionResult Register()
        {
            Session.Remove(PendingRegistrationKey);
            Session.Remove(RegistrationCodeKey);
            Session.Remove(RegistrationCodeExpiryKey);
            return View();
        }

        // POST: Authentication/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model, string captchaCode)
        {
            bool isConfirmationStep = !string.IsNullOrWhiteSpace(model.ConfirmationCode);
            var normalizedUsername = model.TenDN?.Trim();
            model.TenDN = normalizedUsername;

            if (!isConfirmationStep)
            {
                if (Session["CaptchaCode"] == null || Session["CaptchaCode"].ToString() != captchaCode)
                {
                    ModelState.AddModelError("CaptchaCode", "Ma xac nhan khong dung.");
                    GenerateCaptcha();
                    return View(model);
                }
            }

            if (isConfirmationStep)
            {
                var pending = Session[PendingRegistrationKey] as RegisterViewModel;
                var expectedCode = Session[RegistrationCodeKey] as string;
                var expiry = Session[RegistrationCodeExpiryKey] as DateTime?;

                if (pending == null || string.IsNullOrWhiteSpace(expectedCode) || !expiry.HasValue || expiry.Value < DateTime.UtcNow)
                {
                    ModelState.AddModelError("", "Ma xac nhan da het han. Vui long gui lai ma.");
                    ViewBag.ShowConfirmation = true;
                    GenerateCaptcha();
                    return View(model);
                }

                if (!string.Equals(model.ConfirmationCode?.Trim(), expectedCode, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("ConfirmationCode", "Ma xac nhan khong dung.");
                    ViewBag.ShowConfirmation = true;
                    pending.ConfirmationCode = model.ConfirmationCode;
                    return View(pending);
                }

                if (!TryCreateCustomer(pending, out var newCustomer, out var creationError))
                {
                    ModelState.AddModelError("", creationError);
                    ViewBag.ShowConfirmation = true;
                    pending.ConfirmationCode = model.ConfirmationCode;
                    GenerateCaptcha();
                    return View(pending);
                }

                ClearPendingRegistration();
                Session["KhachHang"] = newCustomer;
                Session["KhachHangId"] = newCustomer.MKH;
                Session["KhachHangTen"] = newCustomer.TKH;

                TempData["RegisterSuccess"] = "Dang ky thanh cong! Ban da dang nhap tu dong.";
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email la bat buoc de nhan ma xac nhan.");
            }

            if (!ModelState.IsValid)
            {
                GenerateCaptcha();
                return View(model);
            }

            if (!ValidateRegistrationData(model, out var validationError))
            {
                ModelState.AddModelError("", validationError);
                GenerateCaptcha();
                return View(model);
            }

            var code = GenerateNumericCode(6);
            try
            {
                MailService.SendConfirmationCode(model.Email.Trim(), code);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Khong the gui ma xac nhan: " + ex.Message);
                GenerateCaptcha();
                return View(model);
            }

            Session[PendingRegistrationKey] = CreatePendingModel(model);
            Session[RegistrationCodeKey] = code;
            Session[RegistrationCodeExpiryKey] = DateTime.UtcNow.AddMinutes(10);
            ViewBag.ShowConfirmation = true;
            ViewBag.ConfirmationMessage = "Da gui ma xac nhan den email cua ban. Ma co hieu luc trong 10 phut.";
            GenerateCaptcha();
            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            if (Request.Cookies["RememberMe"] != null)
            {
                HttpCookie cookie = new HttpCookie("RememberMe");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult CaptchaImage()
        {
            GenerateCaptcha();
            return View();
        }

        private void GenerateCaptcha()
        {
            Random random = new Random();
            string captcha = "";
            for (int i = 0; i < 5; i++)
            {
                captcha += random.Next(0, 10).ToString();
            }
            Session["CaptchaCode"] = captcha;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionDescriptor.ActionName == "Register" &&
                filterContext.HttpContext.Request.HttpMethod == "GET")
            {
                GenerateCaptcha();
            }
            base.OnActionExecuting(filterContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void ClearPendingRegistration()
        {
            Session.Remove(PendingRegistrationKey);
            Session.Remove(RegistrationCodeKey);
            Session.Remove(RegistrationCodeExpiryKey);
        }

        private string GenerateNumericCode(int length = 6)
        {
            var random = new Random();
            return string.Concat(Enumerable.Range(0, length).Select(_ => random.Next(0, 10).ToString()));
        }

        private RegisterViewModel CreatePendingModel(RegisterViewModel source)
        {
            return new RegisterViewModel
            {
                TKH = source.TKH,
                TenDN = source.TenDN,
                SDT = source.SDT,
                CMND_CCCD = source.CMND_CCCD,
                DiaChi = source.DiaChi,
                MatKhau = source.MatKhau,
                ConfirmMatKhau = source.ConfirmMatKhau,
                Email = string.IsNullOrWhiteSpace(source.Email) ? null : source.Email.Trim()
            };
        }

        private ActionResult RedirectToLocalOrDefault(string returnUrl, string blockedAction)
        {
            var safeReturnUrl = GetSafeLocalUrl(returnUrl, blockedAction);
            if (!string.IsNullOrWhiteSpace(safeReturnUrl))
            {
                return Redirect(safeReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private string GetSafeLocalUrl(string returnUrl, string blockedAction)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return null;
            }

            if (Url != null && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            SecurityAuditLogger.Log("auth", blockedAction, "warning", new Dictionary<string, object>
            {
                { "returnUrl", returnUrl },
                { "ip", SecurityAuditLogger.GetClientIp(Request) }
            });
            return null;
        }

        private string GenerateGoogleAuthUrl(string clientId, string redirectUri, string returnUrl = null)
        {
            string googleOAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
            var queryParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("response_type", "code"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("scope", "openid email profile"),
                new KeyValuePair<string, string>("access_type", "online")
            };

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                queryParams.Add(new KeyValuePair<string, string>("state", returnUrl));
            }

            string queryString = string.Join("&", queryParams.Select(q => $"{q.Key}={Uri.EscapeDataString(q.Value)}"));
            return $"{googleOAuthUrl}?{queryString}";
        }

        private async Task<string> ExchangeCodeForTokenAsync(string code, string clientId, string redirectUri, string clientSecret)
        {
            string tokenEndpoint = "https://oauth2.googleapis.com/token";
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                });

                var response = await client.PostAsync(tokenEndpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(responseString);

                if (jsonResponse["error"] != null)
                {
                    throw new Exception($"Error exchanging code: {jsonResponse["error_description"]}");
                }

                return jsonResponse["access_token"]?.ToString();
            }
        }

        private async Task<dynamic> GetGoogleUserInfoAsync(string accessToken)
        {
            string userInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var response = await client.GetAsync(userInfoEndpoint);
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic userInfo = Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);

                if (userInfo["error"] != null)
                {
                    throw new Exception($"Error fetching user info: {userInfo["error"]["message"]}");
                }

                return userInfo;
            }
        }

        private bool ValidateRegistrationData(RegisterViewModel model, out string errorMessage)
        {
            errorMessage = null;
            var normalizedUsername = model.TenDN?.Trim();
            if (db.KHACHHANGs.Any(k => k.SDT == model.SDT))
            {
                errorMessage = "So dien thoai da duoc su dung.";
                return false;
            }

            if (db.KHACHHANGs.Any(k => k.CMND_CCCD == model.CMND_CCCD))
            {
                errorMessage = "CMND/CCCD da duoc su dung.";
                return false;
            }

            if (db.KHACHHANGs.Any(k => k.TenDN == normalizedUsername))
            {
                errorMessage = "Ten dang nhap da duoc su dung.";
                return false;
            }

            return true;
        }

        private bool TryCreateCustomer(RegisterViewModel model, out KHACHHANG customer, out string errorMessage)
        {
            customer = null;
            errorMessage = null;

            model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            if (!ValidateRegistrationData(model, out errorMessage))
            {
                return false;
            }

            int newMKH = db.KHACHHANGs.Any() ? db.KHACHHANGs.Max(k => k.MKH) + 1 : 1;
            customer = new KHACHHANG
            {
                MKH = newMKH,
                TKH = model.TKH,
                SDT = model.SDT,
                CMND_CCCD = model.CMND_CCCD,
                DiaChi = model.DiaChi,
                TenDN = model.TenDN?.Trim(),
                MatKhau = PasswordHasher.HashPassword(model.MatKhau),
                Email = model.Email
            };

            try
            {
                db.KHACHHANGs.Add(customer);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Co loi xay ra khi dang ky: " + ex.Message;
                return false;
            }
        }
    }
}

