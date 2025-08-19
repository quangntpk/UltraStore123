namespace UltraStrore.Repository
{
    public interface ITelegramServices
    {
        Task SendOrderNotificationAsync(int orderId);
        Task SendNewOrderNotificationAsync(object orderData);
        Task TestConnectionAsync();
        List<string> GetAllowedChatIds();
    }
}