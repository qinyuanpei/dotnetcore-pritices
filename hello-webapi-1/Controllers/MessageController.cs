using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using hello_webapi.Models;
using ServiceStack.Redis;
using hello_webapi.Models;
using Microsoft.Extensions.Configuration;


namespace hello_webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private RedisConfiguration _configuration;

        private readonly RedisClient _redisPublisher;

        public MessageController(IConfiguration configuration)
        {
            var section = configuration.GetSection("Redis");
            _configuration = section.Get<RedisConfiguration>();
            _redisPublisher = new RedisClient(_configuration.Host);
        }

        [HttpPost]
        [Route("/api/message/publish/barrage")]
        public IActionResult Publish()
        {
            Stream stream = HttpContext.Request.Body;
            byte[] buffer = new  byte[HttpContext.Request.ContentLength.Value];
            stream.Read(buffer, 0, buffer.Length);
            string message = System.Text.Encoding.UTF8.GetString(buffer);
            _redisPublisher.PublishMessage("barrage", message);
            return Ok();
        }
    }
}