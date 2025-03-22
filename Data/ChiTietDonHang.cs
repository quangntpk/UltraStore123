using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class ChiTietDonHang
    {
        public int MaCtdh { get; set; }
        public int? MaDonHang { get; set; }
        public string? MaSanPham { get; set; }
        public int? SoLuong { get; set; }
        public int? Gia { get; set; }
        public int? ThanhTien { get; set; }
        public int? MaCombo { get; set; }
        public string? SanPhamMaSanPham { get; set; } // Ánh xạ cột SanPhamMaSanPham

        public virtual ComBoSanPham? MaComboNavigation { get; set; }
        public virtual DonHang? MaDonHangNavigation { get; set; } // Thêm quan hệ với DonHang
        public virtual SanPham? MaSanPhamNavigation { get; set; } // Thêm quan hệ với SanPham
    }
}