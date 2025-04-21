using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TravelMemories.Core.DTOs.Image
{
    public class ImageUploadRequest
    {
        [Required]
        public IFormFile[] Files { get; set; }

        public Guid? TripId { get; set; }

        public string[] Tags { get; set; }
    }
}