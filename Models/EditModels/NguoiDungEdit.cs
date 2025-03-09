namespace UltraStrore.Models.EditModels
{
    public class NguoiDungEdit
    {
        public string? MaNguoiDung { get; set; }
        public string? HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }

        public string? Sdt { get; set; }
        public string? Cccd { get; set; }
        public string? Email { get; set; }
        public string? TaiKhoan { get; set; }
        public string? DiaChi { get; set; }
        public int? TrangThai { get; set; }

        public int? VaiTro { get; set; }
        public int? CancelConunt { get; set; }
        public DateTime? LockoutEndDate { get; set; }

        public byte[]? HinhAnh { get; set; }
        public DateTime? NgayTao { get; set; }
        public string? MoTa { get; set; }

    }
}
