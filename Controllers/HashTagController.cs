using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using UltraStrore.Services;

namespace UltraStrore.Controllers
{
    [Authorize(Roles = "admin,staff")]
    [Route("api/[controller]")]
    [ApiController]
    public class HashTagController : ControllerBase
    {
        private readonly IHashTagServices _hashTagServices;

        public HashTagController(IHashTagServices hashTagServices)
        {
            _hashTagServices = hashTagServices ?? throw new ArgumentNullException(nameof(hashTagServices));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllHashTag()
        {
            try
            {
                var list = await _hashTagServices.GetAllHashTagAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHashTag(int id)
        {
            try
            {
                var hashTag = await _hashTagServices.GetHashTagAsync(id);
                return Ok(hashTag);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateHashTag([FromBody] HashTagCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdHashTag = await _hashTagServices.CreateHashTagAsync(model);
                return CreatedAtAction(nameof(GetHashTag), new { id = createdHashTag.MaHashTag }, createdHashTag);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHashTag(int id, [FromBody] HashTagEdit model)
        {
            if (id != model.MaHashTag)
                return BadRequest("Mã hashtag không khớp.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedHashTag = await _hashTagServices.UpdateHashTagAsync(model);
                return Ok(updatedHashTag);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHashTag(int id)
        {
            try
            {
                var result = await _hashTagServices.DeleteHashTagAsync(id);
                return result ? NoContent() : NotFound("Hashtag không tồn tại.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchHashTag([FromQuery] string tenHashtag)
        {
            try
            {
                var result = await _hashTagServices.SearchHashTagAsync(tenHashtag);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpGet("{id}/SanPham")]
        public async Task<IActionResult> GetSanPhamByHashTag(int id)
        {
            try
            {
                var sanPhams = await _hashTagServices.GetSanPhamByHashTagAsync(id);
                return Ok(sanPhams);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }
    }
}