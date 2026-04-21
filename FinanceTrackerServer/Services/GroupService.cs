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

        public GroupService(IMemoryCache cache, AppDbContext context)
        {
            _cache = cache;
            _context = context;
        }

        public async Task<GroupDto> Create()
        {
            var group = new Group();
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();

            return new GroupDto
            {
                Id = group.Id,
            };
        }

        public async Task Delete(int id)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
                throw new NotFoundException("Group Doesn't Exist");

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
        }

        public string GenerateInviteCode(int groupId)
        {
            if (!_context.Groups.Any(g => g.Id == groupId))
                throw new NotFoundException("Group Doesn't Exist");

            if (_cache.TryGetValue(groupId, out string existingCode))
            {
                return existingCode;
            }

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");
            var code = $"{groupId}_{timestamp}";
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(code));

            _cache.Set(encoded, groupId, TimeSpan.FromMinutes(15));

            return encoded;
        }

        public (bool isValid, int groupId) ValidateInviteCode(string code)
        {
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
                throw new ConflictException("Invalid Group Code");
            }

            _cache.Remove(decoded);

            return (true, groupId);
        }
    }
}
