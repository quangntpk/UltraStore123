using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using UltraStrore.Data;
using UltraStrore.Models.DTOs;
using UltraStrore.Repository;
using System.IO;

namespace UltraStrore.Services
{
    public class OrderNotificationService : IOrderNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public OrderNotificationService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // Gửi email từ BE với QR code tự tạo
        public async Task SendOrderStatusNotificationAsync(string email, int orderId, string statusMessage)
        {
            var order = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaSanPhamNavigation)
                .Include(d => d.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == orderId);

            if (order == null || string.IsNullOrWhiteSpace(email)) return;

            // Tạo link hóa đơn chính xác theo mã đơn hàng
            string qrLink = $"http://localhost:8080/user/hoadon?orderId={order.MaDonHang}";

            // Tạo QR code dưới dạng mảng byte
            byte[] qrBytes = GenerateQrCodeAsBytes(qrLink);
            string qrBase64 = Convert.ToBase64String(qrBytes);

            // Lấy thông tin cấu hình email
            var emailSettings = _configuration.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("UltraStore", emailSettings["SenderEmail"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = $"Cập nhật trạng thái đơn hàng ORD-{order.MaDonHang:D5}";

            // Nội dung HTML của email
            var htmlBody = $@"
<!DOCTYPE html>
<html lang='vi'>
<head><meta charset='UTF-8'><title>Xác nhận đơn hàng</title></head>
<body style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8f8f8;'>
  <div style='background-color: white; padding: 20px; border-radius: 8px; max-width: 600px; margin: auto;'>
    <h2 style='color: #e91e63;'>UltraStore - Đơn hàng xác nhận</h2>
    <p>Xin chào <strong>{order.TenNguoiNhan ?? order.MaNguoiDungNavigation?.HoTen ?? "khách hàng"}</strong>,</p>
    <p>{statusMessage}</p>
    <p>Mã đơn hàng: <strong>ORD-{order.MaDonHang:D5}</strong></p>
    <p>Chi tiết đơn hàng: <a href='{qrLink}'>Xem tại đây</a></p>
    <p>Quét mã QR để truy cập nhanh:</p>
    <img src='data:image/png;base64,{qrBase64}' alt='QR Code' width='150' height='150' />
    <p style='color: #888; font-size: 13px;'>Cảm ơn bạn đã mua sắm tại UltraStore!</p>
  </div>
</body>
</html>";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = $"Trạng thái đơn hàng ORD-{order.MaDonHang:D5}: {statusMessage}\nXem chi tiết: {qrLink}"
            };

            // 👉 Đính kèm file QR code dạng png
            bodyBuilder.Attachments.Add("QRCode_Order.png", qrBytes, new ContentType("image", "png"));

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        // Tạo QR code dưới dạng mảng byte (dùng cho cả inline và đính kèm)
        private byte[] GenerateQrCodeAsBytes(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            return qrCode.GetGraphic(20);
        }

        // Gửi từ Frontend truyền sẵn QR code
        public async Task SendOrderStatusNotificationFromFrontendAsync(EmailOrderDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.QrBase64)) return;

            var emailSettings = _configuration.GetSection("EmailSettings");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("UltraStore", emailSettings["SenderEmail"]));
            message.To.Add(MailboxAddress.Parse(dto.Email));
            message.Subject = $"Xác nhận đơn hàng ORD-{dto.OrderId:D5}";

            var htmlBody = $@"
<!DOCTYPE html>
<html lang='vi'>
<head><meta charset='UTF-8'><title>Xác nhận đơn hàng</title></head>
<body style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8f8f8;'>
  <div style='background-color: white; padding: 20px; border-radius: 8px; max-width: 600px; margin: auto;'>
    <h2 style='color: #e91e63;'>UltraStore - Đơn hàng xác nhận</h2>
    <p>Xin chào <strong>{dto.Name ?? "khách hàng"}</strong>,</p>
    <p>Đơn hàng của bạn đã được xác nhận thành công.</p>
    <p>Mã đơn hàng: <strong>ORD-{dto.OrderId:D5}</strong></p>
    <p>Xem chi tiết: <a href='http://localhost:8080/user/hoadon?orderId={dto.OrderId}'>tại đây</a></p>
    <p>Quét mã QR để truy cập nhanh:</p>
        /*<img src='data:image/png;base64,{dto.QrBase64}' alt='QR Code' width='150' height='150' />*/
    <p style='color: #888; font-size: 13px;'>Cảm ơn bạn đã mua sắm tại UltraStore!</p>
  </div>
</body>
</html>";

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = $"Đơn hàng ORD-{dto.OrderId:D5} xác nhận thành công.\nLink: http://localhost:8080/user/hoadon?orderId={dto.OrderId}"
            };

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
