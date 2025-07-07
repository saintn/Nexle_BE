using Microsoft.AspNetCore.Mvc;
using nexle_api.Data;
using nexle_api.Models.Dtos;
using nexle_api.Models;
using System.Net.Mail;
using nexle_api.Models.Dtos.Signup;
using Microsoft.EntityFrameworkCore;
using nexle_api.Models.Dtos.Signin;
using nexle_api.Helpers;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using nexle_api.Models.Dtos.RefreshToken;

namespace nexle_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(ApplicationDbContext context, JwtTokenService jwtService) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly JwtTokenService _jwtService = jwtService;

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupRequest request)
        {
            try {
                try { var mail = new MailAddress(request.Email); }
                catch { return BadRequest("Invalid email format."); }

                if (request.Password.Length < 8 || request.Password.Length > 20)
                    return BadRequest("Password must be between 8-20 characters.");

                if (_context.Users.Any(u => u.Email == request.Email))
                    return BadRequest("Email is already in use.");

                var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var user = new User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Hash = hash
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                var response = new SignupResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                };
                return Created("", response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPost("signin")]
        public IActionResult Signin([FromBody] SigninRequest request)
        {
            try
            {
                try { var mail = new MailAddress(request.Email); }
                catch { return BadRequest("Invalid email format."); }

                if (request.Password.Length < 8 || request.Password.Length > 20)
                    return BadRequest("Password must be between 8-20 characters.");

                var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Hash))
                    return BadRequest("Invalid credentials.");

                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = Guid.NewGuid().ToString();

                var refreshEntry = new Token
                {
                    UserId = user.Id,
                    RefreshToken = refreshToken,
                    ExpiresIn = DateTime.UtcNow.AddDays(30).ToString("o"),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Tokens.Add(refreshEntry);
                _context.SaveChanges();

                var response = new SigninResponse
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    User = new UserInfoDto
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var tokens = _context.Tokens.Where(t => t.UserId == userId);
                _context.Tokens.RemoveRange(tokens);
                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var tokenEntry = _context.Tokens.FirstOrDefault(t => t.RefreshToken == request.RefreshToken);
                if (tokenEntry == null)
                    return NotFound("Refresh token not found.");

                _context.Tokens.Remove(tokenEntry);

                var user = _context.Users.Find(tokenEntry.UserId);
                if (user == null)
                    return NotFound("User not found.");

                var newAccessToken = _jwtService.GenerateAccessToken(user);
                var newRefreshToken = Guid.NewGuid().ToString();

                var newTokenEntry = new Token
                {
                    UserId = user.Id,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = DateTime.UtcNow.AddDays(30).ToString("o"),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Tokens.Add(newTokenEntry);
                _context.SaveChanges();

                return Ok(new RefreshTokenResponse
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
