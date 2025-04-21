using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TravelMemories.Core.Interfaces.External;
using TravelMemories.Service.Helpers;

namespace TravelMemories.Service.External
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;

        public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _configuration = configuration;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName, string fileName = null)
        {
            return await _s3Client.UploadFileAsync(_configuration, file, folderName, fileName);
        }

        public async Task<byte[]> DownloadFileAsync(string fileKey)
        {
            return await _s3Client.DownloadFileAsync(_configuration, fileKey);
        }

        public async Task DeleteFileAsync(string fileKey)
        {
            await _s3Client.DeleteFileAsync(_configuration, fileKey);
        }

        public string GetFileUrl(string fileKey)
        {
            return _configuration.GetFileUrl(fileKey);
        }
    }
}