using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class DonHang
    {
        public int MaDonHang { get; set; }
        public string? MaNguoiDung { get; set; }
        public string? MaNhanVien { get; set; }
        public DateTime? NgayDat { get; set; }
        public TrangThaiDonHang TrangThaiDonHang { get; set; }
        public TrangThaiThanhToan TrangThaiHang { get; set; }
        public string? LyDoHuy { get; set; }
        public string? TenNguoiNhan { get; set; }
        public string? Sdt { get; set; }
        public string? DiaChi { get; set; }

        public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
        public virtual List<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>(); // Thêm thuộc tính
    }
}