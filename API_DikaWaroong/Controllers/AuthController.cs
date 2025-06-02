namespace API_DikaWaroong.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using API_DikaWaroong.Data;
    using API_DikaWaroong.Models;
    using API_DikaWaroong.Dtos;
    using System;
    using BCrypt = BCrypt.Net.BCrypt;
    using Microsoft.AspNetCore.Authorization;

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var akun = new Akun
            {
                Email = dto.Email,
                Username = dto.Username,
                Password = BCrypt.HashPassword(dto.Password),
                Role_Id_Role = dto.RoleId
            };

            _context.Akuns.Add(akun);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registrasi berhasil." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var akun = await _context.Akuns
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Email == dto.Email);

            if (akun == null || !BCrypt.Verify(dto.Password, akun.Password))
            {
                return Unauthorized(new { message = "Email atau password salah." });
            }

            var token = GenerateJwtToken(akun);

            return Ok(new
            {
                token,
                akun = new
                {
                    akun.Id_Akun,
                    akun.Username,
                    akun.Email,
                    Role = akun.Role?.Nama_Role
                }
            });
        }


        private string GenerateJwtToken(Akun akun)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, akun.Id_Akun.ToString()),
        new Claim(ClaimTypes.Email, akun.Email),
        new Claim(ClaimTypes.Role, akun.Role.Nama_Role)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var akun = await _context.Akuns
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Id_Akun.ToString() == userId);

            if (akun == null)
                return NotFound();

            return Ok(new
            {
                akun.Id_Akun,
                akun.Username,
                akun.Email,
                Role = akun.Role.Nama_Role
            });
        }

    }

}
