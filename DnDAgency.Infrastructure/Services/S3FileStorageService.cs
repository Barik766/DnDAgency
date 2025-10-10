using Amazon.S3;
using Amazon.S3.Model;
using DnDAgency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DnDAgency.Infrastructure.Services
{
    public class S3FileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3FileStorageService(IAmazonS3 s3Client, string bucketName)
        {
            _s3Client = s3Client;
            _bucketName = bucketName;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var key = $"{folder}/{fileName}";

            using var stream = file.OpenReadStream();
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(putRequest);
            return key;
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileUrl
            };
            await _s3Client.DeleteObjectAsync(deleteRequest);
        }
    }
}