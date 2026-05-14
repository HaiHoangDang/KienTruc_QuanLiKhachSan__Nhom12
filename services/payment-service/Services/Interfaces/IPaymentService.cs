using payment_service.DTOs;

namespace payment_service.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<List<PaymentResponse>> GetAll();

        Task<PaymentResponse?> GetById(int id);

        Task<PaymentResponse> Create(PaymentRequest request);
    }
}