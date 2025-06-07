using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;


namespace UltraStrore.Repository
{
    public interface IThongKeServices
    {
        List<ThongKeView> GetDailyStatistics(int year, int month, int day);
        List<ThongKeView> GetMonthlyStatistics(int year, int month);
        List<ThongKeView> GetYearlyStatistics(int year);
        List<ThongKeView> GetOrderStatusStatistics(int? year = null, int? month = null, int? day = null);
        List<TopProductView> GetTopProductsStatistics(int year, int? month = null, int? day = null);
    }
}