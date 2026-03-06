using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace ProductCatalog.Api.Middleware;

/// <summary>
/// Request logging — built from scratch using RequestDelegate.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        _logger.LogInformation("[{RequestId}] → {Method} {Path}{Query}",
            requestId, context.Request.Method, context.Request.Path, context.Request.QueryString);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("[{RequestId}] ← {StatusCode} in {ElapsedMs}ms",
                requestId, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}

/// <summary>
/// Global exception handler — pattern matches exception types to HTTP status codes.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ArgumentException argEx => (HttpStatusCode.BadRequest, argEx.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            InvalidOperationException opEx => (HttpStatusCode.Conflict, opEx.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(new
        {
            message,
            statusCode = (int)statusCode,
            timestamp = DateTime.UtcNow
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Clean extension methods to register middleware.
/// </summary>
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();

    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}