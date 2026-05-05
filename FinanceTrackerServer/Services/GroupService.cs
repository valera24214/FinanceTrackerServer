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
            //TODO: rework code-group storing
            var user = await _context.Users.FindAsync(_userId);
            if (user.GroupId == null)
                throw new NotFoundException("User doesn't have group");

            if (_cache.TryGetValue(user.GroupId, out string existingCode))
            {
                return existingCode;
            }

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");
            var code = $"{user.GroupId}_{timestamp}";
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(code));

            _cache.Set(encoded, user.GroupId, TimeSpan.FromMinutes(15));

            return encoded;
        }

        public async Task<int> ValidateInviteCode(string code)
        {
            // TODO: rework code-group extraction
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(code));
            var parts = decoded.Split('_');

            if (parts.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(parts));

            var groupId = int.Parse(parts[0]);
            var timestamp = DateTime.ParseExact(parts[1], "yyyyMMddHHmm", null);

            if (DateTime.UtcNow - timestamp > TimeSpan.FromMinutes(60) ||
                !_cache.TryGetValue(code, out int cachedGroupId) ||
                !(cachedGroupId == groupId))
            {
                throw new ValidationException("Invalid Group Code");
            }

            var user = await _context.Users.FindAsync(_userId);
            user.GroupId = groupId;
            _cache.Remove(decoded);
            await _context.SaveChangesAsync();
            
            return groupId;
        }
    }
}
