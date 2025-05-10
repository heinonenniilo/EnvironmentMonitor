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
        public async Task<DeviceInfoDto> UploadAttachment([FromForm] string deviceId, IFormFile file)
        {
            var maxFileSize = _fileUploadSettings.MaxImageUploadSizeMb * 1024 * 1024;
            if (file == null || file.Length > maxFileSize)
            {
                throw new ArgumentException("File too large");
            }
            await _deviceService.AddAttachment(deviceId, new UploadAttachmentModel()
            {
                FileName = file.FileName,
                Stream = file.OpenReadStream(),
                ContentType = file.ContentType
            });
            var deviceInfos = await _deviceService.GetDeviceInfos(false, [deviceId], true);
            return deviceInfos.First();
        }

        [HttpGet("attachment/{deviceId}/{identifier}")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, "image/jpeg")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAttachment([FromRoute] string deviceId, [FromRoute] Guid identifier)
        {
            var stream = await _deviceService.GetAttachment(deviceId, identifier);
            return stream == null ? NotFound() : new FileStreamResult(stream.Stream, stream.ContentType);
        }

        [HttpGet("default-image/{deviceId}")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, "image/jpeg")]
        [Authorize(Roles = "Admin")]
        public async Task<FileStreamResult?> GetDefaultImage([FromRoute] string deviceId)
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

        [HttpDelete("attachment/{deviceId}/{attachmentIdentifier}")]
        [Authorize(Roles = "Admin")]
        public async Task<DeviceInfoDto> DeleteAttachment([FromRoute] string deviceId, [FromRoute] Guid attachmentIdentifier)
        {
            await _deviceService.DeleteAttachment(deviceId, attachmentIdentifier);
            var deviceInfos = await _deviceService.GetDeviceInfos(false, [deviceId], true);
            return deviceInfos.First();
        }

        [HttpGet]
        public async Task<List<DeviceDto>> GetDevices()
        {
            var result = await _deviceService.GetDevices();
            return result;
        }

        [HttpGet(template: "info")]
        [Authorize(Roles = "Admin")]
        public async Task<List<DeviceInfoDto>> GetDeviceInfos()
        {
            var result = await _deviceService.GetDeviceInfos(false, null); // Also the ones marked as non-visible
            return result;
        }

        [HttpGet(template: "info/{identifier}")]
        [Authorize(Roles = "Admin")]
        public async Task<DeviceInfoDto> GetDeviceInfo(string identifier)
        {
            var result = await _deviceService.GetDeviceInfos(false, [identifier], true); // Also the ones marked as non-visible
            return result.First();
        }

        [HttpGet(template: "{identifier}")]
        public async Task<DeviceDto> GetDevice([FromRoute] string identifier)
        {
            return await _deviceService.GetDevice(identifier, AccessLevels.Read);
        }

        [HttpGet(template: "events/{identifier}")]
        public async Task<List<DeviceEventDto>> GetDeviceEvents([FromRoute] string identifier)
        {
            return await _deviceService.GetDeviceEvents(identifier);
        }

        [HttpGet("sensors")]
        public async Task<List<SensorDto>> GetSensors([FromQuery] List<string> deviceIds)
        {
            var result = await _deviceService.GetSensors(deviceIds);
            return result;
        }

        [HttpGet("status")]
        public async Task<DeviceStatusModel> GetDeviceStatus([FromQuery] GetDeviceStatusModel model)
        {
            return await _deviceService.GetDeviceStatus(model);
        }
    }
}
