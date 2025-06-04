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

        public List<TopProductView> GetTopProductsStatistics(int year, int? month = null, int? day = null)
        {
            var statusMap = new Dictionary<int, string>
            {
                { 0, "Chưa xác nhận" },
                { 1, "Đang xử lý" },
                { 2, "Đang giao" },
                { 3, "Hoàn thành" },
                { 4, "Đã hủy" }
            };

            var query = from sp in _context.SanPhams
                        join ctdh in _context.ChiTietDonHangs on sp.MaSanPham equals ctdh.MaSanPham
                        join dh in _context.DonHangs on ctdh.MaDonHang equals dh.MaDonHang
                        join th in _context.ThuongHieus on sp.MaThuongHieu equals th.MaThuongHieu into thuongHieuGroup
                        from th in thuongHieuGroup.DefaultIfEmpty()
                        join lsp in _context.LoaiSanPhams on sp.MaLoaiSanPham equals lsp.MaLoaiSanPham into loaiSanPhamGroup
                        from lsp in loaiSanPhamGroup.DefaultIfEmpty()
                        where dh.NgayDat.HasValue && dh.NgayDat.Value.Year == year
                            && (!month.HasValue || dh.NgayDat.Value.Month == month.Value)
                            && (!day.HasValue || dh.NgayDat.Value.Day == day.Value)
                            && ((int)dh.TrangThaiDonHang == 2 || (int)dh.TrangThaiDonHang == 3)
                        group new { sp, ctdh, dh, th, lsp } by new
                        {
                            sp.TenSanPham,
                            ThuongHieu = th != null ? th.TenThuongHieu : null,
                            sp.ChatLieu,
                            LoaiSanPham = lsp != null ? lsp.TenLoaiSanPham : null
                        } into g
                        select new TopProductView
                        {
                            Id = $"{g.Key.TenSanPham ?? "Unknown"}-{g.Key.ThuongHieu ?? "Unknown"}-{g.Key.ChatLieu ?? "Unknown"}-{g.Key.LoaiSanPham ?? "Unknown"}".GetHashCode().ToString(),
                            Name = g.Key.TenSanPham ?? "Unknown Product",
                            ThuongHieu = g.Key.ThuongHieu ?? "Unknown Brand",
                            ChatLieu = g.Key.ChatLieu ?? "Unknown Material",
                            LoaiSanPham = g.Key.LoaiSanPham ?? "Unknown Category",
                            SoLuongDaBan = g.Sum(x => x.ctdh.SoLuong ?? 0),
                            DoanhThu = g.Sum(x => x.ctdh.ThanhTien ?? 0),
                            StatusBreakdown = g
                                .GroupBy(x => x.dh.TrangThaiDonHang)
                                .Select(sg => new StatusBreakdownView
                                {
                                    Status = statusMap.ContainsKey((int)sg.Key) ? statusMap[(int)sg.Key] : "Không xác định",
                                    SoLuong = sg.Sum(x => x.ctdh.SoLuong ?? 0),
                                    DoanhThu = sg.Sum(x => x.ctdh.ThanhTien ?? 0)
                                })
                                .ToList()
                        };

            return query
                .OrderByDescending(x => x.SoLuongDaBan)
                .Take(10)
                .ToList();
        }

    }
}