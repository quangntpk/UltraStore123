using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;

    public EmailService(IConfiguration configuration)
    {
        _smtpServer = configuration["Smtp:Server"] ?? throw new ArgumentNullException("Smtp:Server is not configured.");
        _smtpPort = int.TryParse(configuration["Smtp:Port"], out int port) ? port : throw new ArgumentNullException("Smtp:Port is not configured or invalid.");
        _smtpUser = configuration["Smtp:Username"] ?? throw new ArgumentNullException("Smtp:Username is not configured.");
        _smtpPass = configuration["Smtp:Password"] ?? throw new ArgumentNullException("Smtp:Password is not configured.");
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Email người nhận không được để trống.", nameof(toEmail));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Tiêu đề email không được để trống.", nameof(subject));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Nội dung email không được để trống.", nameof(body));

        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUser),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
        catch (SmtpException ex)
        {
            throw new Exception($"Lỗi SMTP khi gửi email: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi không xác định khi gửi email: {ex.Message}", ex);
        }
    }
}