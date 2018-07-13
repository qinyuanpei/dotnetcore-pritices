using System;
using WebApiClient;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using WebApiClient.Attributes;
using WebApiClient.DataAnnotations;
using WebApiClient.Parameterables;

namespace hello_retrofit
{
    [HttpHost("http://localhost:28203")]
    public interface IAuthApiCaller : IHttpApiClient
    {
        [HttpPost("/oauth2/token")]
        ITask<string> GetToken([FormField] string client_id,[FormField] string client_secret,[FormField] string grant_type = "client_credentials");
    }
}