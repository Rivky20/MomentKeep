namespace TravelMemories.Core.DTOs.Image
{
    public class ImageSearchRequest
    {
        public Guid? TripId { get; set; }
        public string[] Tags { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Location { get; set; }
        public bool? IsAiGenerated { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}