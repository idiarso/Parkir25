using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ParkIRC.Web.Services;

namespace ParkIRC.Extensions
{
    public static class WebSocketExtensions
    {
        public static IServiceCollection AddWebSocketServer(this IServiceCollection services)
        {
            services.AddSingleton<IWebSocketServer, WebSocketServer>();
            return services;
        }

        public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder app)
        {
            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2),
                ReceiveBufferSize = 4 * 1024  // 4KB
            };

            app.UseWebSockets(webSocketOptions);

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    var webSocketServer = context.RequestServices.GetRequiredService<IWebSocketServer>();
                    await webSocketServer.HandleConnection(context);
                }
                else
                {
                    await next();
                }
            });

            return app;
        }
    }
} 