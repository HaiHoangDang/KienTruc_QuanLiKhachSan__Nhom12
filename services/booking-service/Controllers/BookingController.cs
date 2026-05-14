//using Microsoft.AspNetCore.Mvc;
//using booking_service.DTOs;
//using booking_service.Services.Interfaces;
//using Microsoft.AspNetCore.Authorization;
//using System.Security.Claims;

//namespace booking_service.Controllers
//{
//    [Authorize]
//    [ApiController]
//    [Route("api/[controller]")]
//    public class BookingController : ControllerBase
//    {
//        private readonly IBookingService _service;

//        public BookingController(IBookingService service)
//        {
//            if (service == null)
//            {
//                throw new Exception("SERVICE IS NULL");
//            }

//            _service = service;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAll()
//        {
//            return Ok(await _service.GetAll());
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetById(int id)
//        {
//            var result = await _service.GetById(id);

//            if (result == null)
//                return NotFound();

//            return Ok(result);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(BookingRequest request)
//        {
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

//            if (string.IsNullOrEmpty(userIdClaim))
//                return Unauthorized("Missing user identity");

//            int userId = int.Parse(userIdClaim);

//            var result = await _service.Create(request, userId);

//            return Ok(result);
//        }

//        [HttpGet("test")]
//        public IActionResult Test()
//        {
//            return Ok(new
//            {
//                message = "JWT hợp lệ",
//                user = User.Identity?.Name
//            });
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using booking_service.DTOs;
using booking_service.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace booking_service.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _service;

        public BookingController(IBookingService service)
        {
            _service = service ?? throw new Exception("SERVICE IS NULL");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetById(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookingRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Missing user identity");
                }

                int customerId = int.Parse(userIdClaim);

                var result = await _service.Create(request, customerId);

                return Ok(result);
            }
            //catch (Exception ex)
            //{
            //    return BadRequest(new
            //    {
            //        message = ex.Message
            //    });
            //}
            catch (Exception ex)
            {
                var error = ex.Message;

                var inner = ex.InnerException;

                while (inner != null)
                {
                    error += " | INNER: " + inner.Message;
                    inner = inner.InnerException;
                }

                Console.WriteLine("[BOOKING ERROR] " + error);

                return BadRequest(new
                {
                    message = error
                });
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                message = "JWT hợp lệ",
                user = User.Identity?.Name,
                id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            });
        }
    }
}