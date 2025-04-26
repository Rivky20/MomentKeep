using Microsoft.AspNetCore.Mvc;
using MomentKeep.Core.DTOs;
using MomentKeep.Core.DTOs.Auth;
using MomentKeep.Core.Interfaces;

namespace MomentKeep.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message, "ValidationError"));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (InvalidOperationException)
            {
                return BadRequest(ErrorDto.FromException("Invalid email or password", "AuthenticationError"));
            }
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> CheckEmailAvailable([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(ErrorDto.FromException("Email is required", "ValidationError"));
            }

            var isAvailable = await _authService.IsEmailAvailableAsync(email);
            return Ok(isAvailable);
        }
    }
}