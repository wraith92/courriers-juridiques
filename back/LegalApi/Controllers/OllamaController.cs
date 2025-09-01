using Microsoft.AspNetCore.Mvc;
using LegalApi.Services;

namespace LegalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OllamaController : ControllerBase
    {
        private readonly OllamaService _ollamaService;

        public OllamaController(OllamaService ollamaService)
        {
            _ollamaService = ollamaService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] PromptRequest request)
        {
            var response = await _ollamaService.GenerateResponse(request.Prompt);
            return Ok(new { answer = response });
        }
    }

    public class PromptRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}
