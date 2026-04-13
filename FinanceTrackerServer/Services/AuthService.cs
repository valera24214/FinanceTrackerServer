using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.DTO.AuthAccounts;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FinanceTrackerServer.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly IAuthAccountFactory _authFactory;
        public AuthService(AppDbContext context, IDistributedCache cache, IConfiguration configuration, IAuthAccountFactory authFactory)
        {
            _context = context;
            _cache = cache;
            _config = configuration;
            _authFactory = authFactory;
        }

        private async Task<bool> UserExist(string providerId)
        {
            return await _context.AuthAccounts.AnyAsync(x => x.ProviderId == providerId);
        }

        private string GenerateJwtToken(User user)
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

        private string GenerateEmailVerificationCode()
        {
            var rnd = new Random();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 6; i++)
            {
                sb.Append(rnd.Next(10));
            }

            return sb.ToString();
        }

        private string QuickSha256Hash(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(inputBytes);
                return Convert.ToHexString(hash).ToLower();
            }
        }

        public async Task<string> SendEmailVerificationCode(string email)
        {
            if (await UserExist(email))
                throw new ArgumentException("email already registered");

            var code = GenerateEmailVerificationCode();

            var mail = new MailMessage();
            mail.From = new MailAddress(_config["Email:Address"]);
            mail.To.Add(email);
            mail.Subject = "Подтверждение регистрации";
            mail.Body = $"Код для подтверждения электронной почты: {code}";

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_config["Email:Address"], _config["Email:Password"])
            };

            smtp.Send(mail);

            await SetEmailCache(email, code);

            return code;
        }

        private async Task SetEmailCache(string email, string code)
        {
            await _cache.SetStringAsync(code, email, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });
        }

        public async Task<string?> VerifyEmail(string emailToken)
        {
            var email = await _cache.GetStringAsync(emailToken);
            if (email == null)
                throw new ArgumentNullException("email");
            else
            {
                await _cache.RemoveAsync(emailToken);

                var regToken = Guid.NewGuid().ToString("N");
                await SetEmailCache(email, regToken);

                return regToken;
            }
        }

        public async Task SetPassword(string regToken, string password)
        {
            var email = await _cache.GetStringAsync(regToken);
            if (email == null)
                throw new ArgumentNullException();
            else
            {
                await _cache.RemoveAsync(regToken);

                var passwordDto = new PasswordAccountDto
                {
                    Email = email,
                    Password = password
                };

                var user = new User();
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var acct = _authFactory.CreatePasswordAccount(user.Id, passwordDto);
                await _context.AuthAccounts.AddAsync(acct);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(string jwtToken, string refreshToken)> LoginByPassword(PasswordAccountDto dto)
        {
            if (!await UserExist(dto.Email))
                throw new ArgumentException("This user doesn't exist");

            var acct = _context.AuthAccounts.FirstOrDefault(a => a.ProviderId == dto.Email);

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, acct.PasswordHash))
                throw new ArgumentException("Invalid password");

            var user = _context.Users.FirstOrDefault(u => u.Id == acct.UserId);

            var refreshToken = Guid.NewGuid().ToString();
            var refreshTokenHash = QuickSha256Hash(refreshToken);

            await _cache.SetStringAsync(refreshTokenHash, acct.UserId.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(14)
            });

            return (GenerateJwtToken(user), refreshToken);
        }

        public async Task<(string jwtToken, string refreshToken)> RefreshTokens(string refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentException("Refresh token is null");

            var refreshTokenHash = QuickSha256Hash(refreshToken);
            var userIdStr = await _cache.GetStringAsync(refreshTokenHash);
            if (userIdStr == null)
                throw new ArgumentNullException("Refresh token doesn't exist or has been expired");
  
            var userId = Convert.ToInt32(userIdStr);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new ArgumentNullException("User not found");

            var acct = await _context.AuthAccounts.FirstOrDefaultAsync(a => a.UserId == userId);

            var newRefreshToken = Guid.NewGuid().ToString();
            refreshTokenHash = QuickSha256Hash(newRefreshToken);
            
            await _cache.RemoveAsync(refreshTokenHash);
            await _cache.SetStringAsync(refreshTokenHash, userId.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(14)
            });

            return (GenerateJwtToken(user), newRefreshToken);
        }

        private async Task<User> RegisterByTelegram(TelegramAccountDto dto)
        {
            if (await UserExist(dto.Id.ToString()))
                throw new ArgumentException("User already registered");

            var user = new User();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var acct = _authFactory.CreateTelegramAccount(user.Id, dto);
            await _context.AuthAccounts.AddAsync(acct);
            await _context.SaveChangesAsync();

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

            return GenerateJwtToken(user);
        }


    }
}
