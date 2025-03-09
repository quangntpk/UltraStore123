using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class YeuThich
    {
        public int MaYeuThich { get; set; }
        public string? MaSanPham { get; set; }
        public string? MaNguoiDung { get; set; }
        public int? SoLuongYeuThich { get; set; }
        public DateTime? NgayYeuThich { get; set; }

        public virtual SanPham? MaSanPhamNavigation { get; set; }
        public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    }
}
