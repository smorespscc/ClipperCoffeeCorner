using Microsoft.AspNetCore.Mvc;

namespace ClipperCoffeeCorner.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        public class LoginRequest
        {
            public string? Username { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Email { get; set; }
            public string? ClipperId { get; set; }
            public string? GroupOrderId { get; set; }
            public string Password { get; set; } = "";
        }

        // POST /auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            // must provide at least one identifier
            if (string.IsNullOrWhiteSpace(req.Username)
                && string.IsNullOrWhiteSpace(req.PhoneNumber)
                && string.IsNullOrWhiteSpace(req.Email)
                && string.IsNullOrWhiteSpace(req.ClipperId)
                && string.IsNullOrWhiteSpace(req.GroupOrderId))
            {
                return BadRequest(new
                {
                    error = "VALIDATION_ERROR",
                    message = "Provide at least one identifier (username, phone, email, clipperId, groupOrderId)."
                });
            }

            // fake password check
            if (req.Password != "P@ssw0rd!")
            {
                return Unauthorized(new
                {
                    error = "INVALID_CREDENTIALS",
                    message = "Login failed."
                });
            }

            // success
            return Ok(new
            {
                accessToken = "jwt-or-bearer-here",
                refreshToken = "optional-refresh-token",
                expiresIn = 3600,
                user = new
                {
                    id = "u-9283",
                    username = req.Username ?? "coffeeUser01",
                    displayName = "Student One",
                    email = req.Email ?? "user@spscc.edu",
                    clipperId = req.ClipperId ?? "CLP-00123",
                    phoneNumber = req.PhoneNumber ?? "360-000-0000",
                    roles = new[] { "Customer" }
                }
            });
        }

        public class RegisterRequest
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public string? ClipperId { get; set; }
        }

        // POST /auth/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            return Created("", new
            {
                id = "u-9999",
                username = req.Username,
                displayName = req.Username,
                email = req.Email,
                phoneNumber = req.PhoneNumber,
                clipperId = req.ClipperId
            });
        }
    }
}
