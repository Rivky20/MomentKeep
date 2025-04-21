using TravelMemories.Core.DTOs.Auth;
using TravelMemories.Core.Models;

namespace TravelMemories.Core.Interfaces
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