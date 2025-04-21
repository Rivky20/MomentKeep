using TravelMemories.Core.Models;

namespace TravelMemories.Core.Interfaces.Repositories
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag> GetByNameAsync(string name);
        Task<IEnumerable<Tag>> GetByNamesAsync(IEnumerable<string> names);
        Task<IEnumerable<Tag>> GetTagsByImageIdAsync(Guid imageId);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int limit = 20);
        Task AddTagToImageAsync(Guid imageId, Guid tagId, Guid userId);
        Task RemoveTagFromImageAsync(Guid imageId, Guid tagId);
    }
}