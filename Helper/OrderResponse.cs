using UltraStrore.Data;

namespace UltraStrore.Helper
{
    public class OrderResponse
    {
        public int MaDonHang { get; set; }
        public string? NgayDat { get; set; }
        public TrangThaiDonHang TrangThaiDonHang { get; set; }
        public decimal TongTien { get; set; }
        public string? TenNguoiNhan { get; set; }
        public string? Sdt { get; set; }
        public string? HinhThucThanhToan { get; set; }
        public string? LyDoHuy { get; set; }
        public List<OrderItemResponse> SanPhams { get; set; } = new List<OrderItemResponse>();
    }
}
