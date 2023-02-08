using ImageResizeWebApp.Models;
using ImageResizeWebApp.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ImageResizeWebApp.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        private readonly AzureStorageConfig _storageConfig = null;
        private readonly IStorageInterop _storageHelper;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<ImagesController> _logger;
        private const string MSG_NO_FILES = "No files received from the upload";
        private const string MSG_BAD_STORAGE_DETAILS = "Sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there";
        private const string MSG_BAD_STORAGE_CONTAINER = "Please provide a name for your image container in the azure blob storage";
        private const string MSG_UNSUPPORTED_MEDIA = "Invalid Media Type.  Please use a correct and valid image file";
        private const string MSG_IMAGE_NOT_SAVED = "Looks like the image couldnt upload to the storage";

        public ImagesController(IOptions<AzureStorageConfig> config, ILogger<ImagesController> logger, IStorageInterop storageHelper, TelemetryClient telemetryClient)
        {
            _storageConfig = config.Value;
            _logger = logger;
            _storageHelper = storageHelper;
            _telemetryClient = telemetryClient;
        }

        // POST /api/images/upload
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            bool isUploaded = false;

            try
            {
                if (files.Count == 0)
                {
                    _logger.LogInformation(MSG_NO_FILES);
                    return BadRequest(MSG_NO_FILES);
                }

                var validatedMsg = ValidateSettings();
                if (!string.IsNullOrWhiteSpace(validatedMsg))
                {
                    return BadRequest(validatedMsg);
                }

                foreach (var formFile in files)
                {
                    if (_storageHelper.IsImage(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            using (Stream stream = formFile.OpenReadStream())
                            {
                                isUploaded = await _storageHelper.UploadFileToStorage(stream, formFile.FileName, _storageConfig);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation(MSG_UNSUPPORTED_MEDIA);
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (isUploaded)
                {
                    if (string.IsNullOrWhiteSpace(_storageConfig.ThumbnailContainer))
                    {
                        return new AcceptedResult();
                    }

                    return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
                }
                else
                {
                    _logger.Log(LogLevel.Error, MSG_IMAGE_NOT_SAVED);
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }
        }

        // GET /api/images/thumbnails
        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails()
        {
            var validatedMsg = ValidateSettings();
            if (!string.IsNullOrWhiteSpace(validatedMsg))
            {
                return BadRequest(validatedMsg);
            }

            var thumbnailUrls = new List<string>();
            try
            {
                thumbnailUrls = await _storageHelper.GetThumbNailUrls(_storageConfig);
                var info = new Dictionary<string, string>();
                info.Add("Sample", thumbnailUrls[0]);
                _telemetryClient.TrackEvent("Thumbnails Returned", info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest(ex.Message);
            }

            return new ObjectResult(thumbnailUrls);
        }

        private string ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_storageConfig.AccountKey) || string.IsNullOrWhiteSpace(_storageConfig.AccountName))
            {
                _logger.LogInformation(MSG_BAD_STORAGE_DETAILS);
                return MSG_BAD_STORAGE_DETAILS;
            }
            if (string.IsNullOrWhiteSpace(_storageConfig.ImageContainer))
            {
                _logger.LogInformation(MSG_BAD_STORAGE_CONTAINER);
                return MSG_BAD_STORAGE_CONTAINER;
            }
            return string.Empty;
        }
    }
}
