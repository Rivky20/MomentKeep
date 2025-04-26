using TravelMemories.Core.DTOs.Tag;

public class ImageResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string Url { get; set; }
    public DateTime? TakenAt { get; set; }
    public bool IsAiGenerated { get; set; }
    public Guid? FolderId { get; set; }  
    public List<TagResponse> Tags { get; set; } = new List<TagResponse>();
}