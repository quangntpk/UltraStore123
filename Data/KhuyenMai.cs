namespace UltraStrore.Data
{
    public class KhuyenMai
    {
        public int ID { get; set; }
        public string TenKhuyenMai { get; set; }
        public DateOnly? BatDau { get; set; }
        public DateOnly? KetThuc { get; set; }
        public int? PercentChung { get; set; }
        public bool All { get; set; }
        public bool TrangThai { get; set; }
    }
}
