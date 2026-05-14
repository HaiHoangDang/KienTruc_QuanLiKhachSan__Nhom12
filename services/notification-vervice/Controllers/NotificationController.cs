using Microsoft.AspNetCore.Mvc;
using notification_service.DTOs;
using notification_service.Services.Interfaces;

namespace notification_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] NotificationRequest request)
        {
            try
            {
                var result = await _service.Send(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("booking-success")]
        public async Task<IActionResult> BookingSuccess([FromBody] NotificationRequest request)
        {
            try
            {
                request.Title = string.IsNullOrWhiteSpace(request.Title)
                    ? "Đặt phòng thành công"
                    : request.Title;

                request.Type = string.IsNullOrWhiteSpace(request.Type)
                    ? "BOOKING_SUCCESS"
                    : request.Type;

                var result = await _service.Send(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                message = "notification-service hoạt động"
            });
        }
    }
}