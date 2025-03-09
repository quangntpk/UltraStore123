using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class Video
    {
        public int MaVideo { get; set; }
        public string? TenVideo { get; set; }
        public string? MaSanPham { get; set; }

        public virtual SanPham? MaSanPhamNavigation { get; set; }
    }
}
