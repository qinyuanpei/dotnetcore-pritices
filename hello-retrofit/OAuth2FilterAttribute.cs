using System;
using WebApiClient;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using WebApiClient.Contexts;
using WebApiClient.Attributes;
using WebApiClient.DataAnnotations;
using WebApiClient.Parameterables;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace hello_retrofit
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OAuth2FilterAttribute : ApiActionFilterAttribute
    {
        public override Task OnBeginRequestAsync(ApiActionContext context)
        {
            using (var client = HttpApiClient.Create<IAuthApiCaller>())
            {
                var client_id = "578c06935d7f4c9897316ed50b00c19d";
                var client_secret = "d851c10e1897482eb6f476e359984b27";
                var result = client.GetToken(client_id, client_secret).InvokeAsync().Result;
                Console.WriteLine(result);
                var json = JObject.Parse(result);
                var token = json["access_token"].Value<string>();
                Console.WriteLine($"AccessToken: {token}");
                context.RequestMessage.Headers.Authorization = new AuthenticationHeaderValue($"Bearer {token}");
                return base.OnBeginRequestAsync(context);
            }
        }
    }
}