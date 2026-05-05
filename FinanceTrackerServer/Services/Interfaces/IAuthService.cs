using FinanceTrackerServer.Models.DTO.AuthAccounts;
using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> SendEmailVerificationCode(string email);
        Task<string?> VerifyEmail(string emailToken);
        Task<int> SetPassword(string regToken, string password);

        Task<(string jwtToken, string refreshToken)> LoginByPassword(PasswordAccountDto dto);
        Task<(string jwtToken, string refreshToken)> RefreshTokens(string refreshToken);

        Task<string> LoginByTelegram(TelegramAccountDto dto);
    }
}
