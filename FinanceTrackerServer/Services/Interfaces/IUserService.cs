using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserAsync(int userId);
        Task<List<User>> GetUsersByGroupAsync(int groupId);
    }
}
