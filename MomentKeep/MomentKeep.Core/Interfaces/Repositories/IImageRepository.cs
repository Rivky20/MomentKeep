using MomentKeep.Core.Models;

namespace MomentKeep.Core.Interfaces.Repositories
{
    public interface IImageRepository : IRepository<Image>
    {
        Task<Image> GetImageWithDetailsAsync(Guid imageId);
        Task<IEnumerable<Image>> GetImagesByTripAsync(Guid tripId);
        Task<IEnumerable<Image>> GetImagesByUserAsync(Guid userId);
        Task<IEnumerable<Image>> SearchImagesAsync(Guid? tripId, Guid? userId, string[] tags, DateTime? dateFrom, DateTime? dateTo, string location);
        Task<IEnumerable<Image>> GetAiGeneratedImagesAsync(Guid userId);
    }
}
