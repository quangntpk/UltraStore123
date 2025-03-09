using System;
using System.Collections.Generic;

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
        public bool? TrangThai { get; set; }

        public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
    }
}
