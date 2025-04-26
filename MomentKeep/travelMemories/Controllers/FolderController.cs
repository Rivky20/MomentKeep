using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelMemories.Core.DTOs;
using TravelMemories.Core.DTOs.Image;
using TravelMemories.Core.DTOs.Tag;
using TravelMemories.Core.DTOs.Trip;
using TravelMemories.Core.Interfaces;
using TravelMemories.Core.Models;

namespace TravelMemories.Controllers
{
    [Route("api/folders")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        private readonly IFolderService _folderService;

        public FolderController(IFolderService folderService)
        {
            _folderService = folderService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<FolderResponse>>> GetFolders()
        {
            var userId = Guid.Parse(User.Identity.Name);
            var folders = await _folderService.GetAllFoldersAsync(userId);
            return Ok(folders);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<FolderDetailResponse>> GetFolder(Guid id)
        {
            var userId = Guid.Parse(User.Identity.Name);
            try
            {
                var folder = await _folderService.GetFolderByIdAsync(id, userId);
                return Ok(folder);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    


        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<FolderDetailResponse>> GetTrip(Guid id)
        {
            var userId = GetUserId();

            try
            {
                var trip = await _tripService.GetTripWithImagesAsync(id, userId);

                if (trip == null)
                {
                    return NotFound(ErrorDto.NotFound("Trip not found"));
                }

                return Ok(MapToTripDetailResponse(trip));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
        }

        [HttpGet("shared/{shareId}")]
        public async Task<ActionResult<FolderDetailResponse>> GetSharedTrip(Guid shareId)
        {
            try
            {
                var trip = await _folderService.GetTripByShareIdAsync(shareId);

                if (trip == null)
                {
                    return NotFound(ErrorDto.NotFound("Trip not found"));
                }

                return Ok(MapToTripDetailResponse(trip));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<FolderResponse>> CreateTrip([FromBody] FolderRequest tripRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorDto.ValidationError("Invalid trip data"));
            }

            var userId = GetUserId();

            try
            {
                var trip = await _folderService.CreateTripAsync(userId, tripRequest);
                return CreatedAtAction(nameof(GetTrip), new { id = trip.Id }, MapToTripResponse(trip));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<FolderResponse>> UpdateTrip(Guid id, [FromBody] FolderRequest tripRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorDto.ValidationError("Invalid trip data"));
            }

            var userId = GetUserId();

            try
            {
                var trip = await _folderService.UpdateTripAsync(id, userId, tripRequest);
                return Ok(MapToTripResponse(trip));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ErrorDto.NotFound("Trip not found"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteTrip(Guid id)
        {
            var userId = GetUserId();

            try
            {
                var result = await _folderService.DeleteTripAsync(id, userId);

                if (!result)
                {
                    return NotFound(ErrorDto.NotFound("Trip not found"));
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost("{id}/regenerate-share")]
        [Authorize]
        public async Task<ActionResult<Guid>> RegenerateShareId(Guid id)
        {
            var userId = GetUserId();

            try
            {
                var newShareId = await _folderService.RegenerateShareIdAsync(id, userId);
                return Ok(new { shareId = newShareId });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ErrorDto.NotFound("Trip not found"));
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

        private FolderResponse MapToTripResponse(Folder trip)
        {
            return new FolderResponse
            {
                Id = trip.Id,
                Name = trip.Name,
                Description = trip.Description,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                UserId = trip.UserId,
                ShareId = trip.ShareId,
                ShareUrl = trip.ShareId.HasValue ? $"/trips/shared/{trip.ShareId}" : null,
                LocationName = trip.LocationName,
                Latitude = trip.Latitude,
                Longitude = trip.Longitude,
                CreatedAt = trip.CreatedAt,
                UpdatedAt = trip.UpdatedAt,
                ImageCount = trip.Images?.Count ?? 0
            };
        }

        private FolderDetailResponse MapToTripDetailResponse(Folder trip)
        {
            var response = new FolderDetailResponse
            {
                Id = trip.Id,
                Name = trip.Name,
                Description = trip.Description,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                UserId = trip.UserId,
                ShareId = trip.ShareId,
                ShareUrl = trip.ShareId.HasValue ? $"/trips/shared/{trip.ShareId}" : null,
                LocationName = trip.LocationName,
                Latitude = trip.Latitude,
                Longitude = trip.Longitude,
                CreatedAt = trip.CreatedAt,
                UpdatedAt = trip.UpdatedAt,
                ImageCount = trip.Images?.Count ?? 0
            };

            if (trip.Images != null)
            {
                response.Images = trip.Images.Select(i => new ImageResponse
                {
                    Id = i.Id,
                    FileName = i.FileName,
                    FilePath = i.FilePath,
                    FileSize = i.FileSize,
                    MimeType = i.MimeType,
                    FileUrl = $"/api/images/{i.Id}/content",
                    TakenAt = i.TakenAt,
                    TripId = i.TripId,
                    TripName = trip.Name,
                    IsAiGenerated = i.IsAiGenerated,
                    AiPrompt = i.AiPrompt,
                    AiStyle = i.AiStyle,
                    UserId = i.UserId,
                    CreatedAt = i.CreatedAt,
                    Tags = i.ImageTags?.Select(it => new TagResponse
                    {
                        Id = it.TagId,
                        Name = it.Tag?.Name
                    }).ToList()
                });
            }

            return response;
        }

        #endregion
    }
}