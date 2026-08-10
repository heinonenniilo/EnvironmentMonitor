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
        /// Adds Azure Key Vault as a configuration source when KeyVaultSettings:GetAppSettings is true.
        /// Locally defined configuration (environment variables, user secrets, command line) keeps
        /// priority over Key Vault, since those sources are re-added after the Key Vault provider.
        /// </summary>
        public static IConfigurationBuilder AddKeyVaultAppSettings(this IConfigurationManager configuration, string[]? args = null)
        {
            var settings = new KeyVaultSettings();
            configuration.GetSection("KeyVaultSettings").Bind(settings);

            if (!settings.GetAppSettings || string.IsNullOrEmpty(settings.VaultUri))
            {
                return configuration;
            }

            var vaultUri = new Uri(settings.VaultUri);

            SecretClient secretClient;
            if (!string.IsNullOrEmpty(settings.TenantId) &&
                !string.IsNullOrEmpty(settings.ClientId) &&
                !string.IsNullOrEmpty(settings.ClientSecret))
            {
                secretClient = new SecretClient(vaultUri,
                    new ClientSecretCredential(settings.TenantId, settings.ClientId, settings.ClientSecret));
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
