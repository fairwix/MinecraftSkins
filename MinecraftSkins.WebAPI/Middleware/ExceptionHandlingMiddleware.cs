using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinecraftSkins.Application.Exceptions;

namespace MinecraftSkins.WebAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env; 

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        int statusCode;
        string title;
        string detail;
        object? extensions = null;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                title = "Validation failed";
                detail = _env.IsDevelopment() 
                    ? validationException.Message 
                    : "One or more validation errors occurred.";
                _logger.LogWarning(validationException, "Validation error for {Path}: {Message}", 
                    context.Request.Path, validationException.Message);
                break;
            
            case SkinUnavailableException skinEx:
                statusCode = (int)HttpStatusCode.Conflict;
                title = "Skin not available";
                detail = _env.IsDevelopment() 
                    ? skinEx.Message 
                    : skinEx.Reason;
                extensions = new { skinId = skinEx.SkinId, reason = skinEx.Reason };
                _logger.LogWarning(skinEx, "Skin {SkinId} unavailable: {Reason}", skinEx.SkinId, skinEx.Reason);
                break;
            
            case OperationCanceledException:
                statusCode = 499; 
                title = "Request cancelled";
                detail = "The request was cancelled by the client.";
                _logger.LogInformation("Request cancelled for {Path}", context.Request.Path);
                break;
            
            case DbUpdateConcurrencyException:
                statusCode = (int)HttpStatusCode.Conflict;
                title = "Concurrency conflict";
                detail = "The resource was modified by another user. Please try again.";
                break;
            
            case ExternalServiceUnavailableException exс:
                statusCode = (int)HttpStatusCode.ServiceUnavailable;
                title = "External service unavailable";
                detail = _env.IsDevelopment() 
                    ? exс.Message 
                    : "A required external service is temporarily unavailable.";
                _logger.LogError(exс, "External service {ServiceName} unavailable", exс.ServiceName);
                break;
            
            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                title = "An unexpected error occurred";
                detail = _env.IsDevelopment() 
                    ? exception.ToString()
                    : "Internal server error. Please try again later.";
                _logger.LogError(exception, "Unhandled exception for {Path}", context.Request.Path);
                break;
        }

        context.Response.StatusCode = statusCode;

        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            detail,
            instance = context.Request.Path,
            service = exception is ExternalServiceUnavailableException ex ? ex.ServiceName : null,
            extensions
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        await context.Response.WriteAsync(json);
    }
}