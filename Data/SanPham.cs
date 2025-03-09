using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class SanPham
    {            
        public string MaSanPham { get; set; } = null!;
        public string? TenSanPham { get; set; }
        public int? SoLuong { get; set; }
        public int? Gia { get; set; }
        public int? MaThuongHieu { get; set; }
        public int? MaLoaiSanPham { get; set; }
        public string? KichThuoc { get; set; }
        public DateOnly? NgayTao { get; set; }
        public int? TrangThai { get; set; }
        public bool? Example { get; set; }
        public string? ChatLieu { get; set; }
        public string? MoTa { get; set; }
        public virtual LoaiSanPham? MaLoaiSanPhamNavigation { get; set; }
        public virtual ThuongHieu? MaThuongHieuNavigation { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
        public virtual ICollection<Video> Videos { get; set; } 
        public virtual ICollection<YeuThich> YeuThiches { get; set; } 
        public virtual ICollection<BinhLuan> MaBinhLuans { get; set; }
    }
}
