using TravelMemories.Core.DTOs.Image;
using TravelMemories.Core.Interfaces.External;
using TravelMemories.Core.Interfaces.Repositories;
using TravelMemories.Core.Interfaces;
using TravelMemories.Core.Models;
using TravelMemories.Service.Helpers;

namespace TravelMemories.Service.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IFolderRepository _tripRepository;
        private readonly IUserService _userService;
        private readonly IS3Service _s3Service;

        public ImageService(
            IImageRepository imageRepository,
            ITagRepository tagRepository,
            IFolderRepository tripRepository,
            IUserService userService,
            IS3Service s3Service)
        {
            _imageRepository = imageRepository;
            _tagRepository = tagRepository;
            _tripRepository = tripRepository;
            _userService = userService;
            _s3Service = s3Service;
        }

        public async Task<IEnumerable<Image>> UploadImagesAsync(Guid userId, ImageUploadRequest request)
        {
            // Check if user has enough storage quota
            var totalSize = request.Files.Sum(f => f.Length);
            var sizeInMB = totalSize / (1024 * 1024);

            if (!await _userService.CheckStorageQuotaAsync(userId, (int)totalSize))
            {
                throw new InvalidOperationException("Storage quota exceeded");
            }

            // Check if trip exists and belongs to user if tripId is provided
            if (request.TripId.HasValue)
            {
                var trip = await _tripRepository.GetByIdAsync(request.TripId.Value);

                if (trip == null)
                {
                    throw new InvalidOperationException("Trip not found");
                }

                if (trip.UserId != userId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to add images to this trip");
                }
            }

            var uploadedImages = new List<Image>();

            foreach (var file in request.Files)
            {
                if (file.Length == 0)
                {
                    continue;
                }

                // Check file type
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!IsValidImageExtension(extension))
                {
                    throw new InvalidOperationException($"File type not supported: {extension}");
                }

                // Upload to S3
                var folderName = $"users/{userId}/images";
                var s3Path = await _s3Service.UploadFileAsync(file, folderName);

                // Create image entity
                var image = new Image
                {
                    FileName = file.FileName,
                    FilePath = s3Path,
                    FileSize = (int)file.Length,
                    MimeType = file.ContentType,
                    TripId = request.TripId,
                    UserId = userId,
                    TakenAt = ImageHelper.GetImageTakenDate(file),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _imageRepository.AddAsync(image);
                uploadedImages.Add(image);
            }

            await _imageRepository.SaveChangesAsync();

            // Process tags if provided
            if (request.Tags != null && request.Tags.Length > 0 && uploadedImages.Count > 0)
            {
                await ProcessTagsAsync(uploadedImages, request.Tags, userId);
            }

            return uploadedImages;
        }

        public async Task<Image> GetImageByIdAsync(Guid imageId, Guid userId)
        {
            var image = await _imageRepository.GetImageWithDetailsAsync(imageId);

            if (image == null)
            {
                return null;
            }

            // Check if user has access to this image
            if (image.UserId != userId &&
                (image.TripId == null ||
                (image.Trip != null && image.Trip.UserId != userId && image.Trip.ShareId == null)))
            {
                throw new UnauthorizedAccessException("You do not have permission to view this image");
            }

            return image;
        }

        public async Task<bool> DeleteImageAsync(Guid imageId, Guid userId)
        {
            var image = await _imageRepository.GetByIdAsync(imageId);

            if (image == null)
            {
                return false;
            }

            if (image.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this image");
            }

            // Delete from S3
            await _s3Service.DeleteFileAsync(image.FilePath);

            // Delete from database
            _imageRepository.Remove(image);
            await _imageRepository.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Image>> GetImagesByTripAsync(Guid tripId, Guid userId)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId);

            if (trip == null)
            {
                throw new InvalidOperationException("Trip not found");
            }

            if (trip.UserId != userId && trip.ShareId == null)
            {
                throw new UnauthorizedAccessException("You do not have permission to view images for this trip");
            }

            return await _imageRepository.GetImagesByTripAsync(tripId);
        }

        public async Task<IEnumerable<Image>> GetUserImagesAsync(Guid userId)
        {
            return await _imageRepository.GetImagesByUserAsync(userId);
        }

        public async Task<IEnumerable<Image>> SearchImagesAsync(Guid userId, ImageSearchRequest searchRequest)
        {
            string[] tags = null;

            if (searchRequest.Tags != null && searchRequest.Tags.Length > 0)
            {
                tags = searchRequest.Tags;
            }

            var images = await _imageRepository.SearchImagesAsync(
                searchRequest.TripId,
                userId,
                tags,
                searchRequest.DateFrom,
                searchRequest.DateTo,
                searchRequest.Location
            );

            // Apply pagination
            return images
                .Where(i => searchRequest.IsAiGenerated == null || i.IsAiGenerated == searchRequest.IsAiGenerated)
                .Skip((searchRequest.Page - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize);
        }

        public async Task UpdateImageDetailsAsync(Guid imageId, Guid userId, Image updatedImage)
        {
            var image = await _imageRepository.GetByIdAsync(imageId);

            if (image == null)
            {
                throw new KeyNotFoundException("Image not found");
            }

            if (image.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this image");
            }

            // Update only allowed properties
            image.TakenAt = updatedImage.TakenAt;
            image.TripId = updatedImage.TripId;
            image.UpdatedAt = DateTime.UtcNow;
            image.UpdatedBy = userId;

            _imageRepository.Update(image);
            await _imageRepository.SaveChangesAsync();
        }

        public async Task AddTagsToImageAsync(Guid imageId, Guid userId, string[] tags)
        {
            var image = await _imageRepository.GetByIdAsync(imageId);

            if (image == null)
            {
                throw new KeyNotFoundException("Image not found");
            }

            if (image.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this image");
            }

            await ProcessTagsAsync(new List<Image> { image }, tags, userId);
        }

        public async Task RemoveTagFromImageAsync(Guid imageId, Guid userId, Guid tagId)
        {
            var image = await _imageRepository.GetByIdAsync(imageId);

            if (image == null)
            {
                throw new KeyNotFoundException("Image not found");
            }

            if (image.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this image");
            }

            await _tagRepository.RemoveTagFromImageAsync(imageId, tagId);
        }

        public async Task<byte[]> DownloadImageAsync(Guid imageId, Guid userId)
        {
            var image = await _imageRepository.GetByIdAsync(imageId);

            if (image == null)
            {
                throw new KeyNotFoundException("Image not found");
            }

            // If userId is empty, it's an anonymous access via share link
            if (userId != Guid.Empty)
            {
                // Check if user has access to this image
                var trip = image.TripId.HasValue ? await _tripRepository.GetByIdAsync(image.TripId.Value) : null;

                if (image.UserId != userId && (trip == null || (trip.UserId != userId && trip.ShareId == null)))
                {
                    throw new UnauthorizedAccessException("You do not have permission to download this image");
                }
            }
            else
            {
                // Anonymous access - check if image is part of a shared trip
                if (!image.TripId.HasValue)
                {
                    throw new UnauthorizedAccessException("This image is not publicly accessible");
                }

                var trip = await _tripRepository.GetByIdAsync(image.TripId.Value);
                if (trip == null || trip.ShareId == null)
                {
                    throw new UnauthorizedAccessException("This image is not publicly accessible");
                }
            }

            return await _s3Service.DownloadFileAsync(image.FilePath);
        }

        #region Helper Methods

        private bool IsValidImageExtension(string extension)
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".tif" };
            return validExtensions.Contains(extension);
        }

        private async Task ProcessTagsAsync(List<Image> images, string[] tagNames, Guid userId)
        {
            // Get or create tags
            var existingTags = await _tagRepository.GetByNamesAsync(tagNames);
            var existingTagNames = existingTags.Select(t => t.Name.ToLower()).ToList();

            var newTags = new List<Tag>();
            foreach (var tagName in tagNames)
            {
                if (!existingTagNames.Contains(tagName.ToLower()))
                {
                    var tag = new Tag
                    {
                        Name = tagName.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    await _tagRepository.AddAsync(tag);
                    newTags.Add(tag);
                }
            }

            await _tagRepository.SaveChangesAsync();

            var allTags = existingTags.Concat(newTags).ToList();

            // Associate tags with images
            foreach (var image in images)
            {
                foreach (var tag in allTags)
                {
                    await _tagRepository.AddTagToImageAsync(image.Id, tag.Id, userId);
                }
            }
        }

        #endregion
    }
}