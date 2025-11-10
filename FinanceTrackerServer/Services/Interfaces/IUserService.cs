namespace FinanceTrackerServer.Services.Interfaces
{
    public interface IUserService
    {

        Task BindTelegram(int userId, long telegramId, string telegramUsername);
        Task BindEmail(int userId, string email, string password);
    }
}
