using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User> Register(User user, string password);
        Task<string> Login(string email, string password);

        Task<User> RegisterByTelegram(long TelegramId, string username);
        Task<string> LoginByTelegram(long TelegramId);
    }
}
