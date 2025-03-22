using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;


namespace UltraStrore.Repository
{
    public interface IYeuThichServices
    {
        List<YeuThichView> GetAllYeuThichs();
        Task<YeuThichView> CreateYeuThich(YeuThichCreate yeuThichCreate);
        Task<bool> DeleteYeuThich(int maYeuThich);
    }
}
