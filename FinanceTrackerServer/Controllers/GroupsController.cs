using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.DTO;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services;
using FinanceTrackerServer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTrackerServer.Controllers
{
    [Route("api/groups")]
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private IGroupService _groupService;
        private AppDbContext _context;

        public GroupsController(IGroupService groupService, AppDbContext context)
        {
            _groupService = groupService;
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Users.FindAsync(userId);

            if (user.GroupId != null)
                return BadRequest("User already in group");

            var group = await _groupService.Create();
            user.GroupId = group.Id;
            await _context.SaveChangesAsync();

            return Ok(new { id = group.Id });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Users.FindAsync(userId);

            if (!user.GroupId.HasValue)
                throw new NullReferenceException("This user is not in the group");

            await _groupService.Delete((int)user.GroupId);
            return NoContent();
        }

        [HttpGet("invite_code")]
        public async Task<IActionResult> GetInviteCode()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = _context.Users.Find(userId);

            if (!user.GroupId.HasValue)
            {
                var group = await _groupService.Create();
                user.GroupId = group.Id;
                await _context.SaveChangesAsync();
            }

            var code = _groupService.GenerateInviteCode((int)user.GroupId);
            return Ok(new { code = code, expiresIn = "15 minutes" });
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinGroup([FromBody] JoinGroupDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var (isValid, groupId) = _groupService.ValidateInviteCode(request.InviteCode);

            if (!isValid)
                return BadRequest(new { message = "Invalid or expired invite code" });

            var user = await _context.Users.FindAsync(userId);
            user.GroupId = groupId;
            await _context.SaveChangesAsync();

            return Ok(new { groupId = groupId });
        }
    }
}
