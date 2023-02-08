// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.IO;
using Azure.Storage.Blobs.Specialized;
using Azure.Messaging.EventGrid.SystemEvents;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using Azure.Storage.Blobs;
using System.Threading.Tasks;

namespace SmartCropvc
{
    public static class Function1
    {
        [FunctionName("ResizeImages")]
        public static async Task Run([EventGridTrigger]EventGridEvent eventGridEvent,
            ILogger log)
        {
            var cs_key = System.Environment.GetEnvironmentVariable("cognitive_service_key");
            var service_uri = System.Environment.GetEnvironmentVariable("cognitive_service_endpoint") +
                "vision/v3.2/generateThumbnail?width=600&height=600&smartCropping=true&model-version=latest";
            var BLOB_STORAGE_CONNECTION_STRING = System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var thumbContainerName = System.Environment.GetEnvironmentVariable("thumbnailContainerName");
            var createdEvent = eventGridEvent.Data.ToObjectFromJson<StorageBlobCreatedEventData>();


            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", cs_key);
            var body = new { url = createdEvent.Url };
            HttpResponseMessage response = client.PostAsJsonAsync(service_uri, body).Result;
            log.LogInformation($"request sent to {service_uri}");

            var blobServiceClient = new BlobServiceClient(BLOB_STORAGE_CONNECTION_STRING);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(thumbContainerName);
           
            var blobName = GetBlobNameFromUrl(createdEvent.Url);
            log.LogInformation($"blobName {blobName}");
            await blobContainerClient.UploadBlobAsync(blobName, await response.Content.ReadAsStreamAsync());
            log.LogInformation($"{blobName} created in {blobContainerClient.Uri}");



            log.LogInformation($"request sent to {service_uri}");
        }

        private static string GetBlobNameFromUrl(string bloblUrl)
        {
            var uri = new Uri(bloblUrl);
            var blobClient = new BlobClient(uri);
            return blobClient.Name;
        }
    }
}
