using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Utils
{
    public class ShippingFeeResponse
    {
        public int? code { get; set; }
        public string? message { get; set; }
        public ShippingOrderFee? data { get; set; }
    }

    public class ShippingFeeRequest
    {
        public int? service_type_id { get; set; }
        public int? from_district_id { get; set; }
        public string? from_ward_code { get; set; }
        public int to_district_id { get; set; }
        public string to_ward_code { get; set; }
        public int weight { get; set; }
        public int? length { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public int? insurance_value { get; set; }
        public string? coupon { get; set; }
        public int? cod_failed_amount { get; set; }
        public List<ShippingOrderItem>? items { get; set; }
    }
    public class ShippingOrder
    {
        public int? payment_type_id { get; set; }
        public string? note { get; set; }
        public string? required_note { get; set; }
        public string? return_phone { get; set; }
        public string? return_address { get; set; }
        public int? return_district_id { get; set; }
        public string? return_ward_code { get; set; }
        public string? client_order_code { get; set; }
        public string? from_name { get; set; }
        public string? from_phone { get; set; }
        public string? from_address { get; set; }
        public string? from_ward_name { get; set; }
        public string? from_district_name { get; set; }
        public string? from_province_name { get; set; }
        public string? to_name { get; set; }
        public string? to_phone { get; set; }
        public string? to_address { get; set; }
        public string? to_ward_code { get; set; }
        public int? to_district_id { get; set; }
        public string? to_ward_name { get; set; }
        public string? to_district_name { get; set; }
        public string? to_province_name { get; set; }
        public int? cod_amount { get; set; }
        public string? content { get; set; }
        public int weight { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int cod_failed_amount { get; set; }
        public int? pick_station_id { get; set; }
        public int? deliver_station_id { get; set; }
        public int? insurance_value { get; set; }
        public int? service_type_id { get; set; }
        public string? coupon { get; set; }
        public long? pickup_time { get; set; }
        public List<int>? pick_shift { get; set; }
        public List<ShippingOrderItem>? items { get; set; }
    }
    public class ShippingOrderItemCategory
    {
        public string? level1 { get; set; }
        public string? level2 { get; set; }
        public string? level3 { get; set; }
    }

    public class ShippingOrderItem
    {
        public string? name { get; set; }
        public string? code { get; set; }
        public int? quantity { get; set; }
        public int? price { get; set; }
        public int? weight { get; set; }
        public int? length { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public ShippingOrderItemCategory? category { get; set; }
    }

    public class ShippingOrderFee
    {
        public int? total { get; set; }
        public int? main_service { get; set; }
        public int? insurance { get; set; }
        public int? cod_fee { get; set; }
        public int? station_do { get; set; }
        public int? station_pu { get; set; }
        public int? return_fee { get; set; }
        public int? r2s { get; set; }
        public int? return_again { get; set; }
        public int? coupon { get; set; }
        public int? document_return { get; set; }
        public int? double_check { get; set; }
        public int? double_check_deliver { get; set; }
        public int? pick_remote_areas_fee { get; set; }
        public int? deliver_remote_areas_fee { get; set; }
        public int? pick_remote_areas_fee_return { get; set; }
        public int? deliver_remote_areas_fee_return { get; set; }
        public int? cod_failed_fee { get; set; }
    }

    public class ShippingOrderResponseData
    {
        public string? order_code { get; set; }
        public string? sort_code { get; set; }
        public string? trans_type { get; set; }
        public string? ward_encode { get; set; }
        public string? district_encode { get; set; }
        public ShippingOrderFee fee { get; set; }
        public int? total_fee { get; set; }
        public string? expected_delivery_time { get; set; }
        public string? operation_partner { get; set; }
    }

    public class ShippingOrderResponse
    {
        public int? code { get; set; }
        public string? message { get; set; }
        public string? code_message_value { get; set; }
        public string? message_display { get; set; }
        public ShippingOrderResponseData data { get; set; }
    }

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

    public class Shop
    {
        public int _id { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string ward_code { get; set; }
        public int district_id { get; set; }
        public int client_id { get; set; }
        public int bank_account_id { get; set; }
        public int status { get; set; }
        public object location { get; set; }
        public string version_no { get; set; }
        public bool is_created_chat_channel { get; set; }
        public string address_v2 { get; set; }
        public int ward_id_v2 { get; set; }
        public int province_id_v2 { get; set; }
        public bool is_new_address { get; set; }
        public string updated_ip { get; set; }
        public int updated_employee { get; set; }
        public int updated_client { get; set; }
        public string updated_source { get; set; }
        public DateTime updated_date { get; set; }
        public string created_ip { get; set; }
        public int created_employee { get; set; }
        public int created_client { get; set; }
        public string created_source { get; set; }
        public DateTime created_date { get; set; }
    }

    public class ShopResponseData
    {
        public int last_offset { get; set; }
        public List<Shop> shops { get; set; }
    }

    public class ShopResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public ShopResponseData data { get; set; }
    }

    public class ShopRequest
    {
        public int offset { get; set; }
        public int limit { get; set; }
        public string client_phone { get; set; }
    }

    public class LeadTimeRequest
    {
        public int from_district_id { get; set; }
        public string from_ward_code { get; set; }
        public int to_district_id { get; set; }
        public string to_ward_code { get; set; }
        public int service_id { get; set; }
    }

    public class LeadTimeOrder
    {
        public string from_estimate_date { get; set; }
        public string to_estimate_date { get; set; }
    }

    public class LeadTimeResponseData
    {
        public long leadtime { get; set; }
        public LeadTimeOrder leadtime_order { get; set; }
    }

    public class LeadTimeResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public LeadTimeResponseData data { get; set; }
    }

}