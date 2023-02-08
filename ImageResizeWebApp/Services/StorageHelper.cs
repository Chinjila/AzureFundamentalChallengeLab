using Azure.Storage.Blobs;
using ImageResizeWebApp.Models;

namespace ImageResizeWebApp.Services
{
    public class StorageHelper : IStorageInterop
    {
        private static BlobServiceClient _blobServiceClient;

        public bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<string>> GetThumbNailUrls(AzureStorageConfig _storageConfig)
        {

            //configure the static blob storage account
            ConfigureBlobServiceClient(_storageConfig);

            if (_blobServiceClient is null || string.IsNullOrWhiteSpace(_blobServiceClient.AccountName))
            {
                throw new Exception("Invalid Storage Account information");
            }

            List<string> thumbnailUrls = new List<string>();

            //get the container
            var container = _blobServiceClient.GetBlobContainerClient(_storageConfig.ThumbnailContainer);

            //iterate and get all the blobs, then add their url to the list of thumbnails
            foreach (var blob in container.GetBlobs())
            {
                //get the blob details
                var blobClient = container.GetBlobClient(blob.Name);
                //add each blob url to the list

                thumbnailUrls.Add($"{_storageConfig.CDNEndpoint}{_storageConfig.ThumbnailContainer}/{blob.Name}");
            }

            return await Task.FromResult(thumbnailUrls);
        }

        public async Task<bool> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig)
        {
            //configure the static blob storage account
            ConfigureBlobServiceClient(_storageConfig);

            if (_blobServiceClient is null || string.IsNullOrWhiteSpace(_blobServiceClient.AccountName))
            {
                throw new Exception("Invalid Storage Account information");
            }

            //get the container
            var container = _blobServiceClient.GetBlobContainerClient(_storageConfig.ImageContainer);

            //get a blob client to reference the blob for upload:
            var blobClient = container.GetBlobClient(fileName);

            //upload the file to storage
            blobClient.Upload(fileStream, true);

            return await Task.FromResult(true);
        }

        private static string GetConnectionString(AzureStorageConfig _config)
        {
            return $"DefaultEndpointsProtocol=https;AccountName={_config.AccountName};AccountKey={_config.AccountKey};EndpointSuffix=core.windows.net";
        }
        private static void ConfigureBlobServiceClient(AzureStorageConfig _storageConfig)
        {
            var connectionString = GetConnectionString(_storageConfig);
            _blobServiceClient = new BlobServiceClient(connectionString);
        }
    }
}
