using Telegram.Bot;
using Telegram.Bot.Types;
using System.Text;
using UltraStrore.Repository;
using UltraStrore.Data;
using Microsoft.EntityFrameworkCore;

namespace UltraStrore.Services
{
    public class TelegramServices : ITelegramServices
    {
        private readonly TelegramBotClient _botClient;
        private readonly List<string> _allowedChatIds;
        private readonly ILogger<TelegramServices> _logger;
        private readonly ApplicationDbContext _context;

        public TelegramServices(
            IConfiguration configuration,
            ILogger<TelegramServices> logger,
            ApplicationDbContext context)
        {
            var botToken = configuration["Telegram:BotToken"];
            _logger = logger;
            _context = context;

            _allowedChatIds = configuration.GetSection("Telegram:AllowedChatIds")
                .Get<List<string>>() ?? new List<string>();

            if (string.IsNullOrEmpty(botToken) || !_allowedChatIds.Any())
            {
                _logger.LogWarning("Telegram configuration is missing or no allowed chat IDs configured. Bot notifications will not work.");
                return;
            }

            _botClient = new TelegramBotClient(botToken);
            _logger.LogInformation($"Telegram Bot initialized with {_allowedChatIds.Count} allowed chat IDs");
        }

        public async Task SendOrderNotificationAsync(int orderId)
        {
            try
            {
                if (_botClient == null || !_allowedChatIds.Any())
                {
                    _logger.LogWarning("Telegram bot is not configured or no allowed chat IDs");
                    return;
                }

                var order = await _context.DonHangs
                    .FirstOrDefaultAsync(d => d.MaDonHang == orderId);

                if (order == null)
                {
                    _logger.LogWarning($"Order {orderId} not found");
                    return;
                }

                var message = FormatSimpleOrderMessage(order);

                var successCount = 0;
                var failureCount = 0;

                foreach (var chatId in _allowedChatIds)
                {
                    try
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: message,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                        );
                        successCount++;
                        _logger.LogInformation($"Order notification sent to chat {chatId} for order {orderId}");
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(ex, $"Failed to send notification to chat {chatId} for order {orderId}");
                    }
                }

