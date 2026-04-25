using booking_service.DTOs;

namespace booking_service.Services.Interfaces
{
    public interface IBookingService
    {
        Task<List<BookingResponse>> GetAll();
        Task<BookingResponse?> GetById(int id);
        Task<BookingResponse> Create(BookingRequest request, int userId);
    }
}