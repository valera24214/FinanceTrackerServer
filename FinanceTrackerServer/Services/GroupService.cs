using FinanceTrackerServer.Data;
using FinanceTrackerServer.Interfaces;
using FinanceTrackerServer.Models.DTO;
using FinanceTrackerServer.Models.Entities;
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

        public async Task<GroupDto> Create(Group group)
        {
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();

            return new GroupDto 
            {
                Id = group.Id,
                Name = group.Name,
            };
        }

        public async Task Delete(int id)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
                throw new ArgumentNullException("Invalid group id");

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
        }

        public string GenerateInviteCode(int groupId)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");
            var code = $"{groupId}_{timestamp}";
            _cache.Set(code, true, TimeSpan.FromMinutes(5));

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(code));
        }

        public (bool isValid, int groupId) ValidateInviteCode(string code)
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(code));
                var parts = decoded.Split('_');

                if (parts.Length != 2) 
                    return (false, 0);

                var groupId = int.Parse(parts[0]);
                var timestamp = DateTime.ParseExact(parts[1], "yyyyMMddHHmm", null);

                if (DateTime.UtcNow - timestamp > TimeSpan.FromMinutes(5) ||
                    !_cache.TryGetValue(decoded, out _))
                {
                    return (false, 0);
                }

                _cache.Remove(decoded);

                return (true, groupId);
            }
            catch
            {
                return (false, 0);
            }
        }
    }
}
