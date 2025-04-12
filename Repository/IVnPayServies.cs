using UltraStrore.Helper;

namespace UltraStrore.Repository
{
    public interface IVnPayServies
    {
        string CreatePaymentUrl(HttpContext httpContext ,VnPaymentRequest request);
        VnPaymentResponse PaymentExecute(IQueryCollection collections);
    }
}
