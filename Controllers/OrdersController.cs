using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .Select(d => new
                {
                    MaDonHang = d.MaDonHang,
                    TenNguoiNhan = d.TenNguoiNhan,
                    NgayDat = d.NgayDat != null ? d.NgayDat.Value.ToString("dd/MM/yyyy") : "",
                    TrangThaiDonHang = (int)d.TrangThaiDonHang,
                    TrangThaiThanhToan = (int)d.TrangThaiHang,
                    HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "COD" : "VNPay",
                    LyDoHuy = d.LyDoHuy
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var order = await _context.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaSanPhamNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (order == null)
            {
                return NotFound();
            }

            var orderDetails = new
            {
                SanPhams = order.ChiTietDonHangs.Select(cd => new
                {
                    MaChiTietDh = cd.MaCtdh,
                    TenSanPham = cd.MaSanPhamNavigation.TenSanPham,
                    SoLuong = cd.SoLuong,
                    Gia = cd.Gia, // Thêm trường Gia
                    ThanhTien = cd.ThanhTien
                }),
                ThongTinNguoiDung = new
                {
                    TenNguoiNhan = order.TenNguoiNhan,
                    DiaChi = order.DiaChi,
                    Sdt = order.Sdt,
                    TenNguoiDat = order.MaNguoiDungNavigation.HoTen
                },
                ThongTinDonHang = new
                {
                    NgayDat = order.NgayDat != null ? order.NgayDat.Value.ToString("dd/MM/yyyy") : "",
                    TrangThai = (int)order.TrangThaiDonHang,
                    ThanhToan = (int)order.TrangThaiHang,
                    HinhThucThanhToan = order.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "Thanh toán khi nhận hàng" : "Thanh toán VNPay"
                }
            };

            return Ok(orderDetails);
        }

        // PUT: api/orders/approve/{id}
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            var order = await _context.DonHangs.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.TrangThaiDonHang == TrangThaiDonHang.ChuaXacNhan ||
                order.TrangThaiDonHang == TrangThaiDonHang.DangXuLy ||
                order.TrangThaiDonHang == TrangThaiDonHang.DangGiaoHang)
            {
                order.TrangThaiDonHang = (TrangThaiDonHang)((int)order.TrangThaiDonHang + 1);
                if (order.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang)
                {
                    order.TrangThaiHang = TrangThaiThanhToan.ThanhToanVNPay;
                }
            }
            else
            {
                return BadRequest(new { message = "Không thể duyệt đơn hàng ở trạng thái này" });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Duyệt đơn thành công" });
        }

        // PUT: api/orders/cancel/{id}
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] string lyDoHuy)
        {
            var order = await _context.DonHangs.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Đơn hàng không tồn tại" });
            }

            // Sửa logic để cho phép hủy ở trạng thái Chưa xác nhận (0) hoặc Đang xử lý (1)
            if (order.TrangThaiDonHang != TrangThaiDonHang.ChuaXacNhan && order.TrangThaiDonHang != TrangThaiDonHang.DangXuLy)
            {
                return BadRequest(new { message = "Chỉ có thể hủy đơn hàng khi chưa xác nhận hoặc đang xử lý" });
            }

            if (string.IsNullOrEmpty(lyDoHuy))
            {
                return BadRequest(new { message = "Lý do hủy không được để trống" });
            }

            order.TrangThaiDonHang = TrangThaiDonHang.DaHuy;
            order.LyDoHuy = lyDoHuy;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Hủy đơn thành công" });
        }
    }
}