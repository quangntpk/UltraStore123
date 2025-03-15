
using UltraStrore.Models.CreateModels;
using UltraStrore.Data;
using UltraStrore.Helper;

using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface INguoiDungServices
    {
        Task<List<NguoiDungView>> GetNguoiDungList(string? searchTerm);
        Task<NguoiDungView> GetNguoiDungById(string id);
        Task<NguoiDungView> CreateNguoiDung(NguoiDungCreate model);
        Task<NguoiDungView> UpdateNguoiDung(NguoiDungEdit model);
        Task<bool> DeleteNguoiDung(string id);
        Task<NguoiDungView> DangKy(DangKyView model);
        Task<(NguoiDungView User, string Token)> DangNhap(DangNhapView model);
        Task<NguoiDung> GetNguoiDungByEmailAsync(string email);
        Task<bool> GenerateAndSendOtpAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string otp);
        Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);
    }
}