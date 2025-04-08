using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class ThongKeServices : IThongKeServices
    {
        private readonly ApplicationDbContext _context;

        public ThongKeServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ThongKeView> GetDailyStatistics(int year, int month, int day)
        {
            return _context.DonHangs
                .Where(d => d.NgayDat.HasValue &&
                            d.NgayDat.Value.Year == year &&
                            d.NgayDat.Value.Month == month &&
                            d.NgayDat.Value.Day == day)
                .GroupBy(d => new { d.NgayDat.Value.Year, d.NgayDat.Value.Month, d.NgayDat.Value.Day })
                .Select(g => new ThongKeView
                {
                    Ngay = g.Key.Day,
                    Thang = g.Key.Month,
                    Nam = g.Key.Year,
                    TongDoanhThu = g.Sum(d => d.ChiTietDonHangs.Sum(ct => ct.ThanhTien ?? 0)), // Sử dụng ThanhTien thay vì DonGia
                    TongDonHang = g.Count()
                })
                .ToList();
        }

        public List<ThongKeView> GetMonthlyStatistics(int year, int month)
        {
            return _context.DonHangs
                .Where(d => d.NgayDat.HasValue &&
                            d.NgayDat.Value.Year == year &&
                            d.NgayDat.Value.Month == month)
                .GroupBy(d => new { d.NgayDat.Value.Year, d.NgayDat.Value.Month, d.NgayDat.Value.Day })
                .Select(g => new ThongKeView
                {
                    Ngay = g.Key.Day,
                    Thang = g.Key.Month,
                    Nam = g.Key.Year,
                    TongDoanhThu = g.Sum(d => d.ChiTietDonHangs.Sum(ct => ct.ThanhTien ?? 0)), // Sử dụng ThanhTien thay vì DonGia
                    TongDonHang = g.Count()
                })
                .ToList();
        }

        public List<ThongKeView> GetYearlyStatistics(int year)
        {
            return _context.DonHangs
                .Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Year == year)
                .GroupBy(d => new { d.NgayDat.Value.Year, d.NgayDat.Value.Month })
                .Select(g => new ThongKeView
                {
                    Ngay = 15, // Giả định ngày đại diện là 15
                    Thang = g.Key.Month,
                    Nam = g.Key.Year,
                    TongDoanhThu = g.Sum(d => d.ChiTietDonHangs.Sum(ct => ct.ThanhTien ?? 0)), // Sử dụng ThanhTien thay vì DonGia
                    TongDonHang = g.Count()
                })
                .ToList();
        }
        public List<ThongKeView> GetOrderStatusStatistics()
        {
            var groupedData = _context.DonHangs
                .GroupBy(d => d.TrangThaiDonHang)
                .Select(g => new ThongKeView
                {
                    Ngay = null,
                    Thang = null,
                    Nam = null,
                    TongDoanhThu = 0,
                    TongDonHang = g.Count(),
                    TrangThai = (int)g.Key,
                    TenTrangThai = "" // Gán tạm thời
                })
                .ToList();

            // Ánh xạ TenTrangThai sau khi lấy dữ liệu
            foreach (var item in groupedData)
            {
                item.TenTrangThai = item.TrangThai switch
                {
                    0 => "Chưa xác nhận",
                    1 => "Đang xử lý",
                    2 => "Đang giao",
                    3 => "Hoàn thành",
                    4 => "Đã hủy",
                    _ => "Không xác định"
                };
            }

            return groupedData;
        }

    }
}