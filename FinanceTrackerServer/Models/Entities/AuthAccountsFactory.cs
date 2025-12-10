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
        public string MetaData { get; set; }
    }

    public interface IAuthAccountFactory
    {
        Task<AuthAccount> CreatePasswordAccountAsync(int userId, PasswordAccountDto dto);
        Task<AuthAccount> CreateTelegramAccountAsync(int userId, TelegramAccountDto dto);
    }

    public class AuthAccountsFactory : IAuthAccountFactory
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthAccountsFactory> _logger;

        public AuthAccountsFactory(AppDbContext context, ILogger<AuthAccountsFactory> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AuthAccount> CreatePasswordAccountAsync(int userId, PasswordAccountDto dto)
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
            };

            return await AddAuthAccount(acct);
        }

        public async Task<AuthAccount> CreateTelegramAccountAsync(int userId, TelegramAccountDto dto)
        {
            var idStr = dto.Id.ToString();
            var metadata = dto.Username is null ? null : JsonConvert.SerializeObject(dto.Username);

            var acct = new AuthAccount
            {
                UserId = userId,
                Provider = AuthProvider.Telegram,
                ProviderId = idStr,
                MetaData = metadata
            };

            return await AddAuthAccount(acct);

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

        private async Task<AuthAccount> AddAuthAccount(AuthAccount acct)
        {
            acct.ProviderId = NormalizeProviderId(acct.Provider, acct.ProviderId);
            if (string.IsNullOrWhiteSpace(acct.ProviderId))
                throw new ArgumentException("ProviderId must be provided for this provider");

            _context.AuthAccounts.Add(acct);
            await _context.SaveChangesAsync();
            return acct;
        }
    }
}
