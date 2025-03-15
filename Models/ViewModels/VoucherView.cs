namespace UltraStrore.Models.ViewModels
{
    public class VoucherView
    {
        public int? MaVoucher { get; set; } = null!;
        public string? TenVoucher { get; set; }
        public int? GiaTri { get; set; }
        public string? MoTa { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string? HinhAnh { get; set; }
        public decimal? DieuKien { get; set; }
        public int? SoLuong { get; set; }

        public int? TrangThai { get; set; }
        public List<CouponView>? Coupons { get; set; }
    }
    public class CouponView
    {
        public int? ID { get; set; }
        public string? MaNhap { get; set; }
        public int? TrangThai { get; set; }

        public int? MaVoucher { get; set; }
    }
}
