using UltraStrore.Models.CreateModels;

namespace UltraStrore.Models.EditModels
{
    public class ComboEdit
    {
        public int ID { get; set; }
        public string? TenCombo { get; set; }
        public string? MoTa { get; set; }
        public int? SoLuong { get; set; }
        public int? Gia { get; set; }
        public byte[]? HinhAnh { get; set; }
        public List<ComboCreateDetail> SanPham { get; set; }
    }
}
