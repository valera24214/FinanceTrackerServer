using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Interfaces
{
    public interface IAuthService
    {
        Task<User> Register(User user, string password);
        Task<string> Login(string username, string password);
    }
}
