using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.GetModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace EnvironmentMonitor.Application.Services
{
    /// <summary>
    /// Manages device clients and roles via the Mosquitto dynamic security plugin, using the admin MQTT connection.
    /// </summary>
    public partial class MqttDeviceProvisioningService : IMqttDeviceProvisioningService
    {
        private const int AclPriority = 5;
        private static readonly TimeSpan ResponseTimeout = TimeSpan.FromSeconds(15);

        // Shared across instances: the admin connection is a singleton, so provisioning sequences are serialized.
        private static readonly SemaphoreSlim AdminGate = new(1, 1);

        private readonly IMqttConnection? _adminConnection;
        private readonly MqttSettings? _settings;
        private readonly ILogger<MqttDeviceProvisioningService> _logger;
        private readonly IUserService _userService;
        private readonly IDeviceRepository _deviceRepository;

        public MqttDeviceProvisioningService(
            ILogger<MqttDeviceProvisioningService> logger,
            IUserService userService,
            IDeviceRepository deviceRepository,
            [FromKeyedServices(MqttSettings.AdminConnectionKey)] IMqttConnection? adminConnection = null,
            MqttSettings? settings = null)
        {
            _logger = logger;
            _userService = userService;
            _deviceRepository = deviceRepository;
            _adminConnection = adminConnection;
            _settings = settings;
        }

        public async Task RegisterDevice(Guid identifier, string password, CancellationToken cancellationToken = default)
        {
            EnsureAdmin();
            var deviceIdentifier = await GetMqttDeviceIdentifier(identifier);
            ValidateDeviceIdentifier(deviceIdentifier);
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password is required", nameof(password));
            }
            var settings = GetSettings();
            var role = settings.GetDeviceRoleName(deviceIdentifier);
            var measurementsTopic = settings.GetMeasurementsTopic(deviceIdentifier);
            var commandsTopic = settings.GetCommandsTopic(deviceIdentifier);

            if (await Exists(new JsonObject { ["command"] = "getClient", ["username"] = deviceIdentifier }, cancellationToken)
                || await Exists(new JsonObject { ["command"] = "getRole", ["rolename"] = role }, cancellationToken))
            {
                _logger.LogWarning($"MQTT client or role already exists for device '{deviceIdentifier}'");
                throw new DuplicateEntityException($"MQTT device '{deviceIdentifier}' already exists. Unregister it first.");
            }

            _logger.LogInformation($"Registering MQTT device '{deviceIdentifier}' with role '{role}'");
            await ExecuteCommands(
            [
                new JsonObject { ["command"] = "createClient", ["username"] = deviceIdentifier, ["password"] = password },
                new JsonObject { ["command"] = "createRole", ["rolename"] = role },
                CreateAcl(role, "publishClientSend", measurementsTopic),
                CreateAcl(role, "subscribeLiteral", commandsTopic),
                CreateAcl(role, "publishClientReceive", commandsTopic),
                new JsonObject { ["command"] = "addClientRole", ["username"] = deviceIdentifier, ["rolename"] = role, ["priority"] = AclPriority },
            ], cancellationToken);
            _logger.LogInformation($"MQTT device '{deviceIdentifier}' registered");
        }

        public async Task UnregisterDevice(Guid identifier, CancellationToken cancellationToken = default)
        {
            EnsureAdmin();
            var deviceIdentifier = await GetMqttDeviceIdentifier(identifier);
            ValidateDeviceIdentifier(deviceIdentifier);
            var role = GetSettings().GetDeviceRoleName(deviceIdentifier);

            _logger.LogInformation($"Unregistering MQTT device '{deviceIdentifier}' and role '{role}'");
            await ExecuteCommands(
            [
                new JsonObject { ["command"] = "deleteClient", ["username"] = deviceIdentifier },
                new JsonObject { ["command"] = "deleteRole", ["rolename"] = role },
            ], cancellationToken);
            _logger.LogInformation($"MQTT device '{deviceIdentifier}' unregistered");
        }

        private async Task<string> GetMqttDeviceIdentifier(Guid identifier)
        {
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault()
                ?? throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
            return device.DeviceIdentifier;
        }

        private async Task ExecuteCommands(List<JsonObject> commands, CancellationToken cancellationToken)
        {
            var connection = _adminConnection ?? throw new InvalidOperationException("MQTT admin connection is not configured");

            await AdminGate.WaitAsync(cancellationToken);
            try
            {
                await connection.EnsureConnectedAsync(cancellationToken);

                // Commands are executed one by one, stopping at the first failure.
                foreach (var command in commands)
                {
                    var result = await ExecuteCommand(connection, command, cancellationToken);
                    var error = result["error"]?.GetValue<string>();
                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new InvalidOperationException($"MQTT command '{command["command"]}' failed: {error}");
                    }
                }
            }
            finally
            {
                AdminGate.Release();
            }
        }

        private async Task<bool> Exists(JsonObject getCommand, CancellationToken cancellationToken)
        {
            var connection = _adminConnection ?? throw new InvalidOperationException("MQTT admin connection is not configured");

            await AdminGate.WaitAsync(cancellationToken);
            try
            {
                await connection.EnsureConnectedAsync(cancellationToken);
                var result = await ExecuteCommand(connection, getCommand, cancellationToken);
                return string.IsNullOrEmpty(result["error"]?.GetValue<string>());
            }
            finally
            {
                AdminGate.Release();
            }
        }

        private static async Task<JsonNode> ExecuteCommand(IMqttConnection connection, JsonObject command, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid().ToString();
            var response = new TaskCompletionSource<JsonNode>(TaskCreationOptions.RunContinuationsAsynchronously);
            command["correlationData"] = correlationId;

            Task HandleResponse(string topic, string payload)
            {
                var match = JsonNode.Parse(payload)?["responses"]?.AsArray()
                    .FirstOrDefault(r => r?["correlationData"]?.GetValue<string>() == correlationId);
                if (match != null)
                {
                    response.TrySetResult(match);
                }
                return Task.CompletedTask;
            }

            await using var subscription = await connection.Subscribe(MqttSettings.DynamicSecurityResponseTopic, HandleResponse, cancellationToken);
            var payload = new JsonObject { ["commands"] = new JsonArray(command) }.ToJsonString();
            await connection.Publish(MqttSettings.DynamicSecurityControlTopic, payload, cancellationToken);
            return await response.Task.WaitAsync(ResponseTimeout, cancellationToken);
        }

        private static JsonObject CreateAcl(string role, string aclType, string topic) => new()
        {
            ["command"] = "addRoleACL",
            ["rolename"] = role,
            ["acltype"] = aclType,
            ["topic"] = topic,
            ["allow"] = true,
            ["priority"] = AclPriority,
        };

        private void EnsureAdmin()
        {
            if (!_userService.IsAdmin)
            {
                _logger.LogWarning("Unauthorized attempt to manage MQTT devices");
                throw new UnauthorizedAccessException();
            }
        }

        private MqttSettings GetSettings() => _settings ?? throw new InvalidOperationException("MQTT is not configured");

        private static void ValidateDeviceIdentifier(string deviceIdentifier)
        {
            if (string.IsNullOrEmpty(deviceIdentifier) || !DeviceIdentifierRegex().IsMatch(deviceIdentifier))
            {
                throw new ArgumentException("Device identifier may contain only letters, numbers, _ and -", nameof(deviceIdentifier));
            }
        }

        [GeneratedRegex("^[A-Za-z0-9_-]+$")]
        private static partial Regex DeviceIdentifierRegex();
    }
}
