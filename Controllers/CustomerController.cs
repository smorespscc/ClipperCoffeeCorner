using ClipperCoffeeCorner.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Controllers
{
    /// <summary>
    /// API Controller for customer operations.
    /// Handles authentication, registration, and profile management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            ICustomerService customerService,
            ILogger<CustomerController> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ==================== AUTHENTICATION ====================

        /// <summary>
        /// Authenticates a customer
        /// POST /api/customer/login
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var (success, customerId, errorMessage) = await _customerService.AuthenticateAsync(
                    request.Email,
                    request.Password);

                if (!success)
                {
                    return Unauthorized(new { message = errorMessage });
                }

                // Set session
                var sessionId = HttpContext.Session.Id;
                await _customerService.SetCurrentCustomerAsync(sessionId, customerId!, request.IsStaff);

                // Validate staff code if staff
                if (request.IsStaff && !string.IsNullOrEmpty(request.StaffCode))
                {
                    var isValidStaffCode = await _customerService.ValidateStaffCodeAsync(request.StaffCode);
                    if (!isValidStaffCode)
                    {
                        return BadRequest(new { message = "Invalid staff code" });
                    }
                }

                return Ok(new
                {
                    success = true,
                    customerId,
                    isStaff = request.IsStaff,
                    message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Logs out current customer
        /// POST /api/customer/logout
        /// </summary>
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                await _customerService.LogoutAsync(sessionId);
                HttpContext.Session.Clear();

                return Ok(new { message = "Logout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "An error occurred during logout" });
            }
        }

        /// <summary>
        /// Validates staff code
        /// POST /api/customer/validate-staff-code
        /// </summary>
        [HttpPost("validate-staff-code")]
        public async Task<ActionResult> ValidateStaffCode([FromBody] StaffCodeRequest request)
        {
            try
            {
                var isValid = await _customerService.ValidateStaffCodeAsync(request.StaffCode);
                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating staff code");
                return StatusCode(500, new { message = "An error occurred while validating staff code" });
            }
        }

        // ==================== REGISTRATION ====================

        /// <summary>
        /// Registers a new customer
        /// POST /api/customer/register
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var (success, customerId, errorMessage) = await _customerService.RegisterCustomerAsync(
                    request.Username,
                    request.Email,
                    request.Phone,
                    request.Password);

                if (!success)
                {
                    return BadRequest(new { message = errorMessage });
                }

                // Save notification preferences
                if (customerId != null)
                {
                    await _customerService.SaveNotificationPreferencesAsync(
                        customerId,
                        request.EmailNotifications,
                        request.TextNotifications);
                }

                return Ok(new
                {
                    success = true,
                    customerId,
                    message = "Registration successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Checks if email is already registered
        /// GET /api/customer/check-email/{email}
        /// </summary>
        [HttpGet("check-email/{email}")]
        public async Task<ActionResult> CheckEmail(string email)
        {
            try
            {
                var isRegistered = await _customerService.IsEmailRegisteredAsync(email);
                return Ok(new { isRegistered });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // ==================== PROFILE ====================

        /// <summary>
        /// Gets current customer profile
        /// GET /api/customer/profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult> GetProfile()
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var customerId = await _customerService.GetCurrentCustomerIdAsync(sessionId);

                if (string.IsNullOrEmpty(customerId))
                {
                    return Unauthorized(new { message = "Not authenticated" });
                }

                var profile = await _customerService.GetCustomerProfileAsync(customerId);
                return Ok(new { profile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile");
                return StatusCode(500, new { message = "An error occurred while retrieving profile" });
            }
        }

        /// <summary>
        /// Gets notification preferences
        /// GET /api/customer/notification-preferences
        /// </summary>
        [HttpGet("notification-preferences")]
        public async Task<ActionResult> GetNotificationPreferences()
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var customerId = await _customerService.GetCurrentCustomerIdAsync(sessionId);

                if (string.IsNullOrEmpty(customerId))
                {
                    return Ok(new { emailConsent = false, textConsent = false });
                }

                var (emailConsent, textConsent) = await _customerService.GetNotificationPreferencesAsync(customerId);
                return Ok(new { emailConsent, textConsent });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification preferences");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // ==================== SESSION ====================

        /// <summary>
        /// Gets current session info
        /// GET /api/customer/session
        /// </summary>
        [HttpGet("session")]
        public async Task<ActionResult> GetSession()
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var customerId = await _customerService.GetCurrentCustomerIdAsync(sessionId);
                var isStaff = await _customerService.IsStaffSessionAsync(sessionId);

                return Ok(new
                {
                    isAuthenticated = !string.IsNullOrEmpty(customerId),
                    customerId,
                    isStaff
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }

    // ==================== REQUEST MODELS ====================

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public bool IsStaff { get; set; }
        public string? StaffCode { get; set; }
    }

    public class RegisterRequest
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string Password { get; set; }
        public bool EmailNotifications { get; set; }
        public bool TextNotifications { get; set; }
    }

    public class StaffCodeRequest
    {
        public required string StaffCode { get; set; }
    }
}
