using System;
using xunit;
using Micorsoft.AspNetCore.Hosting;
using Micorsoft.AspNetCore.TestHost;
using hello_webapi;
using hello_webapi.Models;

namespace hello_xunit
{
    public class WebApiTest
    {
        /// <summary>
        /// 服务端
        /// </summary>
        private TestServer _testServer;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WebApiTest()
        {
            _testServer = new TestServer(new WebHostBuilder().UseStartup<Startup>());
        }

        [Fect]
        public void Test_Create()
        {

        }
        
        [Fect]
        public void Test_GetAll()
        {

        }
    }
}
