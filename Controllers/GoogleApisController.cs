using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Repository;
using UltraStrore.Utils;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleApisController : ControllerBase
    {
        private readonly IGoogleApisServices _googleApisServices;

        public GoogleApisController(IGoogleApisServices googleApisServices)
        {
            _googleApisServices = googleApisServices;
        }

        [HttpPost("generate-image")]
        public async Task<IActionResult> GenerateImage([FromBody] ImageGenerationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _googleApisServices.GenerateImageAsync(request);

            if (string.IsNullOrEmpty(response.GeneratedImageBase64))
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }

        [HttpPost("generate-text")]
        public async Task<IActionResult> GenerateText([FromBody] TextGenerationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _googleApisServices.GenerateTextAsync(request);

            if (string.IsNullOrEmpty(response.GeneratedText))
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }

        [HttpPost("search-products")]
        public async Task<IActionResult> SearchProducts([FromBody] TextGenerationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _googleApisServices.SearchProductsAsync(request);

            if (string.IsNullOrEmpty(response.GeneratedText))
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }

        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] TextGenerationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _googleApisServices.AddToCartAsync(request);

            if (string.IsNullOrEmpty(response.GeneratedText))
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }
    }
}