using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartServices services;

        public CartController(ICartServices services)
        {
            this.services = services;
        }
        [HttpPost("ThemSanPhamVaoGioHang")]
        public async Task<IActionResult> ThemSanPhamVaoGioHang(ChiTietGioHangSanPhamCreate info)
        {
            var data = await this.services.ThemSanPham(info);
            return Ok(data);
        }
        [HttpGet("GioHangByKhachHang")]
        public async Task<IActionResult> GioHangByKhachHang(string id)
        {
            var data = await this.services.GioHangViews(id);
            return Ok(data);
        }
    }
}
