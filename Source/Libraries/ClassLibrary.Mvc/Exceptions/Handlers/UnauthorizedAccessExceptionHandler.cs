﻿using ClassLibrary.Mvc.Extensions;
using ClassLibrary.Mvc.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ClassLibrary.Mvc.Exceptions.Handlers
{
    public sealed class UnauthorizedAccessExceptionHandler(ILogger<UnauthorizedAccessExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<UnauthorizedAccessExceptionHandler> _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, System.Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not UnauthorizedAccessException unauthorizedAccessException)
                return false;

            _logger.LogWarning(exception, "{@logMessageHeader} Exception Occurred: {@message}", httpContext.Request.LogMessageHeader(), exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized Access",
                Detail = unauthorizedAccessException.Message.Trim()
            };

            if (string.IsNullOrEmpty(problemDetails.Detail))
            {
                KeyValuePair<int, string> kvp = StatusCodeDefinitions.GetCodeDefinition(problemDetails.Status.Value);
                problemDetails.Detail = kvp.Value.Trim();
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            if (httpContext.Request.IsAjaxRequest())
                await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken).ConfigureAwait(false);
            else
            {
                httpContext.Response.Redirect($"/Error/{problemDetails.Status.Value}");
                await httpContext.Response.StartAsync(cancellationToken).ConfigureAwait(false);
            }

            return true;
        }
    }
}
