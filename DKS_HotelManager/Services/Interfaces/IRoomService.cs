using System.Collections.Generic;
using DKS_HotelManager.DTOs.Room;

namespace DKS_HotelManager.Services.Interfaces
{
    public interface IRoomService
    {
        List<RoomResponse> GetAll();
        RoomResponse GetById(int id);
        RoomResponse Create(CreateRoomRequest request);
        void Update(UpdateRoomRequest request);
        void Delete(int id);
    }
}