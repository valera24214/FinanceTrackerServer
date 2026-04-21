using FinanceTrackerServer.Models;
using FinanceTrackerServer.Models.DTO.AuthAccounts;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;

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

        private void SetReftreshTokenToCookies(HttpResponse responce, string RefreshToken)
        {
            responce.Cookies.Delete("refreshToken");
            responce.Cookies.Append("refreshToken", RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(14),
                    Domain = "localhost"
                }
            );
        }

        [HttpPost("register/password/send_email_code")]
        public async Task<IActionResult> SendEmailCode([FromQuery] string email)
        {
            await _authService.SendEmailVerificationCode(email);
            return Ok();
        }

        [HttpPost("register/password/verify_email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string verification_code)
        {
            var regToken = await _authService.VerifyEmail(verification_code);
            return Ok(new { registration_token = regToken });
        }

        [HttpPost("register/password/set_password")]
        public async Task<IActionResult> SetPassword([FromQuery] string regToken,[FromBody] string password)
        {
            var id = await _authService.SetPassword(regToken, password);
            return Ok(new {id = id});
        }

        [HttpPost("login/password")]
        public async Task<IActionResult> LoginByPassword([FromBody] PasswordAccountDto dto)
        {
            var result = await _authService.LoginByPassword(dto);

            SetReftreshTokenToCookies(Response, result.refreshToken);

            return Ok(new { token = result.jwtToken });
        }

        [HttpPost("login/refresh")]
        public async Task<IActionResult> RefreshTokens()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var result = await _authService.RefreshTokens(refreshToken);

            SetReftreshTokenToCookies(Response, result.refreshToken);

            return Ok(new { token = result.jwtToken });
        }

        [HttpPost("login/telegram")]
        public async Task<IActionResult> LoginByTelegram(TelegramAccountDto dto)
        {
            var token = await _authService.LoginByTelegram(dto);
            return Ok(new { token = token });
        }
    }
}
