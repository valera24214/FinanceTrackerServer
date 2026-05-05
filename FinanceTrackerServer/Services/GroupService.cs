using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models;
using FinanceTrackerServer.Models.DTO;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace FinanceTrackerServer.Services
{
    public class GroupService : IGroupService
    {
        private IMemoryCache _cache;
        private readonly AppDbContext _context;
        private int _userId;

        public GroupService(IMemoryCache cache, AppDbContext context, IUserProvider userProvider)
        {
            _cache = cache;
            _context = context;
            _userId = userProvider.UserId;
        }

        public async Task<GroupDto> Create()
        {
            var user = await _context.Users.FindAsync(_userId);

            var group = new Group();
            await _context.Groups.AddAsync(group);

            user.Group = group;
            await _context.SaveChangesAsync();

            return new GroupDto
            {
                Id = group.Id,
            };
        }

        public async Task Delete()
        {
            var user = await _context.Users.FindAsync(_userId);
            if (user.GroupId == null)
                throw new NotFoundException("Group Doesn't Exist");

            var group = await _context.Groups.FindAsync(user.GroupId);
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GenerateInviteCode()
        {
            var user = await _context.Users.FindAsync(_userId);
            if (user.GroupId == null)
                throw new NotFoundException("User doesn't have group");

            if (_cache.TryGetValue(user.GroupId, out string existingCode))
            {
                return existingCode;
            }

            var rnd = new Random();
            var code = rnd.Next(100_000, 1_000_000).ToString();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");

            _cache.Set(code, user.GroupId, TimeSpan.FromMinutes(15));

            return code;
        }

        public async Task<int> ValidateInviteCode(string code)
        {
            if (!_cache.TryGetValue(code, out int groupId))
                throw new NotFoundException("Invalid code or code has been expired");

            var user = await _context.Users.FindAsync(_userId);
            user.GroupId = groupId;
            _cache.Remove(code);
            await _context.SaveChangesAsync();
            
            return groupId;
        }
    }
}
