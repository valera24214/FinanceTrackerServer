using FinanceTrackerServer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTrackerServer.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPut("bind/email")]
        public async Task<IActionResult> BindEmail([FromQuery] string email, string password)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await _userService.BindEmail(userId, email, password);

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> BindTelegram([FromQuery] long telegramId, string telegramUsername)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await _userService.BindTelegram(userId, telegramId, telegramUsername);

            return Ok();
        }

    }
}
