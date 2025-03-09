using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class ComBoSanPham
    {


        public int MaComBo { get; set; }
        public string? TenComBo { get; set; }
        public string? MoTa { get; set; }
        public string? TenHinhAnh { get; set; }
        public int? SoLuong { get; set; }
        public double? TongGia { get; set; }
        public bool? TrangThai { get; set; }
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
        public virtual ICollection<ChiTietComBo> ChiTietComBos { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
       
    }
}
