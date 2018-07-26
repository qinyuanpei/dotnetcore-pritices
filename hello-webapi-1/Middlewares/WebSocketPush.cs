using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Timers;
using System.Threading;
using System.Diagnostics;
using hello_webapi.Models;
using System.Net.WebSockets;
using System.Text.Encodings;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using ServiceStack.Redis;
using Microsoft.Extensions.Logging.Abstractions;

namespace hello_webapi.Middlewares
{
    public class WebSocketPush
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
        private const string routePostfix = "/push";

        private TimeSpan interval = TimeSpan.FromMinutes(2);

        private readonly SimpleMessageQueue _messageQueue;

        private RedisConfiguration _configuration;

        private ConcurrentBag<WebSocket> _socketList = new ConcurrentBag<WebSocket>();


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next">下一级管道</param>
        /// <param name="logger">日志接口</param>
        public WebSocketPush(RequestDelegate next, ILogger<WebSocketChat> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            var section = configuration.GetSection("Redis");
            _configuration = section.Get<RedisConfiguration>();
           _messageQueue = new SimpleMessageQueue(_configuration.Host);
        }

        public async Task Invoke(HttpContext context)
        {
            if (!IsWebSocket(context))
            {
                await _next.Invoke(context);
                return;
            }

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            _socketList.Add(webSocket);
            while (webSocket.State == WebSocketState.Open)
            {
                var message = _messageQueue.Pull("barrage",TimeSpan.FromMilliseconds(2));
                foreach(var socket in _socketList)
                {
                    await SendMessage(socket,message);
                }
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", default(CancellationToken));
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
        /// 发送消息
        /// </summary>
        /// <param name="webSocket">WebSocket</param>
        /// <param name="entity">消息实体</param>
        /// <typeparam name="TEntity">typeof(TEntity)</typeparam>
        /// <returns></returns>
        private async Task SendMessage<TEntity>(WebSocket webSocket, TEntity entity)
        {
            var settings = new JsonSerializerSettings();
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            var Json = JsonConvert.SerializeObject(entity);
            var bytes = Encoding.UTF8.GetBytes(Json);

            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }

        private async Task SendMessage(WebSocket webSocket, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);

            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }

        private double GetCPUUsage()
        {
            return 0d;
            //return Process.GetCurrentProcess().WorkingSet64
        }
    }
}

