namespace UltraStrore.Models.CreateModels
{

    public class LienHeCreateRequest
    {
        public int MaLienHe { get; set; }
        public string HoTen { get; set; }
        public string Sdt { get; set; }
        public string NoiDung { get; set; }
        public string Email { get; set; }
        public string TrangThai { get; set; }
        public string ReCaptchaToken { get; set; }
    }
}
