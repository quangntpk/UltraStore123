namespace UltraStrore.Models.ViewModels
{
    public class SelectDateProductView
    {
        public DateOnly? BatDau { get; set; }
        public DateOnly? KetThuc { get; set; }
        public List<string> ID { get; set; }
    }
}
