using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.DTO.AuthAccounts;
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
        private readonly IAuthAccountFactory _authFactory;
        public AuthService(AppDbContext context, IConfiguration configuration, IAuthAccountFactory authFactory)
        {
            _context = context;
            _config = configuration;
            _authFactory = authFactory;
        }

        private async Task<bool> UserExist(string providerId)
        {
            return await _context.AuthAccounts.AnyAsync(x => x.ProviderId == providerId);
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

        public async Task<AuthAccount> RegisterByPassword(PasswordAccountDto dto)
        {
            if (await UserExist(dto.Email))
                throw new ArgumentException("User already registered");

            var user = new User();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var acct = await _authFactory.CreatePasswordAccountAsync(user.Id, dto);

            return acct;
        }

        public async Task<string> LoginByPassword(PasswordAccountDto dto)
        {
            if (!await UserExist(dto.Email))
                throw new ArgumentException("This user doesn't exist");

            var acct = _context.AuthAccounts.FirstOrDefault(a => a.ProviderId == dto.Email);
            var user = _context.Users.FirstOrDefault(u => u.Id == acct.UserId);

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, acct.PasswordHash))
                throw new ArgumentException("Invalid password");

            return GenerateToken(user);
        }

        private async Task<User> RegisterByTelegram(TelegramAccountDto dto)
        {
            if (await UserExist(dto.Id.ToString()))
                throw new ArgumentException("User already registered");

            var user = new User();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var acct = _authFactory.CreateTelegramAccountAsync(user.Id, dto);

            return user;
        }

        public async Task<string> LoginByTelegram(TelegramAccountDto dto)
        {
            var user = new User();
            if (!await UserExist(dto.Id.ToString()))
            {
                user = await RegisterByTelegram(dto);
            }
            else 
            {
                var acct = _context.AuthAccounts.FirstOrDefault(u => u.ProviderId == dto.Id.ToString());
                user = _context.Users.FirstOrDefault(u => u.Id == acct.UserId);
            }

            return GenerateToken(user);
        }

        
    }
}
