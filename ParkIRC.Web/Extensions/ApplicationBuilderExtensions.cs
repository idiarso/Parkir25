using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ParkIRC.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApplicationErrorHandling(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        var response = new
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = "An error occurred while processing your request.",
                            DetailedMessage = error.Error.Message
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    }
                });
            });

            app.UseStatusCodePages(async context =>
            {
                context.HttpContext.Response.ContentType = "application/json";

                var response = new
                {
                    StatusCode = context.HttpContext.Response.StatusCode,
                    Message = context.HttpContext.Response.StatusCode switch
                    {
                        401 => "Unauthorized",
                        403 => "Forbidden",
                        404 => "Not Found",
                        500 => "Internal Server Error",
                        _ => "An error occurred"
                    }
                };

                await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
            });

            return app;
        }

        public static IApplicationBuilder UseApplicationSecurityHeaders(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:;");
                context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

                await next();
            });

            return app;
        }

        public static IApplicationBuilder UseApplicationRequestLogging(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var startTime = DateTime.UtcNow;
                var request = context.Request;

                await next();

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                var logMessage = $"{request.Method} {request.Path} - {context.Response.StatusCode} - {duration.TotalMilliseconds}ms";
                // TODO: Add proper logging
                System.Diagnostics.Debug.WriteLine(logMessage);
            });

            return app;
        }

        public static IApplicationBuilder UseApplicationPerformanceMonitoring(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var startTime = DateTime.UtcNow;
                var request = context.Request;

                await next();

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                if (duration.TotalMilliseconds > 1000)
                {
                    var logMessage = $"Slow Request: {request.Method} {request.Path} - {duration.TotalMilliseconds}ms";
                    // TODO: Add proper logging
                    System.Diagnostics.Debug.WriteLine(logMessage);
                }
            });

            return app;
        }

        public static IApplicationBuilder UseApplicationRequestValidation(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var request = context.Request;

                // Validate request size
                if (request.ContentLength > 10 * 1024 * 1024) // 10MB
                {
                    context.Response.StatusCode = 413; // Payload Too Large
                    await context.Response.WriteAsync("Request too large");
                    return;
                }

                // Validate content type
                if (request.ContentType != null && !request.ContentType.StartsWith("application/json"))
                {
                    context.Response.StatusCode = 415; // Unsupported Media Type
                    await context.Response.WriteAsync("Unsupported content type");
                    return;
                }

                await next();
            });

            return app;
        }

        public static IApplicationBuilder UseApplicationResponseCaching(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                context.Response.Headers.Add("Pragma", "no-cache");
                context.Response.Headers.Add("Expires", "0");

                await next();
            });

            return app;
        }

        public static IApplicationBuilder UseApplicationCors(this IApplicationBuilder app)
        {
            app.UseCors("AllowSpecific");
            return app;
        }

        public static IApplicationBuilder UseApplicationAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            return app;
        }

        public static IApplicationBuilder UseApplicationAuthorization(this IApplicationBuilder app)
        {
            app.UseAuthorization();
            return app;
        }

        public static IApplicationBuilder UseApplicationSession(this IApplicationBuilder app)
        {
            app.UseSession();
            return app;
        }

        public static IApplicationBuilder UseApplicationResponseCompression(this IApplicationBuilder app)
        {
            app.UseResponseCompression();
            return app;
        }

        public static IApplicationBuilder UseApplicationHttpsRedirection(this IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            return app;
        }

        public static IApplicationBuilder UseApplicationHsts(this IApplicationBuilder app)
        {
            app.UseHsts();
            return app;
        }

        public static IApplicationBuilder UseApplicationStaticFiles(this IApplicationBuilder app)
        {
            app.UseStaticFiles();
            return app;
        }

        public static IApplicationBuilder UseApplicationRouting(this IApplicationBuilder app)
        {
            app.UseRouting();
            return app;
        }

        public static IApplicationBuilder UseApplicationEndpoints(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            return app;
        }
    }
} 