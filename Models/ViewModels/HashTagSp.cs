using UltraStrore.Data;

namespace UltraStrore.Models.ViewModels
{
    public class HashTagSp
    {
        public string IDSanPham { get; set; }
        public List<DetailHashTagSP>? ListHashTag { get; set; }
    }
    public class DetailHashTagSP
    {
        public int? ID { get; set; }
        public string? Name { get; set; }
    }
}
