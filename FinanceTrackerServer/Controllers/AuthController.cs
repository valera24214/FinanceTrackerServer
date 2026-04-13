using FinanceTrackerServer.Models.DTO.AuthAccounts;
using FinanceTrackerServer.Models.Entities;
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

        [HttpGet("register/password/send_email_code")]
        public async Task<IActionResult> SendEmailCode([FromQuery] string email)
        {
            try
            {
                await _authService.SendEmailVerificationCode(email);
                return Ok(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("register/password/verify_email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string verification_code)
        {
            try
            {
                var regToken = await _authService.VerifyEmail(verification_code);
                return Ok(new { registration_token = regToken });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("register/password/set_password")]
        public async Task<IActionResult> SetPassword([FromQuery] string regToken,[FromBody] string password)
        {
            try
            {
                await _authService.SetPassword(regToken, password);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login/password")]
        public async Task<IActionResult> LoginByPassword([FromBody] PasswordAccountDto dto)
        {
            try
            {
                var result = await _authService.LoginByPassword(dto);
                if (result.jwtToken == null)
                    return Unauthorized("Invalid credentials");

                Response.Cookies.Append("refreshToken", result.refreshToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddDays(14),
                        Domain = "localhost"
                    }
                );

                return Ok(new { token = result.jwtToken });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login/refresh")]
        public async Task<IActionResult> RefreshTokens()
        {
            try 
            {
                var refreshToken = Request.Cookies["refreshToken"];
                var result = await _authService.RefreshTokens(refreshToken);

                if (result.jwtToken == null)
                    return Unauthorized("Invalid credentials");

                Response.Cookies.Delete("refreshToken");
                Response.Cookies.Append("refreshToken", result.refreshToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddDays(14),
                        Domain = "localhost",

                    }
                );

                return Ok(new { token = result.jwtToken });
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
