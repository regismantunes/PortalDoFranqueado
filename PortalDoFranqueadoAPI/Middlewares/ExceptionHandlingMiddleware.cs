using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using System;
using PortalDoFranqueadoAPI.Models;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace PortalDoFranqueadoAPI.Middlewares
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                await HandleValidationExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleValidationExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var response = new ErrorResponse(exception.Message);

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

#if DEBUG
            var message = exception.Message;
#else
            const message = "Internal Server Error";
#endif
            var response = new ErrorResponse(message);

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

}
