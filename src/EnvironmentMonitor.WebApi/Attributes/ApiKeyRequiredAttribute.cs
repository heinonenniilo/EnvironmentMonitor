namespace EnvironmentMonitor.WebApi.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyRequiredAttribute : Attribute
    {
    }
}
