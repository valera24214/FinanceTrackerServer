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

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create()
        {
            var group = await _groupService.Create();
            return Ok(new { id = group.Id });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete()
        {
            await _groupService.Delete();
            return NoContent();
        }

        [HttpGet("invite_code")]
        public async Task<IActionResult> GetInviteCode()
        {
            var code = _groupService.GenerateInviteCode();
            return Ok(new { code = code, expiresIn = "15 minutes" });
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinGroup([FromBody] JoinGroupDto request)
        {
            var groupId = _groupService.ValidateInviteCode(request.InviteCode);
            return Ok(new { groupId = groupId });
        }
    }
}
