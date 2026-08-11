using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace EnvironmentMonitor.Infrastructure.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds Azure Key Vault as a configuration source when DataProtectionKeysSettings:GetAppSettings is true.
        /// No-op if the configuration instance does not support adding sources.
        /// </summary>
        public static IConfiguration AddKeyVaultAppSettings(this IConfiguration configuration, string[]? args = null)
        {
            if (configuration is IConfigurationManager manager)
            {
                manager.AddKeyVaultAppSettings(args);
            }

            return configuration;
        }

        /// <summary>
        /// Adds Azure Key Vault as a configuration source when DataProtectionKeysSettings:GetAppSettings is true.
        /// Locally defined configuration (environment variables, user secrets, command line) keeps
        /// priority over Key Vault, since those sources are re-added after the Key Vault provider.
        /// </summary>
        public static IConfigurationBuilder AddKeyVaultAppSettings(this IConfigurationManager configuration, string[]? args = null)
        {
            var dataProtectionKeysSettings = new DataProtectionKeysSettings();
            configuration.GetSection("DataProtectionKeysSettings").Bind(dataProtectionKeysSettings);

            if (!dataProtectionKeysSettings.GetAppSettings ||
                string.IsNullOrEmpty(dataProtectionKeysSettings.KeyVaultKeyIdentifier))
            {
                return configuration;
            }

            var uriString = dataProtectionKeysSettings.KeyVaulUri;
            Uri? vaultUri;
            if (string.IsNullOrEmpty(uriString))
            {
                // KeyVaultKeyIdentifier points to a key, e.g. https://my-vault.vault.azure.net/keys/my-key/version.
                if (!Uri.TryCreate(dataProtectionKeysSettings.KeyVaultKeyIdentifier, UriKind.Absolute, out var keyIdentifierUri))
                {
                    return configuration;
                }
                vaultUri = new Uri(keyIdentifierUri.GetLeftPart(UriPartial.Authority));
            }
            else
            {
                vaultUri = new Uri(uriString);
            }

            SecretClient secretClient;
            if (!string.IsNullOrEmpty(dataProtectionKeysSettings.TenantId) &&
                !string.IsNullOrEmpty(dataProtectionKeysSettings.ClientId) &&
                !string.IsNullOrEmpty(dataProtectionKeysSettings.ClientSecret))
            {
                secretClient = new SecretClient(vaultUri,
                    new ClientSecretCredential(dataProtectionKeysSettings.TenantId, dataProtectionKeysSettings.ClientId, dataProtectionKeysSettings.ClientSecret));
            }
            else
            {
                secretClient = new SecretClient(vaultUri, new DefaultAzureCredential());
            }

            configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());

            // Re-add local sources so that they override values coming from Key Vault.
            configuration.AddEnvironmentVariables();
            if (args != null && args.Length > 0)
            {
                configuration.AddCommandLine(args);
            }

            return configuration;
        }
    }
}
