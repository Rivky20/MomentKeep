using MomentKeep.Core.Models;

namespace MomentKeep.Core.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User> GetWithDetailsByIdAsync(Guid id);
        Task<int> GetTotalStorageUsedAsync(Guid userId);
        Task<int> GetAiImageCountAsync(Guid userId, DateTime monthStart);
    }
}