namespace UltraStrore.Models.CreateModels
{
    public class ComboCreate
    {
        public string? TenCombo { get; set; }
        public string? MoTa { get; set; }
        public int? SoLuong { get; set; }
        public int? Gia { get; set; }
        public byte[]? HinhAnh { get; set; }
        public List<ComboCreateDetail>? SanPham { get; set; }

    }
}
