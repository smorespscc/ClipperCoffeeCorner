using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ClipperCoffeeCorner.Services;
using System.IO;
using System.Threading.Tasks;

namespace Controllers
{
    [Route("api/webhooks")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly ISquareWebhookService _webhookService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(ISquareWebhookService webhookService, ILogger<WebhookController> logger)
        {
            _webhookService = webhookService;
            _logger = logger;
        }

        [HttpPost("square")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Square()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var requestUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";
            var signature = Request.Headers["x-square-signature"].ToString();

            if (!_webhookService.VerifySignature(requestUrl, body, signature))
            {
                return BadRequest();
            }

            // handle...
            return Ok();
        }
    }
}