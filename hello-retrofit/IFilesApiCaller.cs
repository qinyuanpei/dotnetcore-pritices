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
    [HttpHost("http://localhost:8000")]
    public interface IFilesApiCaller : IHttpApiClient
    {
        //Post http://localhost:8000/files/upload
        [HttpPost("/files/upload")]
        [OAuth2Filter]
        [JsonReturn]
        ITask<string> Upload([HttpContent]List<MulitpartFile> files);
        
        
        //POST http://localhost:8000/files/download/{fileId}
        [HttpPost("/files/download/{fileId}")]
        [OAuth2Filter]
        ITask<HttpResponseMessage> Download(string fileId);
    }
}