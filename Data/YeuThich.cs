using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class YeuThich
    {
        public int MaYeuThich { get; set; }
        public string? MaSanPham { get; set; }
        public string? TenSanPham { get; set; }
        public string? MaNguoiDung { get; set; }
        public string? HoTen { get; set; }
        public DateTime? NgayYeuThich { get; set; }

    }
}
