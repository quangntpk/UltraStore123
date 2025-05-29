using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UltraStrore.Data
{
    public class TinNhan
    {
        [Key]
        public int? MaTinNhan { get; set; }

        [Required]
        public string? NguoiGuiId { get; set; }

        [Required]
        public string? NguoiNhanId { get; set; }

        public string? NoiDung { get; set; } // Dùng cho text hoặc emoji

        public string? KieuTinNhan { get; set; } // "text", "image", "file", "emoji"

        public string? TepDinhKemUrl { get; set; } // URL ảnh hoặc file nếu có

        public DateTime? NgayTao { get; set; } = DateTime.Now;

        public string? TrangThai { get; set; } = "sent";
        public NguoiDung NguoiGui { get; set; }
        public NguoiDung NguoiNhan { get; set; }
    }
}
