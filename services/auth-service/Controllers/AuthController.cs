//using Microsoft.AspNetCore.Mvc;
//using auth_service.DTOs;
//using auth_service.Services;
//using Microsoft.AspNetCore.Authorization;
//namespace auth_service.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AuthController : ControllerBase
//    {
//        private readonly JwtService _jwtService;

//        public AuthController(JwtService jwtService)
//        {
//            _jwtService = jwtService;
//        }

//        [HttpPost("login")]
//        public IActionResult Login([FromBody] LoginRequest request)
//        {
//            Console.WriteLine("EMAIL: " + request?.Email);
//            Console.WriteLine("PASSWORD: " + request?.Password);
//            if (request == null)
//            {
//                return BadRequest("Request null");
//            }

//            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
//            {
//                return BadRequest("Missing email or password");
//            }

//            if (request.Email != "test@gmail.com" || request.Password != "123456")
//            {
//                return Unauthorized();
//            }

//            var token = _jwtService.GenerateToken(request.Email);

//            return Ok(new { token });
//        }
//        [Authorize]
//        [HttpGet("profile")]
//        public IActionResult Profile()
//        {
//            return Ok(new
//            {
//                message = "Đã đăng nhập bằng JWT",
//                user = User.Identity.Name
//            });
//        }
//    }
//}
using auth_service.Data;
using auth_service.DTOs;
using auth_service.Services;
using Microsoft.AspNetCore.Mvc;
using auth_service.Helpers;
namespace auth_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly AppDbContext _db;

        public AuthController(JwtService jwtService, AppDbContext db)
        {
            _jwtService = jwtService;
            _db = db;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.Username) ||
                string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Missing data");
            }

            var user = _db.KhachHangs
                .FirstOrDefault(x => x.TenDN == request.Username);

            if (user == null)
                return Unauthorized("User not found");

            bool ok = false;

            try
            {
                ok = PasswordHasher.VerifyPassword(
                    user.MatKhau,
                    request.Password,
                    out _
                );
            }
            catch
            {
                ok = false;
            }

            if (!ok)
            {
                ok = user.MatKhau == request.Password;
            }

            if (!ok)
                return Unauthorized("Sai mật khẩu");

            var token = _jwtService.GenerateToken(user.MKH.ToString());

            return Ok(new
            {
                token,
                userId = user.MKH,
                username = user.TKH
            });
        }
    }
}   