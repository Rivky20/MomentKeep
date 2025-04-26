using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomentKeep.Core.DTOs;
using MomentKeep.Core.DTOs.AIImage;
using MomentKeep.Core.Interfaces;

namespace MomentKeep.Controllers
{
    [ApiController]
    [Route("api/ai-images")]
    [Authorize]
    public class AIImageController : ControllerBase
    {
        private readonly IAIImageService _aiImageService;
        private readonly IUserService _userService;

        public AIImageController(IAIImageService aiImageService, IUserService userService)
        {
            _aiImageService = aiImageService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<AIImageResponse>> GenerateImage([FromBody] AIImageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorDto.ValidationError("Invalid request data"));
            }

            var userId = GetUserId();

            try
            {
                // Check if user has quota available
                var hasQuota = await _aiImageService.CheckUserQuotaAsync(userId);

                if (!hasQuota)
                {
                    return BadRequest(ErrorDto.FromException("AI image generation quota exceeded"));
                }

                var image = await _aiImageService.GenerateImageAsync(userId, request);

                return Ok(new AIImageResponse
                {
                    ImageId = image.Id,
                    Url = $"/api/images/{image.Id}/content",
                    Prompt = image.AiPrompt,
                    Style = image.AiStyle,
                    CreatedAt = image.CreatedAt,
                    TripId = image.TripId
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
        }

        [HttpGet("quota")]
        public async Task<ActionResult<AIQuotaResponse>> GetQuota()
        {
            var userId = GetUserId();

            try
            {
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(ErrorDto.NotFound("User not found"));
                }

                var usedCount = await _aiImageService.GetUserAiGenerationCountAsync(userId);
                var remaining = Math.Max(0, user.AiQuota - usedCount);

                // Calculate reset date (first day of next month)
                var now = DateTime.UtcNow;
                var resetDate = new DateTime(now.Year, now.Month, 1).AddMonths(1);

                return Ok(new AIQuotaResponse
                {
                    Total = user.AiQuota,
                    Used = usedCount,
                    Remaining = remaining,
                    ResetDate = resetDate
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ErrorDto.FromException(ex.Message));
            }
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        }
    }
}