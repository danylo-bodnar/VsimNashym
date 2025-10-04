using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

internal sealed class ValidationExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<ValidationException> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Detail = "One or more validation errors occured",
                Status = StatusCodes.Status400BadRequest
            }
        };

        var errors = validationException.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key.ToLowerInvariant(), g => g.Select(e => e.ErrorMessage).ToArray());

        context.ProblemDetails.Extensions.Add("errors", errors);

        logger.LogWarning("Validation failed on request {Path} for user {User}. Errors: {Errors}",
                   httpContext.Request.Path,
                   httpContext.User?.Identity?.Name ?? "Anonymous",
                   string.Join("; ", validationException.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
               );

        return await problemDetailsService.TryWriteAsync(context);
    }
}
