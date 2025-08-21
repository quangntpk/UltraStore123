using UltraStrore.Models.DTO;

namespace UltraStrore.Data.Temp
{
    public class PendingVnPayOrder
    {
        public string? TempOrderId { get; set; }
        public DonHang? Order { get; set; }
        public decimal? OriginalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public string? CouponCode { get; set; }
        public int? CartId { get; set; }

        public List<ChiTietGioHangDto>? ChiTietGioHangs { get; set; }
        public List<DonHangSupportDto>? DonHangSupports { get; set; }
        public List<ChiTietDonHangDto>? ChiTietDonHangs { get; set; }
    }
}
