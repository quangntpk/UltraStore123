using UltraStrore.Data;

namespace UltraStrore.Models.DTO
{
    public class OrderDto
    {
        public string MaNguoiDung { get; set; }
        public string TenNguoiNhan { get; set; }
        public string Sdt { get; set; }
        public string DiaChi { get; set; }
        public DateTime NgayDat { get; set; }
        public TrangThaiDonHang TrangThaiDonHang { get; set; }
        public TrangThaiThanhToan TrangThaiHang { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal FinalAmount { get; set; }

        public List<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}
