using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.DTOs;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class OrderNotificationService : IOrderNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IQRCodeService _qrCodeService;

        public OrderNotificationService(
            IConfiguration configuration,
            ApplicationDbContext context,
            IQRCodeService qrCodeService)
        {
            _configuration = configuration;
            _context = context;
            _qrCodeService = qrCodeService;
        }

        public async Task SendOrderStatusNotificationAsync(string email, int orderId, string statusMessage)
        {
            try
            {
                var order = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(cd => cd.MaSanPhamNavigation)
                    .Include(d => d.MaNguoiDungNavigation)
                    .FirstOrDefaultAsync(d => d.MaDonHang == orderId);

                if (order == null || string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine($"[ERROR] Order {orderId} not found or email is empty");
                    return;
                }

                // Tạo link hóa đơn
                string qrLink = $"https://fashionhub.name.vn/user/hoadon?orderId={order.MaDonHang}";
                Console.WriteLine($"[DEBUG] Generated QR link: {qrLink}");

                // ✅ FIX: Tạo QR code với error handling tốt hơn
                byte[] qrBytes = null;
                string qrBase64 = "";
                string qrCid = "qrcode-attachment"; // Content-ID cho attachment

                try
                {
                    qrBytes = _qrCodeService.GenerateQRCode(qrLink, 12); // Tăng size lên 12
                    if (qrBytes != null && qrBytes.Length > 0)
                    {
                        qrBase64 = Convert.ToBase64String(qrBytes);
                        Console.WriteLine($"[DEBUG] QR code generated successfully, Base64 length: {qrBase64.Length}");

                        // Validate Base64 string
                        if (string.IsNullOrEmpty(qrBase64) || qrBase64.Length < 100)
                        {
                            Console.WriteLine("[WARNING] Generated Base64 seems too short, regenerating...");
                            qrBytes = _qrCodeService.GenerateQRCodeAlternative(qrLink, 12);
                            qrBase64 = Convert.ToBase64String(qrBytes);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("QR bytes is null or empty");
                    }
                }
                catch (Exception qrEx)
                {
                    Console.WriteLine($"[ERROR] QR generation failed: {qrEx.Message}");
                    // Fallback: Tạo QR đơn giản
                    try
                    {
                        qrBytes = _qrCodeService.GenerateQRCodeAlternative(qrLink, 10);
                        qrBase64 = Convert.ToBase64String(qrBytes);
                        Console.WriteLine("[INFO] Used alternative QR generation method");
                    }
                    catch (Exception fallbackEx)
                    {
                        Console.WriteLine($"[ERROR] Even fallback QR failed: {fallbackEx.Message}");
                        qrBytes = null;
                        qrBase64 = "";
                    }
                }

                // Lấy thông tin cấu hình email
                var emailSettings = _configuration.GetSection("EmailSettings");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("FashionHub", emailSettings["SenderEmail"]));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = $"Cập nhật trạng thái đơn hàng ORD-{order.MaDonHang:D5}";

                // ✅ FIX: Cải thiện HTML email với multiple QR display methods
                string qrDisplayHtml = "";

                if (!string.IsNullOrEmpty(qrBase64) && qrBytes != null)
                {
                    qrDisplayHtml = $@"
                <div style='text-align: center; margin-bottom: 25px;'>
                    <p style='font-size: 14px; margin-bottom: 15px; font-weight: 600;'>
                        <strong>Chi tiết đơn hàng:</strong> 
                        <a href='{qrLink}' style='color: #e91e63; text-decoration: none; padding: 8px 16px; background-color: #fce4ec; border-radius: 4px; margin-left: 8px;'>
                            Xem tại đây
                        </a>
                    </p>
                    <p style='font-size: 14px; margin-bottom: 15px; color: #666;'>Quét mã QR để truy cập nhanh:</p>
                    
                    <!-- Method 1: Embedded Base64 -->
                   
                    
                    <!-- Method 2: Reference to attachment -->
                    <div style='margin-top: 10px;'>
                        <img 
                            src='cid:{qrCid}' 
                            alt='QR Code đơn hàng ORD-{order.MaDonHang:D5}' 
                            style='width: 150px; height: 150px; display: block; margin: 0 auto; border: 1px solid #ddd; border-radius: 8px;' 
                        />
                    </div>
                </div>";
                }
                else
                {
                    // Fallback khi không có QR code
                    qrDisplayHtml = $@"
                <div style='text-align: center; margin-bottom: 25px;'>
                    <p style='font-size: 14px; margin-bottom: 15px; font-weight: 600;'>
                        <strong>Chi tiết đơn hàng:</strong>
                    </p>
                    <div style='display: inline-block; padding: 15px 25px; background-color: #e91e63; border-radius: 8px;'>
                        <a href='{qrLink}' style='color: white; text-decoration: none; font-weight: 600; font-size: 16px;'>
                            👆 XEM ĐỚN HÀNG CHI TIẾT
                        </a>
                    </div>
                    <p style='font-size: 12px; color: #999; margin-top: 10px;'>QR code tạm thời không khả dụng</p>
                </div>";
                }

                // ✅ Nội dung HTML được cải thiện
                var htmlBody = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Xác nhận đơn hàng ORD-{order.MaDonHang:D5}</title>
    <style>
        @media only screen and (max-width: 600px) {{
            .container {{ padding: 15px !important; }}
            .qr-image {{ width: 120px !important; height: 120px !important; }}
        }}
    </style>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f8f9fa;'>
    <div class='container' style='background-color: white; padding: 30px; border-radius: 12px; max-width: 600px; margin: 0 auto; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
        
        <!-- Header -->
        <div style='text-align: center; margin-bottom: 30px; padding-bottom: 20px; border-bottom: 3px solid #e91e63;'>
            <h1 style='color: #e91e63; margin: 0; font-size: 28px; font-weight: bold;'>
                📦 FashionHub
            </h1>
            <p style='color: #666; margin: 8px 0 0 0; font-size: 16px;'>Xác nhận đơn hàng</p>
        </div>
        
        <!-- Greeting -->
        <div style='margin-bottom: 25px;'>
            <p style='font-size: 18px; margin-bottom: 10px; color: #333;'>
                Xin chào <strong style='color: #e91e63;'>{order.TenNguoiNhan ?? order.MaNguoiDungNavigation?.HoTen ?? "khách hàng"}</strong>,
            </p>
            <p style='font-size: 16px; color: #555; line-height: 1.6; background-color: #f8f9fa; padding: 15px; border-radius: 8px; border-left: 4px solid #e91e63;'>
                {statusMessage}
            </p>
        </div>
        
        <!-- Order Info -->
        <div style='background-color: #f8f9fa; padding: 25px; border-radius: 8px; margin-bottom: 25px; border: 1px solid #e9ecef;'>
            <h3 style='margin: 0 0 15px 0; color: #333; font-size: 18px;'>📋 Thông tin đơn hàng</h3>
            <table style='width: 100%; border-collapse: collapse;'>
                <tr>
                    <td style='padding: 8px 0; font-weight: 600; color: #555; width: 40%;'>Mã đơn hàng:</td>
                    <td style='padding: 8px 0; color: #e91e63; font-size: 18px; font-weight: bold;'>ORD-{order.MaDonHang:D5}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; font-weight: 600; color: #555;'>Ngày đặt:</td>
                    <td style='padding: 8px 0; color: #333;'>{order.NgayDat:dd/MM/yyyy HH:mm}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; font-weight: 600; color: #555;'>Tổng tiền:</td>
                    <td style='padding: 8px 0; color: #28a745; font-size: 18px; font-weight: bold;'>{order.FinalAmount:N0} VND</td>
                </tr>
            </table>
        </div>
        
        <!-- QR Code Section -->
        {qrDisplayHtml}
        
        <!-- Footer -->
        <div style='border-top: 2px solid #e9ecef; padding-top: 25px; text-align: center; margin-top: 30px;'>
            <div style='background-color: #e91e63; color: white; padding: 20px; border-radius: 8px; margin-bottom: 15px;'>
                <h3 style='margin: 0 0 10px 0; font-size: 18px;'>🙏 Cảm ơn bạn đã mua sắm tại FashionHub!</h3>
                <p style='margin: 0; font-size: 14px; opacity: 0.9;'>Chúng tôi luôn nỗ lực mang đến dịch vụ tốt nhất</p>
            </div>
            
            <div style='color: #666; font-size: 13px; line-height: 1.5;'>
                <p style='margin: 5px 0;'>📞 Hotline hỗ trợ: <strong>1900-xxxx</strong></p>
                <p style='margin: 5px 0;'>📧 Email: support@FashionHub.com</p>
                <p style='margin: 5px 0;'>🌐 Website: <a href='https://fashionhub.name.vn' style='color: #e91e63; text-decoration: none;'>FashionHub.com</a></p>
            </div>
        </div>
    </div>
</body>
</html>";

                // ✅ FIX: Tạo email body với attachment
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody,
                    TextBody = $@"
=== FashionHub - XÁC NHẬN ĐƠN HÀNG ===

Xin chào {order.TenNguoiNhan ?? order.MaNguoiDungNavigation?.HoTen ?? "khách hàng"},

{statusMessage}

THÔNG TIN ĐƠN HÀNG:
- Mã đơn hàng: ORD-{order.MaDonHang:D5}
- Ngày đặt: {order.NgayDat:dd/MM/yyyy HH:mm}
- Tổng tiền: {order.FinalAmount:N0} VND

Xem chi tiết đơn hàng tại: {qrLink}

Cảm ơn bạn đã mua sắm tại FashionHub!
Hotline: 1900-xxxx
            "
                };

                // ✅ FIX: Đính kèm QR code với Content-ID
                if (qrBytes != null && qrBytes.Length > 0)
                {
                    var attachment = bodyBuilder.Attachments.Add($"QRCode_Order_{order.MaDonHang}.png", qrBytes, new ContentType("image", "png"));
                    attachment.ContentId = qrCid; // Set Content-ID để reference trong HTML
                    Console.WriteLine($"[DEBUG] QR code attached with CID: {qrCid}");
                }

                message.Body = bodyBuilder.ToMessageBody();

                // Gửi email
                using var client = new SmtpClient();
                await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                Console.WriteLine($"[SUCCESS] Email sent successfully to {email} for order {orderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send email for order {orderId}: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task SendOrderStatusNotificationFromFrontendAsync(EmailOrderDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.QrBase64))
                {
                    Console.WriteLine("[ERROR] Invalid DTO data for frontend email");
                    return;
                }

                var emailSettings = _configuration.GetSection("EmailSettings");
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("FashionHub", emailSettings["SenderEmail"]));
                message.To.Add(MailboxAddress.Parse(dto.Email));
                message.Subject = $"Xác nhận đơn hàng ORD-{dto.OrderId:D5}";

                var htmlBody = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Xác nhận đơn hàng</title>
</head>
<body style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8f8f8; margin: 0;'>
    <div style='background-color: white; padding: 30px; border-radius: 8px; max-width: 600px; margin: auto; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
        <div style='text-align: center; margin-bottom: 30px;'>
            <h2 style='color: #e91e63; margin: 0; font-size: 24px;'>FashionHub - Đơn hàng xác nhận</h2>
        </div>
        
        <div style='margin-bottom: 25px;'>
            <p style='font-size: 16px; margin-bottom: 10px;'>Xin chào <strong>{dto.Name ?? "khách hàng"}</strong>,</p>
            <p style='font-size: 14px; color: #555; line-height: 1.6;'>Đơn hàng của bạn đã được xác nhận thành công.</p>
        </div>
        
        <div style='background-color: #f9f9f9; padding: 20px; border-radius: 5px; margin-bottom: 25px;'>
            <p style='margin: 0; font-size: 14px;'><strong>Mã đơn hàng:</strong> <span style='color: #e91e63; font-size: 16px;'>ORD-{dto.OrderId:D5}</span></p>
        </div>
        
        <div style='text-align: center; margin-bottom: 25px;'>
            <p style='font-size: 14px; margin-bottom: 15px;'><strong>Xem chi tiết:</strong> <a href='https://fashionhub.name.vn/user/hoadon?orderId={dto.OrderId}' style='color: #e91e63; text-decoration: none;'>tại đây</a></p>
            <p style='font-size: 14px; margin-bottom: 15px; color: #666;'>Quét mã QR để truy cập nhanh:</p>
            
        </div>
        
        <div style='border-top: 1px solid #eee; padding-top: 20px; text-align: center;'>
            <p style='color: #888; font-size: 13px; margin: 0;'>Cảm ơn bạn đã mua sắm tại FashionHub!</p>
        </div>
    </div>
</body>
</html>";

                var builder = new BodyBuilder
                {
                    HtmlBody = htmlBody,
                    TextBody = $"Đơn hàng ORD-{dto.OrderId:D5} xác nhận thành công.\nLink: https://fashionhub.name.vn/user/hoadon?orderId={dto.OrderId}"
                };

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                Console.WriteLine($"[SUCCESS] Frontend email sent successfully to {dto.Email} for order {dto.OrderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send frontend email: {ex.Message}");
                throw;
            }
        }
    }
}