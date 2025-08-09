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
    public class ItemInstant
    {
        public string? HinhAnh { get; set; }
        public int SoLuong { get; set; }
        public string IdSanPham { get; set; } // Change to string if it’s a code like "A00002"
        public string KickThuoc { get; set; }
        public string MauSac { get; set; }
        public int SoLuongMua { get; set; }
        public string TenSanPham { get; set; } // Change to string if it’s a name like "Áo khoác nữ"
        public int TienSanPham { get; set; }
    }
    public class PaymentRequestDto1
    {
        public int? CartId { get; set; } // Make nullable if optional
        public string? CouponCode { get; set; }
        public string PaymentMethod { get; set; }
        public string TenNguoiNhan { get; set; }
        public string Sdt { get; set; }
        public string DiaChi { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal FinalAmount { get; set; }
        public string UserId { get; set; }
        public List<ItemInstant> items { get; set; }
    }
}
