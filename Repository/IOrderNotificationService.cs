using UltraStrore.Models.DTOs;

namespace UltraStrore.Repository
{
    public interface IOrderNotificationService
    {
        Task SendOrderStatusNotificationAsync(string email, int orderId, string statusMessage);
        Task SendOrderStatusNotificationFromFrontendAsync(EmailOrderDto dto);
    }
}