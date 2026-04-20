using room_service.DTOs;

namespace room_service.Services.Interfaces
{
    public interface IRoomService
    {
        Task<List<RoomResponse>> GetRooms();
        Task<RoomResponse?> GetById(int id);
    }
}