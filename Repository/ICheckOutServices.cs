using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.DTO;

namespace UltraStrore.Repository
{
    public interface ICheckOutServices
    {
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequestDto request, HttpContext httpContext);
        Task<PaymentResponse> InstantCheckout(PaymentRequestDto1 request, HttpContext httpContext);
        Task ProcessVnPayCallbackAsync(IQueryCollection query, HttpContext httpContext);
    }
}
