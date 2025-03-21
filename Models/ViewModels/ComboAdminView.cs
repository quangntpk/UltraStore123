﻿using UltraStrore.Data;

namespace UltraStrore.Models.ViewModels
{
    public class ComboAdminView
    {
        public int? MaCombo { get ; set; }
        public string? Name { get; set; }
        public byte[]? HinhAnh { get; set; }
        public List<SanPhamViewIncombo>? SanPhams { get; set; }
        public string? MoTa { get; set; }
        public int Gia { get; set; }
        public bool TrangThai { get; set ; }
        public int SoLuong { get; set; }
        public DateOnly? NgayTao { get; set; }
    }
}
