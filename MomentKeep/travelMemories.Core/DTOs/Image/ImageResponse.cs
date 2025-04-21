using TravelMemories.Core.DTOs.Tag;

namespace TravelMemories.Core.DTOs.Image
{
    public class ImageResponse
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int FileSize { get; set; }
        public string MimeType { get; set; }
        public string FileUrl { get; set; }
        public DateTime? TakenAt { get; set; }
        public Guid? TripId { get; set; }
        public string TripName { get; set; }
        public bool IsAiGenerated { get; set; }
        public string AiPrompt { get; set; }
        public string AiStyle { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<TagResponse> Tags { get; set; }
    }
}