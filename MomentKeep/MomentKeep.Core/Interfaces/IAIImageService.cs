using MomentKeep.Core.DTOs.AIImage;
using MomentKeep.Core.Models;

namespace MomentKeep.Core.Interfaces
{
    public interface IAIImageService
    {
        Task<Image> GenerateImageAsync(Guid userId, AIImageRequest request);
        Task<bool> CheckUserQuotaAsync(Guid userId);
        Task<int> GetUserAiGenerationCountAsync(Guid userId);
    }
}