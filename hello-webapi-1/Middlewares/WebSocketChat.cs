using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using hello_webapi.Models;
using System.Net.WebSockets;
using System.Text.Encodings;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
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
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            while (webSocket.State == WebSocketState.Open)
            {
                var entity = await Receiveentity<MessageEntity>(webSocket);
                switch (entity.Type)
                {
                    case MessageType.Chat:
                        await HandleChat(webSocket, entity);
                        break;
                    case MessageType.Event:
                        await HandleEvent(webSocket, entity);
                        break;
                }
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", default(CancellationToken));
        }

        private async Task HandleChat(WebSocket webSocket, MessageEntity entity)
        {
            var receiver = entity.Receiver;
            if (receiver == "All")
            {
                await SendAll(entity.Sender, entity.Message);
            }
            else
            {
                await SendOne(entity.Sender, entity.Receiver, entity.Message);
            }
        }

        private async Task HandleEvent(WebSocket webSocket, MessageEntity entity)
        {
            var userName = entity.Sender;
            var eventData = JsonConvert.DeserializeObject<EventData<object>>(entity.Message);
            var eventName = eventData.Event;
            switch (eventName)
            {
                case Events.Joined:
                    _socketsList.TryAdd(userName, webSocket);
                    await SendAll("系统消息", $"用户{userName}已进入聊天室");
                    await SendEvent<List<string>>(eventName, _socketsList.Select(e => e.Key).ToList());
                    break;
                case Events.Leaved:
                    _socketsList.TryRemove(userName, out webSocket);
                    await SendAll("系统消息", $"用户{userName}已离开聊天室");
                    await SendEvent<List<string>>(eventName, _socketsList.Select(e => e.Key).ToList());
                    break;
            }
        }

        /// <summary>
        /// 群发消息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        private async Task SendAll(string sender, string message, MessageType type = MessageType.Chat)
        {
            if (_socketsList.Count <= 0) return;
            var tasks = _socketsList
                .Select(e => SendOne(sender, e.Key, message, type))
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
        private async Task SendOne(string sender, string receiver, string message, MessageType type = MessageType.Chat)
        {
            if (sender == receiver) return;
            if (string.IsNullOrEmpty(message)) return;
            if (!ValidateUser(receiver)) return;
            var socket = _socketsList[receiver];
            var chatEntity = new MessageEntity() { Sender = sender, Message = message, Type = type };
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

        private async Task SendEvent<TData>(Events eventName, TData data)
        {
            var eventData = new EventData<TData>();
            eventData.Event = eventName;
            eventData.Data = data;
            var message = JsonConvert.SerializeObject(eventData);
            await SendAll("Server", message, MessageType.Event);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="webSocket">WebSocket</param>
        /// <typeparam name="TEntity">typeof(TEntity)</typeparam>
        /// <returns></returns>
        private async Task<TEntity> Receiveentity<TEntity>(WebSocket webSocket)
        {
            var buffer = new ArraySegment<byte>(new byte[bufferSize]);
            var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            while (!result.EndOfMessage)
            {
                result = await webSocket.ReceiveAsync(buffer, default(CancellationToken));
            }

            var json = Encoding.UTF8.GetString(buffer.Array);
            json = json.Replace("\0", "").Trim();
            return JsonConvert.DeserializeObject<TEntity>(json, new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            });
        }
    }
}

