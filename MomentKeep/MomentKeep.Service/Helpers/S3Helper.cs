using Amazon;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MomentKeep.Service.Helpers
{
    public static class S3Helper
    {
        public static async Task<string> UploadFileAsync(
            this IAmazonS3 s3Client,
            IConfiguration configuration,
            IFormFile file,
            string folderName,
            string fileName = null)
        {
            var bucketName = configuration["AWS:S3:BucketName"];
            var accessKey = configuration["AWS:S3:AccessKey"];
            var secretKey = configuration["AWS:S3:SecretKey"];
            var region = configuration["AWS:Region"];

            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentException("S3 bucket name is not configured");
            }

            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentException("AWS credentials are not configured");
            }

            // יצירת client חדש עם האישורים במפורש
            var s3ClientWithCredentials = new AmazonS3Client(
                accessKey,
                secretKey,
                RegionEndpoint.GetBySystemName(region ?? "us-east-1")
            );

            // Generate a unique file name if not provided
            if (string.IsNullOrEmpty(fileName))
            {
                var fileExtension = Path.GetExtension(file.FileName);
                fileName = $"{Guid.NewGuid()}{fileExtension}";
            }

            // Construct the full key (path) in S3
            var key = string.IsNullOrEmpty(folderName)
                ? fileName
                : $"{folderName}/{fileName}";

            using (var fileStream = file.OpenReadStream())
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = fileStream,
                    BucketName = bucketName,
                    Key = key,
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.Private
                };

                var transferUtility = new TransferUtility(s3ClientWithCredentials);
                await transferUtility.UploadAsync(uploadRequest);
            }

            return key;
        }

        public static async Task<byte[]> DownloadFileAsync(
            this IAmazonS3 s3Client,
            IConfiguration configuration,
            string fileKey)
        {
            var bucketName = configuration["AWS:S3:BucketName"];
            var accessKey = configuration["AWS:S3:AccessKey"];
            var secretKey = configuration["AWS:S3:SecretKey"];
            var region = configuration["AWS:Region"];

            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentException("S3 bucket name is not configured");
            }

            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentException("AWS credentials are not configured");
            }

            // יצירת client חדש עם האישורים במפורש
            var s3ClientWithCredentials = new AmazonS3Client(
                accessKey,
                secretKey,
                RegionEndpoint.GetBySystemName(region ?? "us-east-1")
            );

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileKey
            };

            using (var response = await s3ClientWithCredentials.GetObjectAsync(request))
            using (var responseStream = response.ResponseStream)
            using (var memoryStream = new MemoryStream())
            {
                await responseStream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static async Task DeleteFileAsync(
            this IAmazonS3 s3Client,
            IConfiguration configuration,
            string fileKey)
        {
            var bucketName = configuration["AWS:S3:BucketName"];
            var accessKey = configuration["AWS:S3:AccessKey"];
            var secretKey = configuration["AWS:S3:SecretKey"];
            var region = configuration["AWS:Region"];

            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentException("S3 bucket name is not configured");
            }

            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentException("AWS credentials are not configured");
            }

            // יצירת client חדש עם האישורים במפורש
            var s3ClientWithCredentials = new AmazonS3Client(
                accessKey,
                secretKey,
                RegionEndpoint.GetBySystemName(region ?? "us-east-1")
            );

            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = fileKey
            };

            await s3ClientWithCredentials.DeleteObjectAsync(request);
        }

        public static string GetFileUrl(
            this IConfiguration configuration,
            string fileKey)
        {
            var bucketName = configuration["AWS:S3:BucketName"];
            var region = configuration["AWS:Region"];

            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(region))
            {
                throw new ArgumentException("S3 configuration is incomplete");
            }

            // Generate a URL that follows the pattern: https://{bucket}.s3.{region}.amazonaws.com/{key}
            return $"https://{bucketName}.s3.{region}.amazonaws.com/{fileKey}";
        }
    }
}