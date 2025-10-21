using FinanceTrackerServer.Models.DTO.Users;
using FinanceTrackerServer.Services.Interfaces;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace FinanceTrackerServer.Services
{
    public class TelegramAuthServive : ITelegramAuthService
    {
        private readonly string _botToken;
        private readonly ILogger<TelegramAuthServive> _logger;

        public TelegramAuthServive(IConfiguration configuration, ILogger<TelegramAuthServive> logger)
        {
            _botToken = configuration["Telegram:BotToken"] ?? 
                throw new ArgumentNullException("Telegram:BotToken is not configured");
            
            _logger = logger;
        }

        public bool ValidateTelegramAuth(TelegramDto telegramDto)
        {
            try
            {
                if(!IsAuthDateValid(telegramDto.AuthDate))
                {
                    _logger.LogWarning("Telegram auth data is too old");
                    return false;
                }

                return IsTelegramHashValid(telegramDto);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error validating Telegram auth data");
                return false;
            }
        }

        private bool IsAuthDateValid(long authDate)
        {
            var authDateTime = DateTimeOffset.FromUnixTimeSeconds(authDate);
            var now = DateTimeOffset.UtcNow;
            return (now - authDateTime) <= TimeSpan.FromHours(24);
        }

        private string BuildCheckString(TelegramDto telegramDto)
        {
            var fields = new Dictionary<string, string>();

            if (telegramDto.Id != 0)
                fields.Add("id", telegramDto.Id.ToString());
            if (!string.IsNullOrEmpty(telegramDto.FirstName))
                fields.Add("first_name", telegramDto.FirstName);
            if (!string.IsNullOrEmpty(telegramDto.LastName))
                fields.Add("last_name", telegramDto.LastName);
            if (!string.IsNullOrEmpty(telegramDto.Username))
                fields.Add("username", telegramDto.Username);
            if (!string.IsNullOrEmpty(telegramDto.PhotoUrl))
                fields.Add("photo_url", telegramDto.PhotoUrl);

            fields.Add("auth_date", telegramDto.AuthDate.ToString());

            return string.Join("\n", fields.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));
        }

        private static byte[] ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        private static string BytesToHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }

        private bool IsTelegramHashValid(TelegramDto telegramDto)
        {
            var checkString = BuildCheckString(telegramDto);
            using var hmac = new HMACSHA256(ComputeHash(_botToken));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(checkString));
            var computedHashString = BytesToHex(computedHash);

            return string.Equals(computedHashString, telegramDto.Hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
