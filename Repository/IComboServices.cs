using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface IComboServices
    {
        Task<List<ComboAdminView>> ComboViews();
    }
}
