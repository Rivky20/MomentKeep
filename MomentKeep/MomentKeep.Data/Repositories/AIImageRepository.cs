using Microsoft.EntityFrameworkCore;
using MomentKeep.Core.Interfaces.Repositories;
using MomentKeep.Core.Models;
using MomentKeep.Data.Context;

namespace MomentKeep.Data.Repositories
{
    public class AIImageRepository : Repository<Image>, IAIImageRepository
    {
        public AIImageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Image>> GetAIGeneratedImagesAsync(Guid userId)
        {
            return await _context.Images
                .Where(i => i.UserId == userId && i.IsAiGenerated)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetMonthlyGenerationCountAsync(Guid userId, DateTime monthStart)
        {
            var monthEnd = monthStart.AddMonths(1);

            return await _context.Images
                .CountAsync(i => i.UserId == userId &&
                                i.IsAiGenerated &&
                                i.CreatedAt >= monthStart &&
                                i.CreatedAt < monthEnd);
        }

        public async Task<Image> GetAIImageWithDetailsAsync(Guid imageId)
        {
            return await _context.Images
                .Include(i => i.Trip)
                .Include(i => i.User)
                .Include(i => i.ImageTags)
                .ThenInclude(it => it.Tag)
                .SingleOrDefaultAsync(i => i.Id == imageId && i.IsAiGenerated);
        }
    }
}