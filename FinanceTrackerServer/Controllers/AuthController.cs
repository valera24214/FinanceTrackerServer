using FinanceTrackerServer.Models.DTO.Users;
using FinanceTrackerServer.Models.Entities;
using FinanceTrackerServer.Services.Interfaces;
using Microsoft.AspNetCore.Http;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto registerDto)
        {
            try 
            {
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email
                };

                var registeredUser = await _authService.Register(user, registerDto.Password);
                return Ok(new
                {
                    id = registeredUser.Id,
                    username = registeredUser.Username,
                    message = "Registration successful"
                });
            }
            catch(Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto loginDto)
        {
            try
            {
                var token = _authService.Login(loginDto.Email, loginDto.Password);
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
