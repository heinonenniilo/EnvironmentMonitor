using Azure.Identity;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Services
{
    internal class HubMessageService : IHubMessageService
    {
        private readonly IotHubSettings _iotHubSettings;
        private readonly ILogger<HubMessageService> _logger;
        public HubMessageService(IotHubSettings iotHubSettings, ILogger<HubMessageService> logger)
        {
            _iotHubSettings = iotHubSettings;
            _logger = logger;
        }
        public async Task SendMessageToDevice(string deviceIdentifier, string message)
        {
            // Use ClientSecretCredential if all required properties are provided
            Azure.Core.TokenCredential credential;
            if (!string.IsNullOrEmpty(_iotHubSettings.TenantId) && 
                !string.IsNullOrEmpty(_iotHubSettings.ClientId) && 
                !string.IsNullOrEmpty(_iotHubSettings.ClientSecret))
            {
                _logger.LogInformation("Using ClientSecretCredential for IoT Hub authentication");
                credential = new ClientSecretCredential(
                    _iotHubSettings.TenantId, 
                    _iotHubSettings.ClientId, 
                    _iotHubSettings.ClientSecret);
            }
            else
            {
                _logger.LogInformation("Using DefaultAzureCredential for IoT Hub authentication");
                credential = new DefaultAzureCredential();
            }

            var serviceClient = ServiceClient.Create(_iotHubSettings.IotHubDomain, credential);
            _logger.LogInformation($"Trying to create service client with domain: '{_iotHubSettings.IotHubDomain}'");
            var mes = new Message(Encoding.UTF8.GetBytes(message));
            mes.Properties.Add("Priority", "High");
            await serviceClient.SendAsync(deviceIdentifier, mes);
        }
    }
}