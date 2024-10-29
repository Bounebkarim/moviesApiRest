using System.Net;
using FluentValidation;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping;

public class ValidationMappingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (ValidationException exception)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var validationFailureResponse = new ValidationFailureResponse
            {
                Errors = exception.Errors.Select(o => new ValidationResponse
                {
                    PropertyName = o.PropertyName,
                    Message = o.ErrorMessage
                })
            };
            await httpContext.Response.WriteAsJsonAsync(validationFailureResponse);
        }
    }
}