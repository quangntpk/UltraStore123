using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class ChiTietGioHang
    {
        public int MaCtgh { get; set; }
        public int? MaGioHang { get; set; }
        public string? MaSanPham { get; set; }
        public int? SoLuong { get; set; }
        public int? MaCombo { get; set; }
        public int? Gia { get; set; }
        public int? ThanhTien { get; set; }

        public virtual ComBoSanPham? MaComboNavigation { get; set; }
        public virtual GioHang? MaGioHangNavigation { get; set; }
        public virtual SanPham? MaSanPhamNavigation { get; set; }
    }
}
