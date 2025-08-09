using UltraStrore.Models.CreateModels;

namespace UltraStrore.Models.EditModels
{
    public class KhuyenMaiEdit
    {
        public int ID { get; set; }
        public string TenKhuyenMai { get; set; }
        public DateOnly? BatDau { get; set; }
        public DateOnly? KetThuc { get; set; }
        public int? PercentChung { get; set; }
        public List<byte[]>? Pictures { get; set; }
        public bool All { get; set; }
        public List<ChiTietKhuyenMaiCreate>? ChiTiet { get; set; }
    }
}
