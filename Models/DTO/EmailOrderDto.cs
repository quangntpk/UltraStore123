namespace UltraStrore.Models.DTOs
{
    public class EmailOrderDto
    {
        public string Email { get; set; }
        public int OrderId { get; set; }
        public string Name { get; set; }
        public string QrBase64 { get; set; }
    }

}
