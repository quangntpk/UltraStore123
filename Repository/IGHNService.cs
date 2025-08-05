using UltraStrore.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UltraStrore.Repository
{
    public interface IGHNService
    {
        Task<List<UltraStrore.Utils.Province>> GetProvinces();
        Task<List<UltraStrore.Utils.District>> GetDistricts(int provinceId);
        Task<List<UltraStrore.Utils.Ward>> GetWards(int districtId);
        Task<string> CreateShippingOrder(ShippingOrder order);
        Task<List<UltraStrore.Utils.Shop>> GetShops(UltraStrore.Utils.ShopRequest request);
        Task<UltraStrore.Utils.LeadTimeResponseData> GetLeadTime(UltraStrore.Utils.LeadTimeRequest request);
        Task<ShippingOrderFee> GetShippingFee(ShippingFeeRequest request);
    }
}