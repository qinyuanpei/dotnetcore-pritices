using System;
using System.Net.Http;
using WebApiClient;
using WebApiClient.Attributes;
using WebApiClient.DataAnnotations;
using WebApiClient.Parameterables;

namespace hello_retrofit
{
    [HttpHost("http://localhost:8000")]
    public interface IValuesApiCaller : IHttpApiClient
    {
        //GET http://localhost:8000/values1
        [HttpGet("/values1")]
        ITask<string> GetValues();
        
        //GET http://localhost:8000/values1/{id}
        [HttpGet("/values1/{id}")]
        ITask<string> GetValue(int id);
    }
}