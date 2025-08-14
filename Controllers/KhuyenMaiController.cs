using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using UltraStrore.Repository;
using UltraStrore.Models.ViewModels;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhuyenMaiController  : ControllerBase
    {
        private readonly IKhuyenMaiServices _services; 
        public KhuyenMaiController(IKhuyenMaiServices services)
        {
            _services = services;
        }
        [HttpGet("ListKhuyenMaiAdmin")]
        public async Task<List<KhuyenMaiView>> ListKhuyenMaiAdmin(int? id)
        {
            var data = await _services.ListKhuyenMaiAdmin(id);
            return data;
        }
        [HttpGet("ListKhuyenMaiUser")]
        public async Task<List<KhuyenMaiView>> ListKhuyenMaiUser(int? id)
        {
            var data = await _services.ListKhuyenMaiUser(id);
            return data;
        }
        [HttpPost("KhuyenMaiCreate")]
        public async Task<APIResponse> KhuyenMaiCreate(KhuyenMaiCreate data)
        {
            var info = await _services.KhuyenMaiCreate(data);
            return info;
             
        }
        [HttpPost("MoTaKhuyenMaiCreate")]
        public async Task<APIResponse> MoTaKhuyenMaiCreate(MoTaKhuyenMai data)
        {
            var info = await _services.MoTaKhuyenMaiCreate(data);
            return info;

        }
        [HttpPost("KhuyenMaiUpdate")]
        public async Task<APIResponse> KhuyenMaiUpdate(KhuyenMaiEdit data)
        {
            var info = await _services.KhuyenMaiUpdate(data);
            return info;
        }
        [HttpPost("MoTaKhuyenMaiUpdate")]
        public async Task<APIResponse> MoTaKhuyenMaiUpdate(MoTaKhuyenMai data)
        {
            var info = await _services.MoTaKhuyenMaiEdit(data);
            return info;
        }
        [HttpPost("DisableKhuyenMai")]
        public async Task<APIResponse> DisableKhuyenMai(int id)
        {
            var info = await _services.DisableKhuyenMai(id);
            return info;
        }
        [HttpGet("TestSanPhamByIDSorted")]
        public async Task<IActionResult> TestSanPhamByIDSorted(string? id)
        {
            try
            {
                // Test connection
                var connectionString = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetConnectionString("DefaultConnection");

                return Ok(new
                {
                    receivedId = id,
                    connectionStringExists = !string.IsNullOrEmpty(connectionString),
                    connectionString = connectionString?.Substring(0, 50) + "..." // Chỉ hiện một phần
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
