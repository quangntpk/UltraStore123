using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Data
{
    public partial class LoaiSanPham
    {
        [Key]
        public int MaLoaiSanPham { get; set; }
        public string? TenLoaiSanPham { get; set; }
        public string? KiHieu { get; set; }
        public List<string>? KichThuoc { get; set; }
        public byte[]? HinhAnh { get; set; }
        public int? TrangThai { get; set; }
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}