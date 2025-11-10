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

        public async Task BindTelegram(int userId, long telegramId, string telegramUsername)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user.TelegramId.HasValue)
                throw new ArgumentException("User already have telegram data");

            user.TelegramId = telegramId;
            user.TelegramUsername = telegramUsername;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task BindEmail(int userId, string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u=>u.Id == userId);
            if (!string.IsNullOrEmpty(user.Email))
                throw new ArgumentException("User already have email");

            user.Email = email;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
