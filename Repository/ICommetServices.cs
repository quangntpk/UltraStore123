using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface ICommetServices
    {
        Task<List<BinhLuanView>> ListBinhLuan();
        Task<BinhLuanView> AddBinhLuan(BinhLuanCreate binhLuan);
        Task<BinhLuanView> UpdateBinhLuan(int maBinhLuan, BinhLuanEdit binhLuan);
        Task<bool> DeleteBinhLuan(int maBinhLuan);
        Task<bool> ApproveBinhLuan(int maBinhLuan);
        Task<bool> UnapproveBinhLuan(int maBinhLuan);
        Task<bool> LikeBinhLuan(int maBinhLuan); // Thêm phương thức Like
        Task<bool> UnlikeBinhLuan(int maBinhLuan); // Thêm phương thức Unlike
    }
}