using Microsoft.AspNetCore.Mvc;
using ClipperCoffeeCorner.Dtos.Auth;   // <-- LoginRequest, RegisterRequest

namespace ClipperCoffeeCorner.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        // POST /auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            // must provide at least one identifier + password
            var hasAnyId =
                !string.IsNullOrWhiteSpace(req.Username) ||
                !string.IsNullOrWhiteSpace(req.PhoneNumber) ||
                !string.IsNullOrWhiteSpace(req.Email) ||
                !string.IsNullOrWhiteSpace(req.ClipperId) ||
                !string.IsNullOrWhiteSpace(req.GroupOrderId);

            if (!hasAnyId || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new
                {
                    error = "VALIDATION_ERROR",
                    message = "Provide at least one identifier (username, phone, email, clipperId, groupOrderId) and a password."
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

            // success (mock response)
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

        // POST /auth/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new
                {
                    error = "VALIDATION_ERROR",
                    message = "username and password are required"
                });
            }

            // mock create
            return Created(string.Empty, new
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
