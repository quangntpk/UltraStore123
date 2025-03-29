using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Data
{
    public partial class DanhSachDiaChi
    {
        public int MaDiaChi { get; set; }
        public string? MaNguoiDung { get; set; }
        public string? HoTen { get; set; }
        public string? Sdt { get; set; }
        public string? MoTa { get; set; }
        public string? DiaChi { get; set; }
        public string? PhuongXa { get; set; }
        public string? QuanHuyen { get; set; }
        public string? Tinh { get; set; }
        public int? TrangThai { get; set; }

        public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
    }
}
