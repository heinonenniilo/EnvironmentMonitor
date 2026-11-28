namespace EnvironmentMonitor.Application.DTOs
{
    public class RegisterMqttDeviceDto
    {
        public string Password { get; set; } = "";
        public string PasswordConfirm { get; set; } = "";
    }
}
