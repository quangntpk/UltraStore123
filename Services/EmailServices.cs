using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Net.Mail;
using UltraStrore.Repository;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UltraStrore.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly IConfiguration _configuration;

        public EmailServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpEmailAsync(string email, string otp)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("UltraStore", emailSettings["SenderEmail"]));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Mã OTP để đặt lại mật khẩu từ UltraStore";

            var bodyBuilder = new BodyBuilder();

            // Nội dung HTML cho email
            string htmlBody = @"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            margin: 0;
                            padding: 0;
                        }
                        .container {
                            max-width: 600px;
                            margin: 0 auto;
                            background-color: #ffffff;
                            padding: 20px;
                            border-radius: 8px;
                            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                        }
                        .header {
                            text-align: center;
                            padding: 20px 0;
                        }
                        .header img {
                            max-width: 150px;
                            height: auto;
                        }
                        .content {
                            text-align: center;
                            padding: 20px;
                        }
                        .otp {
                            font-size: 36px;
                            font-weight: bold;
                            color: #e91e63;
                            margin: 20px 0;
                            letter-spacing: 5px;
                        }
                        .footer {
                            text-align: center;
                            padding: 20px;
                            font-size: 14px;
                            color: #777;
                            border-top: 1px solid #ddd;
                            margin-top: 20px;
                        }
                        .footer a {
                            color: #e91e63;
                            text-decoration: none;
                        }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <img src='https://your-logo-url.com/logo.png' alt='UltraStore Logo'>
                            <h2>Xin chào từ UltraStore!</h2>
                        </div>
                        <div class='content'>
                            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                            <p>Mã OTP của bạn là:</p>
                            <div class='otp'>" + otp + @"</div>
                            <p>Mã này sẽ hết hạn sau <strong>10 phút</strong>.</p>
                            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 UltraStore. Tất cả quyền được bảo lưu.</p>
                            <p>Liên hệ với chúng tôi: <a href='mailto:support@ultrastore.com'>support@ultrastore.com</a></p>
                            <p>Theo dõi chúng tôi: 
                                <a href='https://facebook.com/ultrastore'>Facebook</a> | 
                                <a href='https://instagram.com/ultrastore'>Instagram</a>
                            </p>
                        </div>
                    </div>
                </body>
                </html>";

            // Gán nội dung HTML vào email
            bodyBuilder.HtmlBody = htmlBody;

            // Nội dung dạng text (dự phòng cho các email client không hỗ trợ HTML)
            bodyBuilder.TextBody = $"Mã OTP của bạn là: {otp}. Mã này sẽ hết hạn sau 10 phút.";
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}