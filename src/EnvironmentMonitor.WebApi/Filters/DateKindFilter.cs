using Microsoft.AspNetCore.Mvc.Filters;

namespace EnvironmentMonitor.WebApi.Filters
{
    public sealed class DateKindFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null)
                    continue;

                SetUnspecified(argument, "From");
                SetUnspecified(argument, "To");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        private static void SetUnspecified(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);

            if (property == null || !property.CanRead || !property.CanWrite)
                return;

            if (property.GetValue(obj) is DateTime dateTime)
            {
                property.SetValue(obj,
                    DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified));
            }
        }
    }
}
