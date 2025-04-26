using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomentKeep.Core.DTOs;
using MomentKeep.Core.DTOs.User;
using MomentKeep.Core.Interfaces;
using MomentKeep.Core.Models;

namespace MomentKeep.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = GetUserId();
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(ErrorDto.NotFound("User not found"));
            }

            return Ok(MapToUserDto(user));
        }

        [HttpPut("me")]
        public async Task<ActionResult<UserDto>> UpdateCurrentUser([FromBody] UpdateUserDto updateUserDto)
        {
            var userId = GetUserId();

            try
            {
                var user = await _userService.UpdateUserAsync(userId, updateUserDto);
                return Ok(MapToUserDto(user));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message, "ValidationError"));
            }
        }

        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorDto.ValidationError("Invalid input data"));
            }

            var success = await _userService.ChangePasswordAsync(userId, changePasswordDto);

            if (!success)
            {
                return BadRequest(ErrorDto.FromException("Current password is incorrect", "ValidationError"));
            }

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpGet("quota")]
        public async Task<ActionResult<UserQuotaDto>> GetUserQuota()
        {
            var userId = GetUserId();
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(ErrorDto.NotFound("User not found"));
            }

            var storageUsed = await _userService.GetCurrentStorageUsageAsync(userId);
            var aiQuotaUsed = await _userService.GetCurrentAiQuotaUsageAsync(userId);

            var quotaDto = new UserQuotaDto
            {
                StorageQuotaMB = user.StorageQuota,
                StorageUsedMB = storageUsed,
                StorageRemainingMB = user.StorageQuota - storageUsed,
                AiQuotaTotal = user.AiQuota,
                AiQuotaUsed = aiQuotaUsed,
                AiQuotaRemaining = user.AiQuota - aiQuotaUsed
            };

            return Ok(quotaDto);
        }

        #region Helper Methods

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                StorageQuota = user.StorageQuota,
                AiQuota = user.AiQuota,
                CreatedAt = user.CreatedAt
            };
        }

        #endregion
    }
}