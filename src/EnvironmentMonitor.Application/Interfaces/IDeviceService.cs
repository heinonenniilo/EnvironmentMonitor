using EnvironmentMonitor.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IDeviceService
    {
        public Task Reboot(string deviceIdentifier);
        public Task<DeviceDto> GetDevice(string deviceIdentifier);
        public Task<List<DeviceDto>> GetDevices();
        public Task<List<SensorDto>> GetSensors(List<string> DeviceIdentifier);
        public Task<List<SensorDto>> GetSensors(List<int> DeviceIds);
    }
}
