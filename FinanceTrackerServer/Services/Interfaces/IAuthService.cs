using FinanceTrackerServer.Models.DTO.AuthAccounts;
using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthAccount> RegisterByPassword(PasswordAccountDto dto);
        Task<string> LoginByPassword(PasswordAccountDto dto);

        Task<string> LoginByTelegram(TelegramAccountDto dto);
    }
}
