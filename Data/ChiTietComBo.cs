using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class ChiTietComBo
    {
        public int MaChiTietComBo { get; set; }
        public int? MaComBo { get; set; }
        public string? MaSanPham { get; set; }
        public int? SoLuong { get; set; }

        public virtual ComBoSanPham? MaComBoNavigation { get; set; }
    }
}
