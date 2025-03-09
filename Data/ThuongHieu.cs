using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class ThuongHieu
    {

        public int MaThuongHieu { get; set; }
        public string? TenThuongHieu { get; set; }

        public virtual ICollection<SanPham> SanPhams { get; set; }
    }
}
