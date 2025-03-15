namespace UltraStrore.Repository
{
    public interface IEmailServices
    {
        Task SendOtpEmailAsync(string email, string otp);
    }
}
