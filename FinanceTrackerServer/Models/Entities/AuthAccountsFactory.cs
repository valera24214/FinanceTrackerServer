using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.DTO.AuthAccounts;
using Newtonsoft.Json;

namespace FinanceTrackerServer.Models.Entities
{
    public enum AuthProvider
    {
        Password,
        Telegram
    }

    public class AuthAccount
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public AuthProvider Provider { get; set; }
        public string ProviderId { get; set; }
        public string? PasswordHash { get; set; }
        public string? MetaData { get; set; }
    }

    public interface IAuthAccountFactory
    {
        AuthAccount CreatePasswordAccount(int userId, PasswordAccountDto dto);
        AuthAccount CreateTelegramAccount(int userId, TelegramAccountDto dto);
    }

    public class AuthAccountsFactory : IAuthAccountFactory
    {
        private readonly AppDbContext _context;

        public AuthAccountsFactory(AppDbContext context)
        {
            _context = context;
        }

        public AuthAccount CreatePasswordAccount(int userId, PasswordAccountDto dto)
        {
            var normalized = NormalizeEmail(dto.Email);
            if(string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Password required", nameof(dto.Password));

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var acct = new AuthAccount
            {
                UserId = userId,
                Provider = AuthProvider.Password,
                ProviderId = normalized,
                PasswordHash = hash,
                MetaData = dto.Email
            };

            return acct;
        }

        public AuthAccount CreateTelegramAccount(int userId, TelegramAccountDto dto)
        {
            var idStr = dto.Id.ToString();
            var metadata = dto.Username is null ? null : JsonConvert.SerializeObject(dto.Username);

            var acct = new AuthAccount
            {
                UserId = userId,
                Provider = AuthProvider.Telegram,
                ProviderId = NormalizeProviderId(AuthProvider.Telegram, idStr),
                MetaData = metadata
            };

            return acct;

        }

        private static string NormalizeEmail(string e) => e.Trim().ToLowerInvariant();
        private static string NormalizeProviderId(AuthProvider provider, string? providerId)
        {
            if (string.IsNullOrWhiteSpace(providerId)) return providerId ?? string.Empty;
            return provider switch
            {
                AuthProvider.Password => NormalizeEmail(providerId),
                AuthProvider.Telegram => providerId.Trim(),
                _ => providerId.Trim()
            };
        }
    }
}
