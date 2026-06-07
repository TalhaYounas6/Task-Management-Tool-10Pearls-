using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;


namespace TaskManagement.API.Controllers
{
    [Route("api/[controller]")] //  base route: /api/auth
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

       
        // Dependency Injection
        public AuthController(UserManager<User> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        // POST: /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            // Check if user exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email already in use." });

            // Create the new user object
            var newUser = new User
            {
                Email = dto.Email,
                UserName = dto.Email,
                FullName = dto.FullName
            };

            // Save to database (Identity automatically hashes the password)
            var result = await _userManager.CreateAsync(newUser, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User registered successfully!" });
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            // Find the user
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            // Verify password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
                return Unauthorized(new { message = "Invalid email or password." });

            // Generate JWT Token
            var tokenString = await GenerateJwtToken(user);

            return Ok(new { token = tokenString, message = "Login successful!" });
        }

        // GET ALL USERS 
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.Select(u => new { u.Id, u.FullName, u.Email }).ToListAsync();
            return Ok(users);
        }

        // MAKE ME AN ADMIN 
        [HttpPost("make-admin")]
        public async Task<IActionResult> MakeAdmin([FromBody] string email)
        {
            // Check if the "Admin" role exists in the database. If not, create it.
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Find the user
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("User not found.");

            // Assign the Admin role
            await _userManager.AddToRoleAsync(user, "Admin");

            return Ok(new { message = $"{user.Email} is now an Admin!" });
        }

        // Helper function to create the JWT 
        private async Task<string> GenerateJwtToken(User user)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("Server configuration error: Missing JWT Key in Environment Variables.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Standard Claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("FullName", user.FullName)
            };

            // Fetch user roles from the database and add them to the token!
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpGet("me")]
        [Authorize] // Ensures only logged-in users can call this
        public async Task<IActionResult> GetMyProfile()
        {
            
            var currentUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

           
            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null) return NotFound("User not found.");

            
            return Ok(new
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            });
        }
    }
}