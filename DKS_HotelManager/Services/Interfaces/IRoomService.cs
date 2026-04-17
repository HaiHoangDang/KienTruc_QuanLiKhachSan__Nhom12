using DKS_HotelManager.DTOs.Room;
using DKS_HotelManager.Models;
using DKS_HotelManager.DTOs.Common;
using System.Collections.Generic;

namespace DKS_HotelManager.Services.Interfaces
{
    public interface IRoomService
    {
        List<RoomResponse> GetAll();
        RoomResponse GetById(int id);
        RoomResponse Create(CreateRoomRequest request);
        void Update(UpdateRoomRequest request);
        void Delete(int id);
        List<DropdownItem> GetHotels();
        List<DropdownItem> GetRoomTypes();
        //List<KHACHSAN> GetHotels();
        //List<LOAIPHONG> GetRoomTypes();
    }
}