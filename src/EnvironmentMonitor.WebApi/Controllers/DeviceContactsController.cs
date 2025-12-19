using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User, Admin")]
    public class DeviceContactsController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DeviceContactsController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpPost]
        public async Task<DeviceContactDto> Create([FromBody] AddOrUpdateDeviceContactDto model)
        {
            return await _deviceService.AddDeviceContact(model);
        }

        [HttpPut]
        public async Task<DeviceContactDto> Update([FromBody] AddOrUpdateDeviceContactDto model)
        {
            return await _deviceService.UpdateDeviceContact(model);
        }

        [HttpDelete("{identifier}")]
        public async Task Delete(Guid identifier)
        {
            await _deviceService.DeleteDeviceContact(identifier);
        }
    }
}
