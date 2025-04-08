namespace UltraStrore.Models.ViewModels
{
    public class ThongKeView
    {
     
        public int? Ngay { get; set; }        
        public int? Thang { get; set; }      
        public int? Nam { get; set; }            
        public decimal TongDoanhThu { get; set; } 
        public int TongDonHang { get; set; }    

    
        public int? TrangThai { get; set; }     
        public string? TenTrangThai { get; set; }
    }
}