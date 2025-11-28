using System.Security.Cryptography;
using System.Text;
using ClipperCoffeeCorner.Data;
using ClipperCoffeeCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClipperCoffeeCorner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // check if username already exists
            var existing = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (existing != null)
            {
                return Conflict(new { message = "Username already exists." });
            }

            var user = new User
            {
                UserName = request.UserName,
                UserRole = request.UserRole,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                NotificationPref = request.NotificationPref,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();   // user.UserId now set

            // hash password
            CreatePasswordHash(request.Password, out var hash, out var salt);

            var pwd = new Password
            {
                UserId = user.UserId,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.Passwords.Add(pwd);
            await _db.SaveChangesAsync();

            var response = new UserResponse
            {
                UserId = user.UserId,
                UserName = user.UserName,
                UserRole = user.UserRole,
                Email = user.Email
            };

            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, response);
        }

        // GET: api/users/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponse>> GetById(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            return new UserResponse
            {
                UserId = user.UserId,
                UserName = user.UserName,
                UserRole = user.UserRole,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber, // !!!
                NotificationPref = user.NotificationPref ?? "None" // !!!
            };
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<ActionResult<UserResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password." });

            var pwd = await _db.Passwords
                .Where(p => p.UserId == user.UserId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (pwd == null || !VerifyPasswordHash(request.Password, pwd.PasswordHash, pwd.PasswordSalt))
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var response = new UserResponse
            {
                UserId = user.UserId,
                UserName = user.UserName,
                UserRole = user.UserRole,
                Email = user.Email
            };

            return Ok(response);
        }

        // === password helpers ===

        private static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA256();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private static bool VerifyPasswordHash(string password, byte[] hash, byte[]? salt)
        {
            if (salt == null) return false;
            using var hmac = new HMACSHA256(salt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(computed, hash);
        }
    }
}