                _logger.LogInformation($"Order {orderId} notification summary: {successCount} success, {failureCount} failures");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send Telegram notification for order {orderId}");
            }
        }

        public async Task SendNewOrderNotificationAsync(object orderData)
        {
            try
            {
                if (_botClient == null || !_allowedChatIds.Any())
                {
                    _logger.LogWarning("Telegram bot is not configured or no allowed chat IDs");
                    return;
                }

                var message = FormatOrderMessage(orderData);

                var successCount = 0;
                var failureCount = 0;

                foreach (var chatId in _allowedChatIds)
                {
                    try
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: message,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                        );
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(ex, $"Failed to send notification to chat {chatId}");
                    }
                }

                _logger.LogInformation($"New order notification summary: {successCount} success, {failureCount} failures");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send new order Telegram notification");
            }
        }

        public async Task TestConnectionAsync()
        {
            try
            {
                if (_botClient == null || !_allowedChatIds.Any())
                {
                    _logger.LogWarning("Telegram bot is not configured or no allowed chat IDs");
                    return;
                }

                var me = await _botClient.GetMe();

                var testMessage = $"🤖 <b>Bot Test Connection</b>\n" +
                                 $"Bot Name: {me.FirstName}\n" +
                                 $"Username: @{me.Username}\n" +
                                 $"Time: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
                                 $"Total Recipients: {_allowedChatIds.Count}";

                var successCount = 0;
                var failureCount = 0;

                foreach (var chatId in _allowedChatIds)
                {
                    try
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: testMessage,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                        );
                        successCount++;
                        _logger.LogInformation($"Test message sent to chat {chatId}");
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(ex, $"Failed to send test message to chat {chatId}");
                    }
                }

                _logger.LogInformation($"Test message summary: {successCount} success, {failureCount} failures");

                if (successCount == 0)
                {
                    throw new Exception($"Failed to send test message to all {_allowedChatIds.Count} recipients");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test message to Telegram");
                throw;
            }
        }

        public List<string> GetAllowedChatIds()
        {
            return _allowedChatIds.ToList();
        }

        // ✅ THÔNG BÁO ĐƠN GIẢN CHO ĐƠN HÀNG THỰC
        private string FormatSimpleOrderMessage(DonHang order)
        {
            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("🛒 <b>ĐƠN HÀNG MỚI</b>");
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine($"📋 <b>Mã đơn hàng:</b> #{order.MaDonHang}");
                sb.AppendLine($"💵 <b>Tổng tiền:</b> {order.FinalAmount:N0}đ");

                // ✅ Phương thức thanh toán đơn giản
                var paymentMethod = order.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang
                    ? "💳 COD"
                    : "✅ VNPay";

                sb.AppendLine($"💰 <b>Phương thức:</b> {paymentMethod}");
                sb.AppendLine($"🕐 <b>Thời gian:</b> {DateTime.Now:dd/MM/yyyy HH:mm}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting simple order message");
                return $"🛒 <b>ĐƠN HÀNG MỚI</b>\n" +
                       $"📋 <b>Mã đơn hàng:</b> #{order.MaDonHang}\n" +
                       $"⚠️ Lỗi hiển thị chi tiết\n" +
                       $"🕐 <b>Thời gian:</b> {DateTime.Now:dd/MM/yyyy HH:mm}";
            }
        }

        private string FormatOrderMessage(dynamic orderData)
        {
            try
            {
                if (HasProperty(orderData, "CustomMessage") && GetPropertyValue(orderData, "CustomMessage") != null)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("📢 <b>THÔNG BÁO</b>");
                    sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
                    sb.AppendLine(GetPropertyValue(orderData, "CustomMessage").ToString());
                    sb.AppendLine();
                    sb.AppendLine($"🕐 <b>Thời gian:</b> {DateTime.Now:dd/MM/yyyy HH:mm}");
                    return sb.ToString();
                }

                // ✅ FALLBACK VỀ FORMAT ĐƠN GIẢN
                var orderId = GetPropertyValue(orderData, "MaDonHang");
                var finalAmount = GetPropertyValue(orderData, "FinalAmount");
                var trangThaiHang = GetPropertyValue(orderData, "TrangThaiHang");

                var sb2 = new StringBuilder();
                sb2.AppendLine("🛒 <b>ĐƠN HÀNG MỚI</b>");
                sb2.AppendLine("━━━━━━━━━━━━━━━━━━━━");
                sb2.AppendLine($"📋 <b>Mã đơn hàng:</b> #{orderId}");

                // ✅ SỬA: Khai báo biến bên ngoài if
                decimal amount = 0;
                if (finalAmount != null && decimal.TryParse(finalAmount.ToString(), out amount))
                {
                    sb2.AppendLine($"💵 <b>Tổng tiền:</b> {amount:N0}đ");
                }
                else
                {
                    sb2.AppendLine($"💵 <b>Tổng tiền:</b> Không xác định");
                }

                // ✅ SỬA: Khai báo biến bên ngoài if  
                int status = -1;
                var paymentMethod = "❓ Không xác định";
                if (trangThaiHang != null && int.TryParse(trangThaiHang.ToString(), out status))
                {
                    paymentMethod = status == 0 ? "💳 COD" : "✅ VNPay";
                }

                sb2.AppendLine($"💰 <b>Phương thức:</b> {paymentMethod}");
                sb2.AppendLine($"🕐 <b>Thời gian:</b> {DateTime.Now:dd/MM/yyyy HH:mm}");

                return sb2.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting order message");
                return $"🛒 <b>ĐƠN HÀNG MỚI</b>\n" +
                       $"⚠️ Có lỗi khi format tin nhắn\n" +
                       $"🕐 <b>Thời gian:</b> {DateTime.Now:dd/MM/yyyy HH:mm}";
            }
        }

        private bool HasProperty(dynamic obj, string propertyName)
        {
            try
            {
                if (obj == null) return false;
                var type = obj.GetType();
                return type.GetProperty(propertyName) != null;
            }
            catch
            {
                return false;
            }
        }

        private object GetPropertyValue(dynamic obj, string propertyName)
        {
            try
            {
                if (obj == null) return null;
                var type = obj.GetType();
                var property = type.GetProperty(propertyName);
                return property?.GetValue(obj);
            }
            catch
            {
                return null;
            }
        }
    }
}