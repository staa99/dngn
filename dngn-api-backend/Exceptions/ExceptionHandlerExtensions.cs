using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace DngnApiBackend.Exceptions
{
    public static class ExceptionHandlerExtensions
    {
        public static void UseAppExceptionHandler(this IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.Use(WriteDevelopmentResponse);
            }
            else
            {
                app.Use(WriteProductionResponse);
            }
        }

        private static Task WriteDevelopmentResponse(HttpContext httpContext, Func<Task> next)
        {
            return WriteResponse(httpContext, true);
        }

        private static Task WriteProductionResponse(HttpContext httpContext, Func<Task> next)
        {
            return WriteResponse(httpContext, false);
        }

        private static async Task WriteResponse(HttpContext httpContext, bool includeDetails)
        {
            var exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
            var ex = exceptionDetails?.Error;

            if (ex != null)
            {
                httpContext.Response.ContentType = "application/json";
                var stream = httpContext.Response.Body;

                var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

                string code;
                string message;

                if (ex is BaseApplicationException applicationException)
                {
                    code                            = applicationException.Code;
                    message                         = applicationException.Message;
                    httpContext.Response.StatusCode = applicationException.StatusCode;
                }
                else
                {
                    code                            = "UNEXPECTED_ERROR";
                    message                         = "An unexpected error occurred";
                    httpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                }

                if (includeDetails)
                {
                    await JsonSerializer.SerializeAsync(stream, new
                    {
                        status = "failed",
                        error  = new {code, message},
                        traceId,
                        exception = ex.ToString()
                    });
                }
                else
                {
                    await JsonSerializer.SerializeAsync(stream, new
                    {
                        status = "failed",
                        error  = new {code, message},
                        traceId
                    });
                }
            }
        }
    }
}