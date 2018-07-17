using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.WebSockets;
using hello_webapi.Repository;
using System.Net.WebSockets;
using System.Threading;
using hello_webapi.Middlewares;
using hello_webapi.Extenstions;

namespace hello_webapi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddLogging(Logger=>
            {
                Logger.AddDebug();
                Logger.AddConsole();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            

            //app.UseHttpsRedirection();
            app.UseWebSockets(new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            });

            app.UseMvc();
            app.UseWebSocketChat();
            app.Use(async (context, next) =>
            {
                var requestPath = context.Request.Path;
                if (requestPath == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var socket = await context.WebSockets.AcceptWebSocketAsync();
                        await Echo(context,socket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }

                await next();
            });

        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), default(CancellationToken));
            while (!result.CloseStatus.HasValue)
            {
                
                var message = System.Text.Encoding.UTF8.GetString(buffer,0,buffer.Length).Trim();
                var date = DateTime.Now;
                buffer = System.Text.Encoding.UTF8.GetBytes($"服务器收到消息:{message}");
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, default(CancellationToken));

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), default(CancellationToken));
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, default(CancellationToken));
        }
    }
}
