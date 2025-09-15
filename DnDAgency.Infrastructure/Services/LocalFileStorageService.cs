using DnDAgency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DnDAgency.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _webRootPath;

        public LocalFileStorageService(string? webRootPath, string contentRootPath)
        {
            _webRootPath = webRootPath ?? Path.Combine(contentRootPath, "wwwroot");
            Directory.CreateDirectory(_webRootPath);
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var uploads = Path.Combine(_webRootPath, folder);
            Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(uploads, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            // Возвращаем относительный путь без ведущего слэша
            return $"{folder}/{fileName}";
        }

        public Task DeleteFileAsync(string fileUrl)
        {
            var path = Path.Combine(_webRootPath, fileUrl.TrimStart('/'));
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }
    }
}
