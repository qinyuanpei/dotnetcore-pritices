using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using hello_webapi.Middlewares;

namespace hello_webapi.Extenstions
{
    public static class MiddlewareExtenstion
    {
        public static void UseWebSocketChat(this IApplicationBuilder app)
        {
            app.UseMiddleware<WebSocketChat>();
        }

        public static void UseWebSocketPush(this IApplicationBuilder app)
        {
            app.UseMiddleware<WebSocketPush>();
        }
    }
}