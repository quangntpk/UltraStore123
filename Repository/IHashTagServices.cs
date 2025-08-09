using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UltraStrore.Repository
{
    public interface IHashTagServices
    {
        Task<List<HashTagView>> GetAllHashTagAsync();
        Task<HashTagView> GetHashTagAsync(int maHashTag);
        Task<HashTagView> CreateHashTagAsync(HashTagCreate model);
        Task<HashTagView> UpdateHashTagAsync(HashTagEdit model);
        Task<bool> DeleteHashTagAsync(int maHashTag);
        Task<List<SanPhamView>> GetSanPhamByHashTagAsync(int maHashTag);
        Task<List<HashTagView>> SearchHashTagAsync(string tenHashTag);
    }
}