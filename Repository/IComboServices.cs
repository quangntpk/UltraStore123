using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface IComboServices
    {
        Task<List<ComboAdminView>> ComboViews(int? id);
        Task<APIResponse> AddCombo(ComboCreate? info);
        Task<APIResponse> EditCombo(ComboEdit? info);
    }
}
