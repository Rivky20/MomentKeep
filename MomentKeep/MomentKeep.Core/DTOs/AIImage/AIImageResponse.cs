namespace MomentKeep.Core.DTOs.AIImage
{
    public class AIImageResponse
    {
        public Guid ImageId { get; set; }
        public string Url { get; set; }
        public string Prompt { get; set; }
        public string Style { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? TripId { get; set; }
    }
}