using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace UltraStrore.Repository
{
    public interface IYeuThichServices
    {
        List<YeuThichView> GetAllYeuThichs();
        Task<YeuThichView> CreateYeuThich(YeuThichCreate yeuThichCreate);
        Task<bool> DeleteYeuThich(int maYeuThich);
    }
}
