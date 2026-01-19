using FinanceTrackerServer.Models.DTO.AuthAccounts;
using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> SendEmailVerificationCode(string email);
        Task<string?> VerifyEmail(string emailToken);
        Task SetPassword(string regToken, string password);
        Task<string> LoginByPassword(PasswordAccountDto dto);

        Task<string> LoginByTelegram(TelegramAccountDto dto);
    }
}
