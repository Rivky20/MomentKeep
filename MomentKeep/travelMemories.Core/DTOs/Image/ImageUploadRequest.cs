using Microsoft.AspNetCore.Http;

public class ImageUploadRequest
{
    public IFormFile File { get; set; }
    public Guid? FolderId { get; set; } 
    public List<string> Tags { get; set; } = new List<string>();
}