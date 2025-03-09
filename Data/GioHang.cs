using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class GioHang
    {

        public int MaGioHang { get; set; }
        public string? MaNguoiDung { get; set; }

        public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
    }
}
