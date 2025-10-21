using FinanceTrackerServer.Models.DTO.Users;

namespace FinanceTrackerServer.Services.Interfaces
{
    public interface ITelegramAuthService
    {
       bool ValidateTelegramAuth(TelegramDto telegramDto);
    }
}
