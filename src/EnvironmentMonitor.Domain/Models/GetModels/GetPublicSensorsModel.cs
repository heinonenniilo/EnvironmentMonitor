namespace EnvironmentMonitor.Domain.Models.GetModels
{
    public class GetPublicSensorsModel
    {
        public List<Guid>? Identifiers { get; set; }
        public bool? IsActive { get; set; }
    }
}
