using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using hello_webapi.Models;
using System.Net.WebSockets;
using System.Text.Encodings;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Abstractions;

namespace hello_webapi.Middlewares
{
    public class WebSocketChat
    {
        /// <summary>
        /// 日志接口
        /// </summary>
        private ILogger<WebSocketChat> _logger;

        /// <summary>
        /// 下一级管道
        /// </summary>
        private RequestDelegate _next;

        /// <summary>
        /// 缓冲区大小
        /// </summary>
        private const int bufferSize = 1024 * 4;

        /// <summary>
        /// URL地址后缀
        /// </summary>
        private const string routePostfix = "/ws";

        /// <summary>
        /// Socket列表
        /// </summary>
        /// <typeparam name="string">typeof(string),用户名</typeparam>
        /// <typeparam name="WebSocket">typeof(WebSocket),WebSocket</typeparam>
        /// <returns></returns>
        private static ConcurrentDictionary<string, WebSocket> _socketsList
            = new ConcurrentDictionary<string, WebSocket>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next">下一级管道</param>
        /// <param name="logger">日志接口</param>
        public WebSocketChat(RequestDelegate next, ILogger<WebSocketChat> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!IsWebSocket(context))
            {
                await _next.Invoke(context);
                return;
            }

            var userName = context.Request.Query["username"].ToArray()[0];
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            if (!ValidateUser(userName))
            {
                _socketsList.TryAdd(userName, socket);
            }

            var webSocket = _socketsList[userName];
            switch(webSocket.State)
            {
                case WebSocketState.
            }
            while (webSocket.State == WebSocketState.Open)
            {
                var message = await ReceiveMessage(webSocket);
                message = message.Replace("\0", "").Trim();
                
                await SendAll(userName,message);
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", default(CancellationToken));
        }

        /// <summary>
        /// 群发消息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        private async Task SendAll(string sender, string message)
        {
            if (_socketsList.Count <= 0) return;
            var tasks = _socketsList
                .Select(e => SendOne(sender, e.Key, message))
                .ToList();
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 给指定用户发送消息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="receiver">接收者</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        private async Task SendOne(string sender, string receiver, string message)
        {
            if (sender == receiver) return;
            if (string.IsNullOrEmpty(message)) return;
            if (!ValidateUser(sender) || !ValidateUser(receiver)) return;
            var socket = _socketsList[receiver];
            var chatEntity = new ChatEntity() { Sender = sender, Message = message };
            await SendMessage(socket, chatEntity);
        }

        /// <summary>
        /// 当前请求是否为WebSocket
        /// </summary>
        /// <param name="context">Http上下文</param>
        /// <returns></returns>
        private bool IsWebSocket(HttpContext context)
        {
            return context.WebSockets.IsWebSocketRequest &&
                context.Request.Path == routePostfix;
        }

        /// <summary>
        /// 验证用户是否存在
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns></returns>
        private bool ValidateUser(string userName)
        {
            return _socketsList.ContainsKey(userName);
        }

        private async Task SendMessage(WebSocket webSocket, ChatEntity entity)
        {
            var bytes = Encoding.UTF8.GetBytes(entity.ToString());

            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
            // while(webSocket.State == WebSocketState.Open)
            // {
            //     var 
            // }
            // var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // while (!result.CloseStatus.HasValue)
            // {

            //     buffer = System.Text.Encoding.UTF8.GetBytes(message);
            //     await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
            //     result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), default(CancellationToken));
            // }
            //await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, default(CancellationToken));
        }

        private async Task<string> ReceiveMessage(WebSocket webSocket)
        {
            var buffer = new ArraySegment<byte>(new byte[bufferSize]);
            var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            while (!result.EndOfMessage)
            {
                result = await webSocket.ReceiveAsync(buffer, default(CancellationToken));
            }

            return Encoding.UTF8.GetString(buffer.Array);
        }
    }
}

