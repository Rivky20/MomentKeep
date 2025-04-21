using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelMemories.Core.DTOs;
using TravelMemories.Core.DTOs.Image;
using TravelMemories.Core.DTOs.Tag;
using TravelMemories.Core.Interfaces;
using TravelMemories.Core.Models;

namespace TravelMemories.Controllers
{
    [ApiController]
    [Route("api/images")]
    [Authorize]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageResponse>>> GetUserImages()
        {
            var userId = GetUserId();
            var images = await _imageService.GetUserImagesAsync(userId);

            return Ok(images.Select(MapToImageResponse));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ImageResponse>> GetImage(Guid id)
        {
            var userId = GetUserId();

            try
            {
                var image = await _imageService.GetImageByIdAsync(id, userId);

                if (image == null)
                {
                    return NotFound(ErrorDto.NotFound("Image not found"));
                }

                return Ok(MapToImageResponse(image));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("{id}/content")]
        [AllowAnonymous]
        public async Task<ActionResult> GetImageContent(Guid id)
        {
            Guid? userId = null;

            if (User.Identity.IsAuthenticated)
            {
                userId = GetUserId();
            }

            try
            {
                var imageBytes = await _imageService.DownloadImageAsync(id, userId ?? Guid.Empty);

                // Get image details to determine content type
                var image = await _imageService.GetImageByIdAsync(id, userId ?? Guid.Empty);

                return File(imageBytes, image.MimeType);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ErrorDto.NotFound("Image not found"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("trip/{tripId}")]
        public async Task<ActionResult<IEnumerable<ImageResponse>>> GetTripImages(Guid tripId)
        {
            var userId = GetUserId();

            try
            {
                var images = await _imageService.GetImagesByTripAsync(tripId, userId);
                return Ok(images.Select(MapToImageResponse));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost("upload")]
        public async Task<ActionResult<IEnumerable<ImageResponse>>> UploadImages([FromForm] ImageUploadRequest request)
        {
            if (request.Files == null || request.Files.Length == 0)
            {
                return BadRequest(ErrorDto.ValidationError("No files were uploaded"));
            }

            var userId = GetUserId();

            try
            {
                var images = await _imageService.UploadImagesAsync(userId, request);
                return Ok(images.Select(MapToImageResponse));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<ImageSearchResponse>> SearchImages([FromBody] ImageSearchRequest request)
        {
            var userId = GetUserId();

            try
            {
                var images = await _imageService.SearchImagesAsync(userId, request);

                var response = new ImageSearchResponse
                {
                    Images = images.Select(MapToImageResponse),
                    TotalCount = images.Count(),
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(images.Count() / (double)request.PageSize)
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteImage(Guid id)
        {
            var userId = GetUserId();

            try
            {
                var result = await _imageService.DeleteImageAsync(id, userId);

                if (!result)
                {
                    return NotFound(ErrorDto.NotFound("Image not found"));
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost("{id}/tags")]
        public async Task<ActionResult> AddTags(Guid id, [FromBody] string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                return BadRequest(ErrorDto.ValidationError("No tags provided"));
            }

            var userId = GetUserId();

            try
            {
                await _imageService.AddTagsToImageAsync(id, userId, tags);
                return Ok(new { message = "Tags added successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ErrorDto.NotFound("Image not found"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpDelete("{id}/tags/{tagId}")]
        public async Task<ActionResult> RemoveTag(Guid id, Guid tagId)
        {
            var userId = GetUserId();

            try
            {
                await _imageService.RemoveTagFromImageAsync(id, userId, tagId);
                return Ok(new { message = "Tag removed successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ErrorDto.NotFound("Image or tag not found"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        #region Helper Methods

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        }

        private ImageResponse MapToImageResponse(Image image)
        {
            return new ImageResponse
            {
                Id = image.Id,
                FileName = image.FileName,
                FilePath = image.FilePath,
                FileSize = image.FileSize,
                MimeType = image.MimeType,
                FileUrl = $"/api/images/{image.Id}/content",
                TakenAt = image.TakenAt,
                TripId = image.TripId,
                TripName = image.Trip?.Name,
                IsAiGenerated = image.IsAiGenerated,
                AiPrompt = image.AiPrompt,
                AiStyle = image.AiStyle,
                UserId = image.UserId,
                CreatedAt = image.CreatedAt,
                Tags = image.ImageTags?.Select(it => new TagResponse
                {
                    Id = it.TagId,
                    Name = it.Tag?.Name
                }).ToList()
            };
        }

        #endregion
    }
}