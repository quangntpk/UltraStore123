using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface ILienHeServices
    {
        Task<List<LienHeView>> GetLienHeList(string? searchTerm);
        Task<LienHeView> GetLienHeById(int id);
        Task<LienHeView> CreateLienHe(LienHeCreate model);
        Task<LienHeView> UpdateLienHe(LienHeEdit model);
        Task<bool> DeleteLienHe(int id);
    }
}