using FinanceTrackerServer.Data;
using FinanceTrackerServer.Interfaces;
using FinanceTrackerServer.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinanceTrackerServer.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
        }

        public async Task<bool> UserExist(string Email)
        {
            return await _context.Users.AnyAsync(x => x.Email == Email);
        }

        public string GenerateToken(User user)
        {
            var cliams = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: cliams,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User> Register(User user, string password)
        {
            if (await UserExist(user.Username))
                throw new ArgumentException("User already registered");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<string> Login(string email, string password)
        {
            if (!await UserExist(email))
                throw new ArgumentException("This user doesn't exist");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new ArgumentException("Invalid password");

            return GenerateToken(user);
        }

    }
}
