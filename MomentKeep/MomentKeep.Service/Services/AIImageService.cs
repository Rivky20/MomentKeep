using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MomentKeep.Core.DTOs.AIImage;
using MomentKeep.Core.Interfaces.External;
using MomentKeep.Core.Interfaces.Repositories;
using MomentKeep.Core.Interfaces;
using MomentKeep.Core.Models;

namespace MomentKeep.Service.Services
{
    public class AIImageService : IAIImageService
    {
        private readonly IHuggingFaceClient _huggingFaceClient;
        private readonly IS3Service _s3Service;
        private readonly IUserRepository _userRepository;
        private readonly IAIImageRepository _aiImageRepository;
        private readonly IConfiguration _configuration;

        public AIImageService(
            IHuggingFaceClient huggingFaceClient,
            IS3Service s3Service,
            IUserRepository userRepository,
            IAIImageRepository aiImageRepository,
            IConfiguration configuration)
        {
            _huggingFaceClient = huggingFaceClient;
            _s3Service = s3Service;
            _userRepository = userRepository;
            _aiImageRepository = aiImageRepository;
            _configuration = configuration;
        }

        public async Task<Image> GenerateImageAsync(Guid userId, AIImageRequest request)
        {
            // Check if user has quota available
            if (!await CheckUserQuotaAsync(userId))
            {
                throw new InvalidOperationException("AI image generation quota exceeded");
            }

            // Generate image using Hugging Face API
            var imageBytes = await _huggingFaceClient.GenerateImageAsync(
                request.Prompt,
                request.Style,
                request.Size
            );

            // Create filename and determine content type
            var fileName = $"{Guid.NewGuid()}.png";
            var contentType = "image/png";

            // Upload the image to S3
            var folderName = $"users/{userId}/ai-images";

            // Convert byte array to IFormFile for uploading to S3
            using (var memoryStream = new MemoryStream(imageBytes))
            {
                var formFile = new FormFile(
                    baseStream: memoryStream,
                    baseStreamOffset: 0,
                    length: imageBytes.Length,
                    name: "file",
                    fileName: fileName
                );

                // Upload to S3
                var filePath = await _s3Service.UploadFileAsync(formFile, folderName, fileName);

                // Create image entity
                var image = new Image
                {
                    FileName = fileName,
                    FilePath = filePath,
                    FileSize = imageBytes.Length,
                    MimeType = contentType,
                    TripId = request.TripId,
                    IsAiGenerated = true,
                    AiPrompt = request.Prompt,
                    AiStyle = request.Style,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                // Save to database
                await _aiImageRepository.AddAsync(image);
                await _aiImageRepository.SaveChangesAsync();

                return image;
            }
        }

        public async Task<bool> CheckUserQuotaAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var count = await _userRepository.GetAiImageCountAsync(userId, currentMonth);

            return count < user.AiQuota;
        }

        public async Task<int> GetUserAiGenerationCountAsync(Guid userId)
        {
            var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            return await _userRepository.GetAiImageCountAsync(userId, currentMonth);
        }
    }
}