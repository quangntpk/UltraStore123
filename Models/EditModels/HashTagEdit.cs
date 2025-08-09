using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Models.EditModels
{
    public class HashTagEdit
    {
        [Required]
        public int? MaHashTag { get; set; }
        public string? TenHashTag { get; set; }
        public byte[]? HinhAnh { get; set; }
        public int? TrangThai { get; set; }
    }
}