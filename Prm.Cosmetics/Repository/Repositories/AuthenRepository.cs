using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.Data;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Model;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class AuthenRepository :IAuthenRepository
    {
        private readonly ComesticDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthenRepository(ComesticDbContext context,IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string GenerateJwtToken(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString() ?? string.Empty),
            new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
            new Claim("RoleId", user.RoleId.ToString() ?? string.Empty),
            new Claim(ClaimTypes.Role ,user.Role.Name?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            };



            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> Login(LoginModel model)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return null;
            }

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return null;
            }
            var claims = new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString() ?? string.Empty),
            new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
            new Claim("RoleId", user.RoleId.ToString() ?? string.Empty),
            new Claim(ClaimTypes.Role ,user.Role.Name?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
               };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> Register(RegisterModel registerDTO)
        {
            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDTO.Email);
            if (existingUser != null)
            {
                return null; // Or throw an exception, or return a specific error message
            }

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password);

            // Create new user
            var user = new User
            {
                Email = registerDTO.Email,
                PasswordHash = hashedPassword,
                FullName = registerDTO.FullName,
                RoleId = 2, // Default role, adjust as needed
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            user.Role = await _context.Roles.FindAsync(user.RoleId);
            // Generate JWT token for the new user
            return GenerateJwtToken(user);
        }

    }
}
