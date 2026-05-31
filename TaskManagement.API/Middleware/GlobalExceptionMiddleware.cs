using System.Net;
using System.Text.Json;

namespace TaskManagement.API.Middleware
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
                //  tells ASP.NET to  run the controllers, database queries...
                await _next(context);
            }
            catch (Exception ex)
            {
                // If anything crashes anywhere in the app it will fall back to here

                // log the exact error and stack trace
                _logger.LogError(ex, "CRITICAL: An unhandled exception occurred while processing the request.");

                // return a user friendly JSON response to the  frontend
                await HandleExceptionAsync(context);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context)
        {
            // Set the response type to JSON and status code to 500 (Server Error)
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // message 
            var response = new
            {
                error = "Server Error",
                message = "An unexpected error occurred on our end. Our engineering team has been notified."
            };

            // serialize and send back to the user
            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}