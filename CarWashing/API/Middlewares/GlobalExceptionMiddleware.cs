using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            catch (DbUpdateException dbUpdateException)
                when (dbUpdateException.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                _logger.LogWarning(dbUpdateException, "A duplicate record was rejected for {Method} {Path}", context.Request.Method, context.Request.Path);
                await HandleExceptionAsync(context, dbUpdateException.InnerException);
            }
            catch (DbUpdateConcurrencyException concurrencyException)
            {
                _logger.LogWarning(concurrencyException, "A concurrent update was rejected for {Method} {Path}", context.Request.Method, context.Request.Path);
                await HandleExceptionAsync(context, concurrencyException);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            var problemDetails = exception switch
            {
                ValidationException validationEx => new ValidationProblemDetails(
                    validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()))
                {
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Instance = context.Request.Path
                },

                UnauthorizedAccessException unauthorizedEx => new ProblemDetails
                {
                    Title = "Unauthorized",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = unauthorizedEx.Message,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                    Instance = context.Request.Path
                },

                InvalidOperationException invalidOpEx => new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = invalidOpEx.Message,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Instance = context.Request.Path
                },

                KeyNotFoundException notFoundEx => new ProblemDetails
                {
                    Title = "Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = notFoundEx.Message,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    Instance = context.Request.Path
                },

                PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } => new ProblemDetails
                {
                    Title = "Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "A record with the same unique value already exists.",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    Instance = context.Request.Path
                },

                DbUpdateConcurrencyException => new ProblemDetails
                {
                    Title = "Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "This booking has changed. Refresh and try again.",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    Instance = context.Request.Path
                },

                _ => new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred. Please try again later.",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                    Instance = context.Request.Path
                }
            };

            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType());
        }
    }

    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
