namespace UltraStrore.Repository
{
    public interface IEmailServices
    {
        Task SendOtpEmailAsync(string email, string otp);
        Task SendOtpEmailAccountAsync (string email, string account);
    }
}
