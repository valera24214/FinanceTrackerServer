using FinanceTrackerServer.Models.DTO.AuthAccounts;
using FinanceTrackerServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTrackerServer.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register/password")]
        public async Task<IActionResult> RegisterByPassword(PasswordAccountDto dto)
        {
            try
            {
                await _authService.RegisterByPassword(dto);
                return Ok(new
                {
                    message = "Registration successful"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login/password")]
        public async Task<IActionResult> Login(PasswordAccountDto dto)
        {
            try
            {
                var token = await _authService.LoginByPassword(dto);
                if (token == null)
                    return Unauthorized("Invalid credentials");

                return Ok(new { token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login/telegram")]
        public async Task<IActionResult> LoginByTelegram(TelegramAccountDto dto)
        {
            try
            {
                var token = await _authService.LoginByTelegram(dto);
                if (token == null)
                    return Unauthorized("Invalid credentials");

                return Ok(new { token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
