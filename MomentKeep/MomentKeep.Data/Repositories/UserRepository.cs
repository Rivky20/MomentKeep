using Microsoft.EntityFrameworkCore;
using MomentKeep.Core.Interfaces.Repositories;
using MomentKeep.Core.Models;
using MomentKeep.Data.Context;

namespace MomentKeep.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .SingleOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User> GetWithDetailsByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Trips)
                .Include(u => u.Images)
                .SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task<int> GetTotalStorageUsedAsync(Guid userId)
        {
            return await _context.Images
                .Where(i => i.UserId == userId)
                .SumAsync(i => i.FileSize) / (1024 * 1024); // Convert to MB
        }

        public async Task<int> GetAiImageCountAsync(Guid userId, DateTime monthStart)
        {
            DateTime utcMonthStart = DateTime.SpecifyKind(monthStart, DateTimeKind.Utc);
            var monthEnd = utcMonthStart.AddMonths(1);

            return await _context.Images
                .CountAsync(i => i.UserId == userId &&
                             i.IsAiGenerated &&
                             i.CreatedAt >= utcMonthStart &&
                             i.CreatedAt < monthEnd);
        }
    }
}