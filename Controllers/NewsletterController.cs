using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

[Route("api/[controller]")]
[ApiController]
public class NewsletterController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public NewsletterController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("Subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || !IsValidEmail(request.Email))
        {
            return BadRequest("Invalid email address.");
        }

        try
        {
            await SendWelcomeEmail(request.Email);
            return Ok("Subscription successful!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to subscribe: {ex.Message}");
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private async Task SendWelcomeEmail(string email)
    {
        var smtpServer = _configuration["Smtp:Server"];
        var smtpPort = int.Parse(_configuration["Smtp:Port"]);
        var smtpUsername = _configuration["Smtp:Username"];
        var smtpPassword = _configuration["Smtp:Password"];

        using var client = new SmtpClient(smtpServer, smtpPort)
        {
            EnableSsl = true,
            Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword)
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpUsername, "FashionHub"),
            Subject = "Chào mừng bạn đến với FashionHub!",
            Body = $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Chào mừng bạn đến với FashionHub!</title>
                </head>
                <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
                    <table role='presentation' border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: #f4f4f4;'>
                        <tr>
                            <td align='center' style='padding: 20px 0;'>
                                <table role='presentation' border='0' cellpadding='0' cellspacing='0' width='600' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);'>
                                    <!-- Header -->
                                    <tr>
                                        <td style='background: linear-gradient(90deg, #6B46C1, #B794F4); border-top-left-radius: 8px; border-top-right-radius: 8px; padding: 20px; text-align: center;'>
                                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: bold;'>Chào mừng bạn đến với FashionHub!</h1>
                                        </td>
                                    </tr>
                                    <!-- Content -->
                                    <tr>
                                        <td style='padding: 40px 30px; color: #333333;'>
                                            <p style='font-size: 16px; line-height: 24px; margin: 0 0 16px;'>Cảm ơn bạn đã đăng ký nhận tin tức từ chúng tôi! Bạn sẽ nhận được những cập nhật mới nhất về các bộ sưu tập, ưu đãi độc quyền và nhiều hơn nữa.</p>
                                            <p style='font-size: 16px; line-height: 24px; margin: 0 0 16px;'><strong>Ngày đăng ký:</strong> {System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}</p>
                                            <p style='font-size: 16px; line-height: 24px; margin: 0 0 24px;'>Thời trang cao cấp, bền vững và giao hàng nhanh chóng - tất cả đang chờ bạn tại FashionHub.</p>
                                            <!-- CTA Button -->
                                            <table role='presentation' border='0' cellpadding='0' cellspacing='0' style='margin: 0 auto;'>
                                                <tr>
                                                    <td style='background-color: #6B46C1; border-radius: 5px;'>
                                                        <a href='http://localhost:8080' style='display: inline-block; padding: 12px 24px; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 5px;'>Khám phá ngay</a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <!-- Footer -->
                                    <tr>
                                        <td style='background-color: #f8f8f8; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px; padding: 20px; text-align: center;'>
                                            <p style='font-size: 14px; color: #666666; margin: 0 0 8px;'>Nếu bạn không muốn nhận thêm email, hãy <a href='http://localhost:8080' style='color: #6B46C1; text-decoration: underline;'>hủy đăng ký</a>.</p>
                                            <p style='font-size: 14px; color: #666666; margin: 0;'>© 2025 FashionHub. All rights reserved.</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>",
            IsBodyHtml = true,
        };

        mailMessage.To.Add(email);

        await client.SendMailAsync(mailMessage);
    }
}

public class SubscribeRequest
{
    public string Email { get; set; }
}
