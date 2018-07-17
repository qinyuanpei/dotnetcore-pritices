using System;
using System.Net.WebSockets;
using System.Text.Encodings;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;

namespace hello_webapi.Middlewares
{
    public class WebSocketChat
    {
        // 日志接口
        private Microsoft.Extensions.Logging.ILogger<WebSocketChat> _logger;

        // 下一级管道
        private RequestDelegate _next;

        // 缓冲区大小
        private int bufferSize = 1024 * 4;

        // Socket列表
        private static ConcurrentDictionary<string, WebSocket> _sockers
            = new ConcurrentDictionary<string, WebSocket>();

        // 构造函数
        public WebSocketChat(RequestDelegate next, Microsoft.Extensions.Logging.ILogger<WebSocketChat> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var isWebSocket = context.WebSockets.IsWebSocketRequest;
            if (!isWebSocket)
            {
                await _next.Invoke(context);
                return;
            }

            var requestPath = context.Request.Path;
            if (requestPath == "/ws")
            {
                var clientId = context.Request.Query["username"].ToArray()[0];
                if (!_sockers.ContainsKey(clientId))
                {
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    _sockers.TryAdd(clientId, socket);
                }
                var webSocket = _sockers[clientId];
                //var clientId = context.Request.QueryString["clientId"];


                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), default(CancellationToken));
                while (!result.CloseStatus.HasValue)
                {

                    var message = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length).Trim();
                    var date = DateTime.Now;
                    buffer = System.Text.Encoding.UTF8.GetBytes($"{date} - 服务器收到消息 - {message}");
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), result.MessageType, result.EndOfMessage, default(CancellationToken));

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), default(CancellationToken));
                }
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, default(CancellationToken));
            }
        }

    }
}
