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
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var akun = await _context.Akuns.Include(a => a.Role).FirstOrDefaultAsync(a => a.Email == dto.Email);
            if (akun == null || !BCrypt.Verify(dto.Password, akun.Password))
                return Unauthorized("Email atau password salah.");

            var token = GenerateJwtToken(akun);
            return Ok(new { token });
        }

        private string GenerateJwtToken(Akun akun)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, akun.Id_Akun.ToString()),
            new Claim(ClaimTypes.Email, akun.Email),
            new Claim(ClaimTypes.Role, akun.Role.Nama_Role)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
