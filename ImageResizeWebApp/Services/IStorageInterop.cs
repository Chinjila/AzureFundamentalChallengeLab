using ImageResizeWebApp.Models;

namespace ImageResizeWebApp.Services
{
    public interface IStorageInterop
    {
        bool IsImage(IFormFile file);
        Task<bool> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig);
        Task<List<string>> GetThumbNailUrls(AzureStorageConfig _storageConfig);
    }
}