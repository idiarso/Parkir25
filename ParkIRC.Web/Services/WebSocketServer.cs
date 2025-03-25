using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using ParkIRC.Extensions;

namespace ParkIRC.Web.Services
{
    public interface IWebSocketServer
    {
        Task HandleConnection(HttpContext context);
        Task BroadcastMessage(string message);
        Task BroadcastMessage<T>(string eventType, T data);
        Task SendMessage(string connectionId, string message);
        Task SendMessage<T>(string connectionId, string eventType, T data);
    }

    public class WebSocketServer : IWebSocketServer
    {
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        private readonly ILogger<WebSocketServer> _logger;

        public WebSocketServer(ILogger<WebSocketServer> logger)
        {
            _logger = logger;
        }

        public async Task HandleConnection(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var connectionId = Guid.NewGuid().ToString();

            try
            {
                _connections.TryAdd(connectionId, webSocket);
                _logger.LogInformation("WebSocket client connected: {ConnectionId}", connectionId);

                await HandleWebSocketCommunication(connectionId, webSocket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling WebSocket connection {ConnectionId}", connectionId);
            }
            finally
            {
                await CloseConnection(connectionId);
            }
        }

        public async Task BroadcastMessage(string message)
        {
            var tasks = _connections.Select(kvp => SendMessageInternal(kvp.Value, message));
            await Task.WhenAll(tasks);
        }

        public async Task BroadcastMessage<T>(string eventType, T data)
        {
            var message = new WebSocketMessage<T>
            {
                Event = eventType,
                Data = data
            };

            await BroadcastMessage(JsonSerializer.Serialize(message));
        }

        public async Task SendMessage(string connectionId, string message)
        {
            if (_connections.TryGetValue(connectionId, out var webSocket))
            {
                await SendMessageInternal(webSocket, message);
            }
        }

        public async Task SendMessage<T>(string connectionId, string eventType, T data)
        {
            var message = new WebSocketMessage<T>
            {
                Event = eventType,
                Data = data
            };

            await SendMessage(connectionId, JsonSerializer.Serialize(message));
        }

        private async Task HandleWebSocketCommunication(string connectionId, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await HandleMessage(connectionId, message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await CloseConnection(connectionId);
                        break;
                    }
                }
                catch (WebSocketException ex)
                {
                    _logger.LogError(ex, "WebSocket error for connection {ConnectionId}", connectionId);
                    break;
                }
            }
        }

        private async Task HandleMessage(string connectionId, string message)
        {
            try
            {
                // Handle incoming messages here
                // You can implement your own message handling logic
                _logger.LogInformation("Received message from {ConnectionId}: {Message}", connectionId, message);

                // Echo the message back to the sender
                await SendMessage(connectionId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message from {ConnectionId}", connectionId);
            }
        }

        private async Task SendMessageInternal(WebSocket webSocket, string message)
        {
            if (webSocket.State != WebSocketState.Open)
                return;

            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }

        private async Task CloseConnection(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var webSocket))
            {
                try
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Closing connection",
                            CancellationToken.None
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing WebSocket connection {ConnectionId}", connectionId);
                }
                finally
                {
                    webSocket.Dispose();
                    _logger.LogInformation("WebSocket client disconnected: {ConnectionId}", connectionId);
                }
            }
        }
    }

    public class WebSocketMessage<T>
    {
        public string Event { get; set; }
        public T Data { get; set; }
    }
} 