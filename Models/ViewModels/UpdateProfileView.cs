namespace UltraStrore.Models.ViewModels
{
    public class UpdateProfileView
    {
        public string? HoTen { get; set; }
        public string? TaiKhoan { get; set; }
        public int? GioiTinh { get; set; }
        public string? Email { get; set; }
        public string? Sdt { get; set; }
        public string? DiaChi { get; set; }
        public string? CCCD { get; set; }
        public DateTime? NgaySinh { get; set; }
        public IFormFile? HinhAnh { get; set; }
        public string? MatKhauCu { get; set; }
        public string? MatKhauMoi { get; set; }

    }
}
