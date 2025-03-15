namespace UltraStrore.Models.CreateModels
{
    public class VoucherCreate
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
    }
}
