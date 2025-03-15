using UltraStrore.Data;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface ICommetServices
    {
        Task<List<BinhLuanView>> ListBinhLuan();
        Task<BinhLuan> AddBinhLuan(BinhLuan binhLuan);
        Task<BinhLuan> UpdateBinhLuan(int maBinhLuan, BinhLuan binhLuan);
        Task<bool> DeleteBinhLuan(int maBinhLuan);
        Task<bool> ApproveBinhLuan(int maBinhLuan);
        Task<bool> UnapproveBinhLuan(int maBinhLuan);
    }
}