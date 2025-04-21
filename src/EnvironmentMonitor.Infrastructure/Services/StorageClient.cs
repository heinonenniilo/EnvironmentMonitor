using Azure.Identity;
using Azure.Storage.Blobs;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
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
        private readonly BlobContainerClient _containerClient;
        private readonly StorageAccountSettings _settings;
        public StorageClient(StorageAccountSettings settings)
        {
            var credential = new DefaultAzureCredential(); // uses system-assigned managed identity
            _settings = settings;

            _containerClient = !string.IsNullOrEmpty(_settings.ConnectionString) ?
                new BlobContainerClient(_settings.ConnectionString, _settings.ContainerName,
                new BlobClientOptions()) : new BlobContainerClient(new Uri(_settings.AccountUri), new DefaultAzureCredential());
            _containerClient.CreateIfNotExists();
        }

        public async Task<Uri> Upload(Stream stream, string blobName)
        {
            if (string.IsNullOrEmpty(blobName))
            {
                throw new ArgumentException("Invalid blob name");
            }
            var blobClient = _containerClient.GetBlobClient(blobName);
            var res = await blobClient.UploadAsync(stream, overwrite: true);
            return blobClient.Uri;
        }

        public async Task<AttachmentInfoModel> GetImageAsync(string fileName)
        {
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
