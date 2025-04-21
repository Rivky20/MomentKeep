using TravelMemories.Core.DTOs.Image;
using TravelMemories.Core.Models;

namespace TravelMemories.Core.Interfaces
{
    public interface IImageService
    {
        Task<IEnumerable<Image>> UploadImagesAsync(Guid userId, ImageUploadRequest request);
        Task<Image> GetImageByIdAsync(Guid imageId, Guid userId);
        Task<bool> DeleteImageAsync(Guid imageId, Guid userId);
        Task<IEnumerable<Image>> GetImagesByTripAsync(Guid tripId, Guid userId);
        Task<IEnumerable<Image>> GetUserImagesAsync(Guid userId);
        Task<IEnumerable<Image>> SearchImagesAsync(Guid userId, ImageSearchRequest searchRequest);
        Task UpdateImageDetailsAsync(Guid imageId, Guid userId, Image updatedImage);
        Task AddTagsToImageAsync(Guid imageId, Guid userId, string[] tags);
        Task RemoveTagFromImageAsync(Guid imageId, Guid userId, Guid tagId);
        Task<byte[]> DownloadImageAsync(Guid imageId, Guid userId);
    }
}