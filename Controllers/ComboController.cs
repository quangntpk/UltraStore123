using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ComboController : ControllerBase
    {
        private readonly IComboServices services;

        public ComboController(IComboServices services)
        {
            this.services = services;
        }
        [HttpGet("ComboSanPhamView")]
        public async Task<IActionResult> ComboSanPhamView(int? id)
        {          
            var data = await services.ComboViews(id);
            return Ok(data);   
        }
        [Authorize(Roles = "admin,staff")]
        [HttpGet("ComboSanPhamViewAdmin")]
        public async Task<IActionResult> ComboSanPhamViewAdmin(int? id)
        {           

            var data = await services.ComboViews(id);
                return Ok(data);  
        }

        [Authorize(Roles = "admin,staff")]
        [HttpPost("CreateComboSanPham")]
        public async Task<IActionResult> AddCombo(ComboCreate info)
        {
            var data = await services.AddCombo(info);  
            return Ok();
        }
        [Authorize(Roles = "admin,staff")]
        [HttpPost("EditComboSanPham")]
        public async Task<IActionResult> EditCombo(ComboEdit info)
        {
            var data = await services.EditCombo(info);
            return Ok(data);
        }
        [Authorize(Roles = "admin,staff")]
        [HttpGet("DeleteCombo")]
        public async Task<IActionResult> DeleteCombo(int id)
        {
            var data = await services.XoaCombo(id);
            return Ok(data);
        }
    }
}
