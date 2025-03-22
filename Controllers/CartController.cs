using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
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
        [HttpPost("ThemComboVaoGioHang")]
        public async Task<IActionResult> ThemComboVaoGioHang(ChiTietGioHangComboCreate info)
        {
            var data = await this.services.ThemCombo(info);
            return Ok(data);
        }
        [HttpPost("TangSoLuongSanPham")]
        public async Task<IActionResult> TangSoLuongSanPham(TangGiamSoLuongGioHang info)
        {
            var data = await this.services.TangSoLuongSanPham(info);
            return Ok(data);
        }
        [HttpPost("GiamSoLuongSanPham")]
        public async Task<IActionResult> GiamSoLuongSanPham(TangGiamSoLuongGioHang info)
        {
            var data = await this.services.GiamSoLuongSanPham(info);
            return Ok(data);
        }
        [HttpDelete("XoaSanPham")]
        public async Task<IActionResult> XoaSanPham(TangGiamSoLuongGioHang info)
        {
            var data = await this.services.XoaChiTietGioHang(info);
            return Ok(data);
        }
        [HttpDelete("XoaCombo")]
        public async Task<IActionResult> XoaCombo(TangGiamSoLuongGioHang info)
        {
            var data = await this.services.XoaChiTietGiohangCombo(info);
            return Ok(data);
        }
        [HttpDelete("XoaComboVersion")]
        public async Task<IActionResult> XoaComboVersion(GioHangComboVersion info)
        {
            var data = await this.services.XoaVersionComboGioHang(info);
            return Ok(data);
        }
    }
}
