using MomentKeep.Core.DTOs.Image;

public class FolderDetailResponse : FolderResponse
{
    public List<ImageResponse> Images { get; set; } = new List<ImageResponse>();
}