using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace EnvironmentMonitor.WebApi.Services
{
    public class ApiExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ApiExceptionHandler> _logger;
        public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "WebApi exception occured");
            var problemDetails = new ProblemDetails
            {
                Title = "An error occurred",
                Status = StatusCodes.Status500InternalServerError,
                Detail = ""
            };
            switch (exception)
            {
                case UnauthorizedAccessException:
                case UnauthorizedException:
                    problemDetails.Title = "Not allowed";
                    problemDetails.Status = StatusCodes.Status404NotFound;
                    break;
                case ArgumentException argEx:
                    problemDetails.Title = "Bad request";
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Detail = argEx.Message;
                    break;
                default:
                    problemDetails.Title = "Error occured";
                    problemDetails.Status = StatusCodes.Status500InternalServerError;
                    break;
            }
            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
