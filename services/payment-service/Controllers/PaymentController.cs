using Microsoft.AspNetCore.Mvc;
using payment_service.DTOs;
using payment_service.Services.Interfaces;

namespace payment_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentController(IPaymentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _service.GetById(id);

            if (payment == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy thanh toán."
                });
            }

            return Ok(payment);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentRequest request)
        {
            try
            {
                var result = await _service.Create(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                var error = ex.Message;

                var inner = ex.InnerException;

                while (inner != null)
                {
                    error += " | INNER: " + inner.Message;
                    inner = inner.InnerException;
                }

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
                message = "payment-service hoạt động"
            });
        }
    }
}