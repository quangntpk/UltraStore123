using System.Text.Json.Serialization;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Models.CreateModels
{
    public class KhuyenMaiCreate
    {
        public string TenKhuyenMai { get; set; }
        public DateOnly? BatDau { get; set; }
        public DateOnly? KetThuc { get; set; }
        public int? PercentChung { get; set; }
        public List<byte[]>? Pictures { get; set; }
        public bool All { get; set; }
        public List<ChiTietKhuyenMaiCreate>? ChiTiet { get; set; }
    }
    public class ChiTietKhuyenMaiCreate
    {
        public string? IdSanPham { get; set; }
        public int? IdCombo { get; set; }
        public string? TenSanPhamCombo { get; set; }
        public float? GiaMoi { get; set; }
        public int? Percent { get; set; }
        public int? GiaGoc { get; set; }
    }
}
