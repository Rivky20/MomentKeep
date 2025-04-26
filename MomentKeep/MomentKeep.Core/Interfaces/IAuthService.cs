using MomentKeep.Core.DTOs.Auth;
using MomentKeep.Core.Models;

namespace MomentKeep.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<User> GetCurrentUserAsync(Guid userId);
        Task<bool> IsEmailAvailableAsync(string email);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    }
}