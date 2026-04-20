using Microsoft.AspNetCore.Mvc;
using room_service.Services.Interfaces;

namespace room_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _service;

        public RoomController(IRoomService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _service.GetRooms();
            return Ok(rooms);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var room = await _service.GetById(id);

            if (room == null)
                return NotFound("Room không tồn tại");

            return Ok(room);
        }
    }
}