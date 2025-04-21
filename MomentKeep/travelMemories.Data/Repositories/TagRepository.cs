using Microsoft.EntityFrameworkCore;
using TravelMemories.Core.Interfaces.Repositories;
using TravelMemories.Core.Models;
using TravelMemories.Data.Context;

namespace TravelMemories.Data.Repositories
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Tag> GetByNameAsync(string name)
        {
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Tag>> GetByNamesAsync(IEnumerable<string> names)
        {
            var normalizedNames = names.Select(n => n.ToLower());

            return await _context.Tags
                .Where(t => normalizedNames.Contains(t.Name.ToLower()))
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetTagsByImageIdAsync(Guid imageId)
        {
            return await _context.ImageTags
                .Where(it => it.ImageId == imageId)
                .Select(it => it.Tag)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int limit = 20)
        {
            return await _context.Tags
                .OrderByDescending(t => t.ImageTags.Count)
                .Take(limit)
                .ToListAsync();
        }

        public async Task AddTagToImageAsync(Guid imageId, Guid tagId, Guid userId)
        {
            var exists = await _context.ImageTags
                .AnyAsync(it => it.ImageId == imageId && it.TagId == tagId);

            if (!exists)
            {
                var imageTag = new ImageTag
                {
                    ImageId = imageId,
                    TagId = tagId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _context.ImageTags.AddAsync(imageTag);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveTagFromImageAsync(Guid imageId, Guid tagId)
        {
            var imageTag = await _context.ImageTags
                .FirstOrDefaultAsync(it => it.ImageId == imageId && it.TagId == tagId);

            if (imageTag != null)
            {
                _context.ImageTags.Remove(imageTag);
                await _context.SaveChangesAsync();
            }
        }
    }
}