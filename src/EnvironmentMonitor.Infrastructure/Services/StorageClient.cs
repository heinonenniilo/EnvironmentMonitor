using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class StorageClient : IStorageClient
    {
        private readonly BlobContainerClient? _containerClient;
        private readonly StorageAccountSettings _settings;
        private readonly ILogger<StorageClient> _logger;
        public StorageClient(StorageAccountSettings settings, ILogger<StorageClient> logger)
        {
            var credential = new DefaultAzureCredential();
            _logger = logger;
            _settings = settings;

            if (!string.IsNullOrEmpty(_settings.ConnectionString))
            {
                _containerClient = new BlobContainerClient(_settings.ConnectionString, _settings.ContainerName,
                new BlobClientOptions());
            }
            else if (!string.IsNullOrEmpty(_settings.AccountUri))
            {
                _logger.LogInformation($"Trying to form blob container client with uri: {_settings.AccountUri}. ");
                _containerClient = new BlobContainerClient(new Uri(_settings.AccountUri), new DefaultAzureCredential());
            }
            else if (!string.IsNullOrEmpty(_settings.AccountName) && !string.IsNullOrEmpty(_settings.ContainerName))
            {
                var uriString = $"https://{_settings.AccountName}.blob.core.windows.net/{_settings.ContainerName}";
                _logger.LogInformation($"Trying to form blob container client with uri: {uriString}. ");
                _containerClient = new BlobContainerClient(new Uri(uriString), new DefaultAzureCredential());
            }
            else
            {
                _logger.LogWarning("No setting defined for storage client");
            }
        }

        public async Task<Uri> Upload(UploadAttachmentModel model)
        {
            if (_containerClient == null)
            {
                throw new InvalidOperationException("Blob client not initialized");
            }
            await _containerClient.CreateIfNotExistsAsync();
            if (string.IsNullOrEmpty(model.FileName))
            {
                throw new ArgumentException("Invalid blob name");
            }
            _logger.LogInformation($"Trying to upload blob named: {model.FileName}");
            var blobClient = _containerClient.GetBlobClient(model.FileName);
            var res = await blobClient.UploadAsync(model.Stream, new BlobUploadOptions()
            {
                HttpHeaders = new BlobHttpHeaders() { ContentType = model.ContentType }
            });

            return blobClient.Uri;
        }

        public async Task<AttachmentInfoModel> GetImageAsync(string fileName)
        {
            if (_containerClient == null)
            {
                throw new InvalidOperationException("No container client initialized");
            }
            var blobClient = _containerClient.GetBlobClient(fileName);
            var exists = await blobClient.ExistsAsync();
            if (!exists)
            {
                throw new FileNotFoundException("Image not found");
            }
            var response = await blobClient.DownloadAsync();
            return new AttachmentInfoModel()
            {
                Stream = response.Value.Content,
                ContentType = response.Value.ContentType,
            };
        }

        public async Task<bool> DeleteBlob(string fileName)
        {
            if (_containerClient == null)
            {
                throw new InvalidOperationException("Client not initialized");
            }
            var blobClient = _containerClient.GetBlobClient(fileName);
            _logger.LogInformation($"Removing blob named '{fileName}'");
            var result = await blobClient.DeleteIfExistsAsync();
            _logger.LogInformation($"Remove result: {result.Value}");
            return result.Value;
        }
    }
}
