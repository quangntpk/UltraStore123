namespace UltraStrore.Models.ViewModels
{
    public class TopProductView
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ThuongHieu { get; set; }
        public string ChatLieu { get; set; }
        public string LoaiSanPham { get; set; }
        public int SoLuongDaBan { get; set; }
        public decimal DoanhThu { get; set; }
        public List<StatusBreakdownView> StatusBreakdown { get; set; }
    }

    public class StatusBreakdownView
    {
        public string Status { get; set; }
        public int SoLuong { get; set; }
        public decimal DoanhThu { get; set; }
    }
}