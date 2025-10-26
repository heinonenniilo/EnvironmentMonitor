using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    public class KeyVaultClient : IKeyVaultClient
    {
        private readonly SecretClient? _secretClient;
        private readonly ILogger<KeyVaultClient> _logger;

        public KeyVaultClient(KeyVaultSettings settings, ILogger<KeyVaultClient> logger)
        {
            _logger = logger;

            if (!string.IsNullOrEmpty(settings.VaultUri))
            {
                var keyVaultUrl = new Uri(settings.VaultUri);
                _secretClient = new SecretClient(keyVaultUrl, new DefaultAzureCredential());
            }
            else
            {
                _logger.LogWarning("KeyVault URL not configured");
            }
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            if (_secretClient == null)
            {
                throw new InvalidOperationException("KeyVault client not initialized");
            }

            var secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value.Value;
        }

        public async Task<string> StoreSecretAsync(string secretName, string secretValue)
        {
            if (_secretClient == null)
            {
                throw new InvalidOperationException("KeyVault client not initialized");
            }

            var secret = await _secretClient.SetSecretAsync(secretName, secretValue);
            return secret.Value.Name;
        }

        public async Task<string> StoreStreamAsSecretAsync(string secretName, Stream stream)
        {
            if (_secretClient == null)
            {
                throw new InvalidOperationException("KeyVault client not initialized");
            }

            // Convert stream to Base64 string (works for any binary data)
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();
            var base64String = Convert.ToBase64String(bytes);

            var secret = await _secretClient.SetSecretAsync(secretName, base64String);
            return secret.Value.Name;
        }

        public async Task<AttachmentDownloadModel> GetSecretAsStreamAsync(string secretName)
        {
            if (_secretClient == null)
            {
                throw new InvalidOperationException("KeyVault client not initialized");
            }

            var secret = await _secretClient.GetSecretAsync(secretName);
            var bytes = Convert.FromBase64String(secret.Value.Value);
            var stream = new MemoryStream(bytes);

            return new AttachmentDownloadModel
            {
                Stream = stream,
                ContentType = "application/octet-stream", // To be fixed
            };
        }

        public async Task<string> StoreTextFileAsSecretAsync(string secretName, Stream stream, Encoding? encoding = null)
        {
            if (_secretClient == null)
            {
                throw new InvalidOperationException("KeyVault client not initialized");
            }

            encoding ??= Encoding.UTF8;

            // Read stream as text
            using var reader = new StreamReader(stream, encoding);
            var textContent = await reader.ReadToEndAsync();

            // Validate that it's valid text (optional - check for control characters except newlines/tabs)
            if (ContainsInvalidTextCharacters(textContent))
            {
                throw new InvalidOperationException("Stream contains binary data or invalid text characters");
            }

            var secret = await _secretClient.SetSecretAsync(secretName, textContent);
            return secret.Value.Name;
        }

        public async Task<bool> DeleteSecretAsync(string secretName)
        {
            if (_secretClient == null)
            {
                throw new InvalidOperationException("KeyVault client not initialized");
            }

            _logger.LogInformation($"Deleting secret: {secretName}");
            var operation = await _secretClient.StartDeleteSecretAsync(secretName);
            await operation.WaitForCompletionAsync();
            return operation.HasCompleted;
        }

        private bool ContainsInvalidTextCharacters(string content)
        {
            foreach (char c in content)
            {
                // Allow printable characters, newlines, carriage returns, and tabs
                if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
                {
                    return true;
                }
            }
            return false;
        }
    }
}
