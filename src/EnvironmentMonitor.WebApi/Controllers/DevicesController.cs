using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Infrastructure.Data.Migrations.Application;
using EnvironmentMonitor.Infrastructure.Identity;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.WebApi.Attributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EnvironmentMonitor.Application.Services;
using EnvironmentMonitor.Domain.Models.Pagination;
using EnvironmentMonitor.Domain.Models.GetModels;
using EnvironmentMonitor.Domain.Models.AddModels;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User, Viewer, Admin")]
    public class DevicesController : ControllerBase
    {

        private readonly IDeviceService _deviceService;
        private readonly FileUploadSettings _fileUploadSettings;

        public DevicesController(IDeviceService deviceService, FileUploadSettings fileUploadSettings)
        {
            _deviceService = deviceService;
            _fileUploadSettings = fileUploadSettings;
        }

        [HttpPut("update")]
        public async Task<DeviceInfoDto> Update([FromBody] UpdateDeviceDto model) => await _deviceService.UpdateDevice(model);

        [HttpPost("reboot")]
        [Authorize(Roles = "Admin")]
        public async Task Reboot([FromBody] MessageDeviceModel model) => await _deviceService.Reboot(model.DeviceIdentifier);

        [HttpPost("motion-control-status")]
        [Authorize(Roles = "Admin")]
        public async Task SetMotionControlStatus([FromBody] SetMotionControlStatusMessage model) => await _deviceService.SetMotionControlStatus(model.DeviceIdentifier, (MotionControlStatus)model.Mode);

        [HttpPost("motion-control-delay")]
        [Authorize(Roles = "Admin")]
        public async Task SetMotionControlDelay([FromBody] SetMotionControlDelayMessag model) => await _deviceService.SetMotionControlDelay(model.DeviceIdentifier, model.DelayMs);

        [HttpPost("attachment")]
        [Authorize(Roles = "Admin")]
        public async Task<DeviceInfoDto> UploadAttachment([FromForm] Guid deviceId, [FromForm] bool isDeviceImage, [FromForm] string? fileName, IFormFile file, [FromForm] bool isSecret)
        {
            var maxFileSize = _fileUploadSettings.MaxImageUploadSizeMb * 1024 * 1024;

            if (!string.IsNullOrEmpty(fileName) && (fileName.Length > 100 || fileName.Length < 6))
            {
                throw new ArgumentException("Invalid filename length");
            }

            if (file == null || file.Length > maxFileSize)
            {
                throw new ArgumentException("File too large");
            }

            await _deviceService.AddAttachment(new UploadDeviceAttachmentModel()
            {
                DeviceIdentifier = deviceId,
                FileName = string.IsNullOrEmpty(fileName) ? file.FileName : fileName,
                Stream = file.OpenReadStream(),
                ContentType = file.ContentType,
                IsDeviceImage = isDeviceImage,
                IsSecret = isSecret
            });
            var deviceInfos = await _deviceService.GetDeviceInfos(false, [deviceId], true);
            return deviceInfos.First();
        }

        [HttpGet("{deviceId}/attachment/{identifier}")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAttachment([FromRoute] Guid deviceId, [FromRoute] Guid identifier)
        {
            var stream = await _deviceService.GetAttachment(deviceId, identifier);
            return stream == null ? NotFound() : new FileStreamResult(stream.Stream, stream.ContentType);
        }

        [HttpGet("{deviceId}/default-image")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, "image/jpeg")]
        [Authorize(Roles = "Admin")]
        public async Task<FileStreamResult?> GetDefaultImage([FromRoute] Guid deviceId)
        {
            var stream = await _deviceService.GetDefaultImage(deviceId);
            return stream == null ? null : new FileStreamResult(stream.Stream, stream.ContentType);
        }

        [HttpPost("default-image")]
        [Authorize(Roles = "Admin")]
        public async Task<DeviceInfoDto> SetDefaultImage([FromBody] SetDefaultImage model)
        {
            await _deviceService.SetDefaultImage(model.DeviceIdentifier, model.AttachmentGuid);
            var res = await _deviceService.GetDeviceInfos(false, [model.DeviceIdentifier], true);
            return res.FirstOrDefault();
        }

        [HttpDelete("{deviceId}/attachment/{attachmentIdentifier}")]
        [Authorize(Roles = "Admin")]
        public async Task<DeviceInfoDto> DeleteAttachment([FromRoute] Guid deviceId, [FromRoute] Guid attachmentIdentifier)
        {
            await _deviceService.DeleteAttachment(deviceId, attachmentIdentifier);
            var deviceInfos = await _deviceService.GetDeviceInfos(false, [deviceId], true);
            return deviceInfos.First();
        }

        [HttpGet]
        public async Task<List<DeviceDto>> GetDevices() => await _deviceService.GetDevices(true, true);

        [HttpGet(template: "info")]
        [Authorize(Roles = "Admin")]
        public async Task<List<DeviceInfoDto>> GetDeviceInfos() => await _deviceService.GetDeviceInfos(false, null, false, true);

        [HttpGet(template: "{identifier}/info")]
        [Authorize(Roles = "Admin")]
        public async Task<DeviceInfoDto> GetDeviceInfo(Guid identifier)
        {
            var result = await _deviceService.GetDeviceInfos(false, [identifier], true, true, true);
            return result.First();
        }

        [HttpGet(template: "{identifier}")]
        public async Task<DeviceDto> GetDevice([FromRoute] string identifier) => await _deviceService.GetDevice(identifier, AccessLevels.Read);

        [HttpGet(template: "{identifier}/events")]
        public async Task<List<DeviceEventDto>> GetDeviceEvents([FromRoute] Guid identifier) => await _deviceService.GetDeviceEvents(identifier);

        [HttpGet("sensors")]
        public async Task<List<SensorDto>> GetSensors([FromQuery] List<Guid> deviceIds) => await _deviceService.GetSensors(deviceIds);

        [HttpGet("status")]
        public async Task<DeviceStatusModel> GetDeviceStatus([FromQuery] GetDeviceStatusModel model) => await _deviceService.GetDeviceStatus(model);

        [HttpGet("device-messages")]
        [Authorize(Roles = "Viewer, Admin")]
        public async Task<PaginatedResult<DeviceMessageDto>> GetDeviceMessages([FromQuery] GetDeviceMessagesModel model) => await _deviceService.GetDeviceMessages(model);
    }
}
