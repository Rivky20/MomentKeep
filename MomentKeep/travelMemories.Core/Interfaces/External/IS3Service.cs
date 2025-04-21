using Microsoft.AspNetCore.Http;

namespace TravelMemories.Core.Interfaces.External
{

    public interface IS3Service
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName, string? fileName = null);
        Task<byte[]> DownloadFileAsync(string fileKey);
        Task DeleteFileAsync(string fileKey);
        string GetFileUrl(string fileKey);
    }
}