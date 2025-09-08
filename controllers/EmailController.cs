using Microsoft.AspNetCore.Mvc;
using DocumentMailService.Models;
using DocumentMailService.Services;

namespace DocumentMailService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<ActionResult<EmailResponse>> SendEmail([FromBody] EmailRequest request)
        {
            var response = await _emailService.SendEmailAsync(request);
            if (response.Success)
                return Ok(response);
            return StatusCode(500, response);
        }

        [HttpPost("send-with-embedding")]
        public async Task<ActionResult<EmailResponse>> SendEmailWithEmbedding([FromBody] EmailWithEmbeddingRequest request)
        {
            var response = await _emailService.SendEmailWithEmbeddingAsync(request);
            if (response.Success)
                return Ok(response);
            return StatusCode(500, response);
        }
    }
}