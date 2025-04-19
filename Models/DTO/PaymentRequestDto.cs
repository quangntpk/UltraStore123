namespace UltraStrore.Models.DTO
{
    public class PaymentRequestDto
    {
        public int CartId { get; set; }
        public string? CouponCode { get; set; }
        public string PaymentMethod { get; set; }
        public string TenNguoiNhan { get; set; } 
        public string Sdt { get; set; }      
        public string DiaChi { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal FinalAmount { get; set; }
    }
}
