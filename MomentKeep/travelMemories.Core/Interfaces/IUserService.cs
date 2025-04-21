using TravelMemories.Core.DTOs.User;
using TravelMemories.Core.Models;

namespace TravelMemories.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(Guid id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> UpdateUserAsync(Guid id, UpdateUserDto userDto);
        Task DeleteUserAsync(Guid id);
        Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto changePasswordDto);
        Task<int> GetCurrentStorageUsageAsync(Guid userId);
        Task<int> GetCurrentAiQuotaUsageAsync(Guid userId);
        Task<bool> CheckStorageQuotaAsync(Guid userId, int fileSize);
        Task<bool> CheckAiQuotaAsync(Guid userId);
    }
}