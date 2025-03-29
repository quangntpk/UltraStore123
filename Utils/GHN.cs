namespace UltraStrore.Utils
{
    public class GHN
    {
        public string ApiKey { get; set; }
        public string ShopId { get; set; }
    }

    public class Province
    {
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; }
    }

    public class ProvinceResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public List<Province> Data { get; set; }
    }

    public class District
    {
        public int DistrictID { get; set; }
        public string DistrictName { get; set; }
    }

    public class DistrictResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public List<District> Data { get; set; }
    }

    public class Ward
    {
        public string WardCode { get; set; }
        public string WardName { get; set; }
    }

    public class WardResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public List<Ward> Data { get; set; }
    }

    public class ShippingOrder
    {
        public string to_name { get; set; }
        public string to_phone { get; set; }
        public string to_address { get; set; }
        public string to_ward_code { get; set; }
        public int to_district_id { get; set; }
        public int weight { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int service_type_id { get; set; }
        public int service_id { get; set; }
        public int payment_type_id { get; set; }
        public string note { get; set; }
        public int cod_amount { get; set; }
        public List<ShippingOrderItem> items { get; set; }
    }

    public class ShippingOrderItem
    {
        public string name { get; set; }
        public string code { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
        public int weight { get; set; }
    }
}