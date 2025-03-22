using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YeuThichController : ControllerBase
    {
        private readonly IYeuThichServices _yeuthichServices;

        public YeuThichController(IYeuThichServices yeuthichServices)
        {
            _yeuthichServices = yeuthichServices;
        }

        [HttpGet]
        public ActionResult<List<YeuThichView>> GetAllYeuThichs()
        {
            try
            {
                var yeuThichs = _yeuthichServices.GetAllYeuThichs();
                return Ok(yeuThichs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<YeuThichView>> CreateYeuThich([FromBody] YeuThichCreate yeuThichCreate)
        {
            try
            {
                var yeuThich = await _yeuthichServices.CreateYeuThich(yeuThichCreate);
                return Ok(yeuThich);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpDelete("{maYeuThich}")]
        public async Task<ActionResult> DeleteYeuThich(int maYeuThich)
        {
            try
            {
                var result = await _yeuthichServices.DeleteYeuThich(maYeuThich);
                if (result)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound("YeuThich not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}