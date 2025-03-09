using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class LoaiSanPham
    {

        public int MaLoaiSanPham { get; set; }
        public string? TenLoaiSanPham { get; set; }
        public string? KiHieu { get; set; }

        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}
