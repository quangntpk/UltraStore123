using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface IJwtTokenServices
    {
        string GenerateToken(NguoiDungView user);
    }
}