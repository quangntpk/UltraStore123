using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class HinhAnh
    {
        public int MaHinhAnh { get; set; }
        public string? TenHinhAnh { get; set; }
        public string? Link { get; set; }
        public string? MaSanPham { get; set; }
        public int? MaBinhLuan { get; set; }
        public byte[]? Data { get; set; }
        public virtual BinhLuan? MaBinhLuanNavigation { get; set; }
        public virtual SanPham? MaSanPhamNavigation { get; set; }
    }
}
