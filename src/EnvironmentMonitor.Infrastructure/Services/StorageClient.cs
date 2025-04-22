using Azure.Identity;
using Azure.Storage.Blobs;
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

        public async Task<Uri> Upload(Stream stream, string blobName)
        {
            if (_containerClient == null)
            {
                throw new InvalidOperationException("Blob client not initialized");
            }
            await _containerClient.CreateIfNotExistsAsync();
            if (string.IsNullOrEmpty(blobName))
            {
                throw new ArgumentException("Invalid blob name");
            }
            _logger.LogInformation($"Trying to upload blob named: {blobName}");
            var blobClient = _containerClient.GetBlobClient(blobName);
            var res = await blobClient.UploadAsync(stream, overwrite: true);
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
    }
}
