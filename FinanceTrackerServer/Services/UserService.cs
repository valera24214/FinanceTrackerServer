using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrackerServer.Services
{
    public class UserService:IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            return user;
        }

        public async Task<List<User>> GetUsersByGroupAsync(int groupId)
        {
            var users = _context.Users.Where(u=>u.GroupId == groupId).ToList();
            return users;
        }
    }
}
