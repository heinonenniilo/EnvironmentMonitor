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
        private readonly KeyVaultSettings _settings;


        private const string EncodingTagKey = "Encoding";
        private const string Base64EncodingKey = "base64";

        public KeyVaultClient(KeyVaultSettings settings, ILogger<KeyVaultClient> logger)
        {
            _logger = logger;
            _settings = settings;

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

            // TODO not properly implemented, should support base64 encoding for strings as well.
            var secret = new KeyVaultSecret(secretName, secretValue);
            secret.Properties.Tags[EncodingTagKey] = "text";
            var response = await _secretClient.SetSecretAsync(secret);
            return response.Value.Name;
        }

        public async Task<string> StoreStreamAsSecretAsync(string secretName, Stream stream)
        {
            if (_secretClient == null)
            {
                throw new InvalidOperationException("KeyVault client not initialized");
            }

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            string secretValue;
            string encoding;
            if (_settings.Base64EncodeSecrets)
            {
                _logger.LogInformation($"Storing secret '{secretName}' as Base64 encoded");
                var base64String = Convert.ToBase64String(bytes);
                secretValue = base64String;
                encoding = Base64EncodingKey;
            }
            else
            {
                _logger.LogInformation($"Storing secret '{secretName}' as plain text");
                var textContent = Encoding.UTF8.GetString(bytes);
                // Validate that it's valid text
                if (ContainsInvalidTextCharacters(textContent))
                {
                    throw new InvalidOperationException("Stream contains binary data or invalid text characters. Enable Base64EncodeSecrets setting to store binary files.");
                }
                secretValue = textContent;
                encoding = "text";
            }

            var secret = new KeyVaultSecret(secretName, secretValue);
            secret.Properties.Tags[EncodingTagKey] = encoding;
            var response = await _secretClient.SetSecretAsync(secret);
            return response.Value.Name;
        }

        public async Task<AttachmentDownloadModel> GetSecretAsStreamAsync(string secretName)
        {
            if (_secretClient == null)
            {
                throw new InvalidOperationException("KeyVault client not initialized");
            }

            var secret = await _secretClient.GetSecretAsync(secretName);
            var secretValue = secret.Value.Value;

            byte[] bytes;


            var encoding = secret.Value.Properties.Tags.TryGetValue(EncodingTagKey, out var enc) ? enc : "text";
            if (encoding.Equals(Base64EncodingKey))
            {
                _logger.LogInformation($"Reading secret '{secretName}' as Base64 encoded");
                bytes = Convert.FromBase64String(secretValue);
            }
            else
            {
                _logger.LogInformation($"Reading secret '{secretName}' as plain text");
                bytes = Encoding.UTF8.GetBytes(secretValue);
            }

            var stream = new MemoryStream(bytes);

            return new AttachmentDownloadModel
            {
                Stream = stream,
                ContentType = "application/octet-stream", // TODO FIX
                SizeInBytes = bytes.Length
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
