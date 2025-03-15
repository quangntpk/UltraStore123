using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStoreApi.ViewModels;

namespace UltraStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DonHang
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var donHangs = await _context.DonHangs
                .Include(dh => dh.MaNguoiDungNavigation)
                .Include(dh => dh.ChiTietDonHangs)
                .ThenInclude(ct => ct.SanPham)
                .Select(dh => new DonHangView
                {
                    MaDonHang = dh.MaDonHang,
                    MaNguoiDung = dh.MaNguoiDung,
                    MaNhanVien = dh.MaNhanVien,
                    NgayDat = dh.NgayDat,
                    TrangThaiDonHang = dh.TrangThaiDonHang,
                    TrangThaiHang = dh.TrangThaiHang,
                    LyDoHuy = dh.LyDoHuy,
                    TenNguoiNhan = dh.TenNguoiNhan,
                    Sdt = dh.Sdt,
                    DiaChi = dh.DiaChi,
                    TenNguoiDung = dh.MaNguoiDungNavigation != null ? dh.MaNguoiDungNavigation.HoTen : null,
                    ChiTietDonHangs = dh.ChiTietDonHangs.Select(ct => new ChiTietDonHangView
                    {
                        MaCtdh = ct.MaCtdh,
                        MaDonHang = ct.MaDonHang,
                        MaSanPham = ct.MaSanPham,
                        TenSanPham = ct.SanPham != null ? ct.SanPham.TenSanPham : null,
                        SoLuong = ct.SoLuong,
                        Gia = ct.Gia,
                        ThanhTien = ct.ThanhTien,
                        MaCombo = ct.MaCombo
                    }).ToList()
                })
                .ToListAsync();

            return Ok(donHangs);
        }

        // GET: api/DonHang/{maDonHang}
        [HttpGet("{maDonHang}")]
        public async Task<IActionResult> GetById(int maDonHang)
        {
            var donHang = await _context.DonHangs
                .Include(dh => dh.MaNguoiDungNavigation)
                .Include(dh => dh.ChiTietDonHangs)
                .ThenInclude(ct => ct.SanPham)
                .Select(dh => new DonHangView
                {
                    MaDonHang = dh.MaDonHang,
                    MaNguoiDung = dh.MaNguoiDung,
                    MaNhanVien = dh.MaNhanVien,
                    NgayDat = dh.NgayDat,
                    TrangThaiDonHang = dh.TrangThaiDonHang,
                    TrangThaiHang = dh.TrangThaiHang,
                    LyDoHuy = dh.LyDoHuy,
                    TenNguoiNhan = dh.TenNguoiNhan,
                    Sdt = dh.Sdt,
                    DiaChi = dh.DiaChi,
                    TenNguoiDung = dh.MaNguoiDungNavigation != null ? dh.MaNguoiDungNavigation.HoTen : null,
                    ChiTietDonHangs = dh.ChiTietDonHangs.Select(ct => new ChiTietDonHangView
                    {
                        MaCtdh = ct.MaCtdh,
                        MaDonHang = ct.MaDonHang,
                        MaSanPham = ct.MaSanPham,
                        TenSanPham = ct.SanPham != null ? ct.SanPham.TenSanPham : null,
                        SoLuong = ct.SoLuong,
                        Gia = ct.Gia,
                        ThanhTien = ct.ThanhTien,
                        MaCombo = ct.MaCombo
                    }).ToList()
                })
                .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);

            if (donHang == null) return NotFound("Đơn hàng không tồn tại");
            return Ok(donHang);
        }

        // PUT: api/DonHang/duyet/{maDonHang}
        [HttpPut("duyet/{maDonHang}")]
        public async Task<IActionResult> DuyetDonHang(int maDonHang)
        {
            try
            {
                var donHang = await _context.DonHangs.FindAsync(maDonHang);
                if (donHang == null) return NotFound("Đơn hàng không tồn tại");
                if (donHang.TrangThaiDonHang != "Đang xử lý") return BadRequest("Chỉ có thể duyệt khi đơn hàng đang xử lý!");

                donHang.TrangThaiDonHang = "Đang giao";
                await _context.SaveChangesAsync();
                return Ok(new { message = "Duyệt đơn hàng thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/DonHang/huy/{maDonHang}
        [HttpPut("huy/{maDonHang}")]
        public async Task<IActionResult> HuyDonHang(int maDonHang, [FromBody] string lyDoHuy)
        {
            try
            {
                var donHang = await _context.DonHangs.FindAsync(maDonHang);
                if (donHang == null) return NotFound("Đơn hàng không tồn tại");
                if (donHang.TrangThaiDonHang != "Đang xử lý") return BadRequest("Chỉ có thể hủy khi đơn hàng đang xử lý!");

                donHang.TrangThaiDonHang = "Đã hủy";
                donHang.LyDoHuy = lyDoHuy;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Hủy đơn hàng thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}