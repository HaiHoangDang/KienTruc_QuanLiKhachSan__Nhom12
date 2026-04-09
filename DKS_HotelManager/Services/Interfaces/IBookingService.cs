using System.Collections.Generic;
using DKS_HotelManager.DTOs.Booking;

namespace DKS_HotelManager.Services.Interfaces
{
    public interface IBookingService
    {
        List<BookingResponse> GetAll();
        BookingResponse GetById(int id);
        BookingResponse Create(CreateBookingRequest request);
        void Cancel(int id);
        void UpdateStatus(int id, string trangThai);
    }
}