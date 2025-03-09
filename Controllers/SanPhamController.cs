using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Data.Temp;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
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
        [HttpPost("EditSanPham")]
        public async Task<IActionResult> EditSanPham([FromBody]List<SanPhamEdit> info)
        {
            var data = await this.services.EditSanPham(info);
            return Ok(data);
        }
        [HttpPost("CreateSanPham")]
        public async Task<IActionResult> CreateSanPham([FromBody] List<SanPhamCreate> info)
        {
            var data = await this.services.CreateSanPham(info);
            return Ok(data);
        }
        [HttpGet("DeleteSanPham")]
        public async Task<IActionResult> DeleteSanPham(string id)
        {
            var data = await this.services.DeleteSanPham(id);
            return Ok();
        }
        [HttpGet("ActiveSanPham")]
        public async Task<IActionResult> ActiveSanPham(string id)
        {
            var data = await this.services.ActiveSanPham(id);
            return Ok();
        }
    }
}
