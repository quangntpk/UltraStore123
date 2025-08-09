namespace UltraStrore.Data
{
    public class Coupon
    {
        public int? ID { get; set; }
        public string? MaNhap { get; set; }
        public int? TrangThai { get; set; }
        public string? MaNguoiDung { get; set; }
        public int? MaVoucher { get; set; } 
        public virtual Voucher? MaVoucherNavigation { get; set; } 
    }
}
