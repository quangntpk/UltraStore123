namespace UltraStrore.Models.CreateModels
{
    public class MoTaSanPhamCreateModel
    {
        public string? MaSanPham { get; set; }
        public string? IdMoTa { get; set; }
        public MoTaModel MoTa { get; set; }
    }

    public class MoTaModel
    {
        public HeaderModel Header { get; set; }
        public List<PictureModel> Picture { get; set; }
        public List<TitleModel> Title { get; set; }
    }

    public class HeaderModel
    {
        public string Title { get; set; }
    }

    public class PictureModel
    {
        public string Url { get; set; }
    }

    public class TitleModel
    {
        public string Name { get; set; }
        public List<SubtitleModel> Subtitle { get; set; }
    }

    public class SubtitleModel
    {
        public string Name { get; set; }
        public DescriptionModel Description { get; set; }
    }

    public class DescriptionModel
    {
        public string Content { get; set; }
    }
}
