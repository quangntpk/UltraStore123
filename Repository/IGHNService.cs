namespace UltraStrore.Repository
{
    public interface IGHNService
    {
        Task<List<UltraStrore.Utils.Province>> GetProvinces();
        Task<List<UltraStrore.Utils.District>> GetDistricts(int provinceId);
        Task<List<UltraStrore.Utils.Ward>> GetWards(int districtId);
        Task<string> CreateShippingOrder(UltraStrore.Utils.ShippingOrder order);
    }
}