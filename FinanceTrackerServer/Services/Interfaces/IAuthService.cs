using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User> Register(User user, string password);
        Task<string> Login(string username, string password);
    }
}
