using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UltraStrore.Data;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class ThongKeServices : IThongKeServices
    {
        private readonly ApplicationDbContext _context;
        private readonly string _loaiSanPhamPath;
        public ThongKeServices(ApplicationDbContext context)
        {
            _context = context;
            _loaiSanPhamPath = Path.Combine(Directory.GetCurrentDirectory(), "DanhMuc", "loaisanpham.json");
        }
        private async Task<List<LoaiSanPham>> LoadLoaiSanPhamAsync()
        {
            if (File.Exists(_loaiSanPhamPath))
            {
                var jsonContent = await File.ReadAllTextAsync(_loaiSanPhamPath);
                if (!string.IsNullOrWhiteSpace(jsonContent))
                {
                    return JsonSerializer.Deserialize<List<LoaiSanPham>>(jsonContent) ?? new List<LoaiSanPham>();
                }
            }
            return new List<LoaiSanPham>();
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
        public List<ThongKeView> GetOrderStatusStatistics(int? year = null, int? month = null, int? day = null)
        {
            var query = _context.DonHangs.AsQueryable();

            // Apply time filters if provided
            if (year.HasValue)
            {
                query = query.Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Year == year.Value);
            }
            if (month.HasValue)
            {
                query = query.Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Month == month.Value);
            }
            if (day.HasValue)
            {
                query = query.Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Day == day.Value);
            }

            var groupedData = query
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
                    5 => "Đã hủy",
                    _ => "Không xác định"
                };
            }

            return groupedData;
        }

        public async Task<List<TopProductView>> GetTopProductsStatistics(int year, int? month = null, int? day = null)
        {
            var loaiSanPhams = await LoadLoaiSanPhamAsync();
            var statusMap = new Dictionary<int, string>
            {
                { 0, "Chưa xác nhận" },
                { 1, "Đang xử lý" },
                { 2, "Đang giao" },
                { 3, "Hoàn thành" },
                { 4, "Đã hủy" }
            };

            // Step 1: Query database WITHOUT using loaiSanPhams
            var dbQuery = from sp in _context.SanPhams
                          join ctdh in _context.ChiTietDonHangs on sp.MaSanPham equals ctdh.MaSanPham
                          join dh in _context.DonHangs on ctdh.MaDonHang equals dh.MaDonHang
                          join th in _context.ThuongHieus on sp.MaThuongHieu equals th.MaThuongHieu into thuongHieuGroup
                          from th in thuongHieuGroup.DefaultIfEmpty()
                          where dh.NgayDat.HasValue && dh.NgayDat.Value.Year == year
                              && (!month.HasValue || dh.NgayDat.Value.Month == month.Value)
                              && (!day.HasValue || dh.NgayDat.Value.Day == day.Value)
                              && ((int)dh.TrangThaiDonHang == 2 || (int)dh.TrangThaiDonHang == 3)
                          select new
                          {
                              TenSanPham = sp.TenSanPham,
                              ThuongHieu = th != null ? th.TenThuongHieu : null,
                              ChatLieu = sp.ChatLieu,
                              MaLoaiSanPham = sp.MaLoaiSanPham, // Lấy MaLoaiSanPham để lookup sau
                              SoLuong = ctdh.SoLuong ?? 0,
                              ThanhTien = ctdh.ThanhTien ?? 0,
                              TrangThaiDonHang = dh.TrangThaiDonHang
                          };

            // Step 2: Execute database query first
            var rawData = await dbQuery.ToListAsync();

            // Step 3: Process in memory with loaiSanPhams
            var groupedData = rawData
                .GroupBy(x => new
                {
                    x.TenSanPham,
                    x.ThuongHieu,
                    x.ChatLieu,
                    x.MaLoaiSanPham
                })
                .Select(g => new TopProductView
                {
                    Id = $"{g.Key.TenSanPham ?? "Unknown"}-{g.Key.ThuongHieu ?? "Unknown"}-{g.Key.ChatLieu ?? "Unknown"}-{g.Key.MaLoaiSanPham?.ToString() ?? "Unknown"}".GetHashCode().ToString(),
                    Name = g.Key.TenSanPham ?? "Unknown Product",
                    ThuongHieu = g.Key.ThuongHieu ?? "Unknown Brand",
                    ChatLieu = g.Key.ChatLieu ?? "Unknown Material",
                    // Lookup LoaiSanPham từ in-memory collection
                    LoaiSanPham = loaiSanPhams.FirstOrDefault(lsp => lsp.MaLoaiSanPham == g.Key.MaLoaiSanPham)?.TenLoaiSanPham ?? "Unknown Category",
                    SoLuongDaBan = g.Sum(x => x.SoLuong),
                    DoanhThu = g.Sum(x => x.ThanhTien),
                    StatusBreakdown = g
                        .GroupBy(x => x.TrangThaiDonHang)
                        .Select(sg => new StatusBreakdownView
                        {
                            Status = statusMap.ContainsKey((int)sg.Key) ? statusMap[(int)sg.Key] : "Không xác định",
                            SoLuong = sg.Sum(x => x.SoLuong),
                            DoanhThu = sg.Sum(x => x.ThanhTien)
                        })
                        .ToList()
                })
                .OrderByDescending(x => x.SoLuongDaBan)
                .Take(10)
                .ToList();

            return groupedData;
        }

    }
}