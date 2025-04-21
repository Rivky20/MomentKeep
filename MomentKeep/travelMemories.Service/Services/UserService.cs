using Microsoft.AspNetCore.Identity;
using TravelMemories.Core.DTOs.User;
using TravelMemories.Core.Interfaces.Repositories;
using TravelMemories.Core.Interfaces;
using TravelMemories.Core.Models;

namespace TravelMemories.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<User> UpdateUserAsync(Guid id, UpdateUserDto userDto)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Check if email is changing and if it's already in use
            if (!string.IsNullOrEmpty(userDto.Email) &&
                userDto.Email.ToLower() != user.Email.ToLower() &&
                await _userRepository.EmailExistsAsync(userDto.Email))
            {
                throw new InvalidOperationException("Email is already in use");
            }

            // Update user properties
            if (!string.IsNullOrEmpty(userDto.FirstName))
            {
                user.FirstName = userDto.FirstName;
            }

            if (!string.IsNullOrEmpty(userDto.LastName))
            {
                user.LastName = userDto.LastName;
            }

            if (!string.IsNullOrEmpty(userDto.Email))
            {
                user.Email = userDto.Email;
            }

            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return user;
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Verify current password
            var passwordHasher = new PasswordHasher<User>();
            var verificationResult = passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                changePasswordDto.CurrentPassword
            );

            if (verificationResult != PasswordVerificationResult.Success)
            {
                return false;
            }

            // Update password
            user.PasswordHash = passwordHasher.HashPassword(user, changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            return await _userRepository.SaveChangesAsync();
        }

        public async Task<int> GetCurrentStorageUsageAsync(Guid userId)
        {
            return await _userRepository.GetTotalStorageUsedAsync(userId);
        }

        public async Task<int> GetCurrentAiQuotaUsageAsync(Guid userId)
        {
            var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            return await _userRepository.GetAiImageCountAsync(userId, currentMonth);
        }

        public async Task<bool> CheckStorageQuotaAsync(Guid userId, int fileSize)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var currentUsage = await _userRepository.GetTotalStorageUsedAsync(userId);
            var newFileSizeMB = fileSize / (1024 * 1024);

            return (currentUsage + newFileSizeMB) <= user.StorageQuota;
        }

        public async Task<bool> CheckAiQuotaAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var currentUsage = await _userRepository.GetAiImageCountAsync(userId, currentMonth);

            return currentUsage < user.AiQuota;
        }
    }
}