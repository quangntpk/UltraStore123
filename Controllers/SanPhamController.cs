using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Data.Temp;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class SanPhamController : ControllerBase
    {
        private readonly ISanPhamServices services;

        public SanPhamController(ISanPhamServices services)
        {
            this.services = services;
        }
        [HttpGet("ListSanPham")]
        public async Task<IActionResult> ListSanPham(string? id)
        {
            var data = await this.services.ListSanPham(id);
            return Ok(data);
        }
        [HttpGet("SanPhamByID")]
        public async Task<IActionResult> SanPhamByID(string id)
        {
            var data = await this.services.SanPhamByID(id);
            return Ok(data);
        }
        [HttpPost("NewSanPham")]
        public async Task<IActionResult> NewSanPham()
        {
            return Ok();
        }
        [HttpGet("SanPhamByIDSorted")]
        public async Task<IActionResult> SanPhamByIDSorted(string? id)
        {
            var data = await this.services.SanPhamByIDSorteds(id);
            return Ok(data);
        }
        [Authorize(Roles = "admin,staff")]
        [HttpPost("EditSanPham")]
        public async Task<IActionResult> EditSanPham([FromBody]FullInfoSanPhamEdit info)
        {
            var data = await this.services.EditSanPham(info);
            return Ok(data);
        }
        [Authorize(Roles = "admin,staff")]
        [HttpPost("CreateSanPham")]
        public async Task<IActionResult> CreateSanPham(FullCreateSanPham? info)
        {
            var data = await this.services.CreateSanPham(info);
            return Ok(data);
        }
        [Authorize(Roles = "admin,staff")]
        [HttpGet("DeleteSanPham")]
        public async Task<IActionResult> DeleteSanPham(string id)
        {
            var data = await this.services.DeleteSanPham(id);
            return Ok();
        }
        [Authorize(Roles = "admin,staff")]
        [HttpGet("ActiveSanPham")]
        public async Task<IActionResult> ActiveSanPham(string id)
        {
            var data = await this.services.ActiveSanPham(id);
            return Ok();
        }
        [HttpGet("ListSanPhamLQ")]
        public async Task<IActionResult> ListSanPhamLQ(string id)
        {
            var data = await this.services.ListSanPhamLQ(id);
            return Ok(data);
        }
        [HttpPost("MoTaSanPhamCreate")]
        public async Task<IActionResult> MoTaSanPhamCreate(List<MoTaSanPhamCreateModel> info)
        {
            var data = await this.services.MoTaSanPhamCreate(info);
            return Ok(data);
        }
        [HttpPost("ReportByDate")]
        public async Task<IActionResult> ReportByDate(SelectDateProductView? info)
        {
            var data = await this.services.ReportByDate(info);
            return Ok(data);
        }
    }
}
