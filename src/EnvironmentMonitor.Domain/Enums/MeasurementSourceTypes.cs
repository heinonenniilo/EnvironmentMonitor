using System.ComponentModel;

namespace EnvironmentMonitor.Domain.Enums
{
    public enum MeasurementSourceTypes
    {
        [Description("IoT Hub")]
        IotHub = 0,
        [Description("Rest interface")]
        Rest = 1,
        [Description("Ilmatieteenlaitos Open Data")]
        Ilmatieteenlaitos = 2
    }
}
