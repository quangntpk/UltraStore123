using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Data.Temp
{
    public class TinNhan
    {
        [Key]
        public int MaTinNhan { get; set; }
        public string NguoiGuiId { get; set; }
        public string NguoiNhanId { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; }

    }
}
