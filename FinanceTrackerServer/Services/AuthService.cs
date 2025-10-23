using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services.Interfaces;
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

        private async Task<bool> UserExist(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email);
        }

        private async Task<bool> UserExist(long telegramId)
        {
            return await _context.Users.AnyAsync(x => x.TelegramId == telegramId);
        }

        private string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
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

        public async Task<User> RegisterByTelegram(long telegramId, string username)
        {
            if (await UserExist(telegramId))
                throw new ArgumentException("User already registered");

            var user = new User
            {
                Username = username,
                TelegramId = telegramId,
                TelegramUsername = username
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<string> LoginByTelegram(long telegramId)
        {
            if (!await UserExist(telegramId))
                throw new ArgumentException("This user doesn't exist");

            var user = _context.Users.FirstOrDefault(u => u.TelegramId == telegramId);

            return GenerateToken(user);
        }

        public async Task BindTelegram(int userId, long telegramId, string telegramUsername)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user.TelegramId.HasValue)
                throw new ArgumentException("User already have telegram data");

            user.TelegramId = telegramId;
            user.TelegramUsername = telegramUsername;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task BindEmail(int userId, string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (!string.IsNullOrEmpty(user.Email))
                throw new ArgumentException("User already have email");

            user.Email = email;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
