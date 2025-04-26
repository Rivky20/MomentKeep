using System.Net;
using System.Text.Json;
using MomentKeep.Core.DTOs;

namespace MomentKeep.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            var errorType = "ServerError";
            var message = "An unexpected error occurred";

            // Handle specific exceptions
            switch (ex)
            {
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    errorType = "UnauthorizedError";
                    message = "You are not authorized to perform this action";
                    break;

                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorType = "BadRequestError";
                    message = ex.Message;
                    break;

                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorType = "BadRequestError";
                    message = ex.Message;
                    break;

                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    errorType = "NotFoundError";
                    message = "The requested resource was not found";
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var errorResponse = new ErrorDto
            {
                Type = errorType,
                Message = message
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(errorResponse, jsonOptions);

            await context.Response.WriteAsync(json);
        }
    }
}