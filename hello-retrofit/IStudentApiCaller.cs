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
    public interface IStudentApiCaller : IHttpApiClient
    {
        //GET http://localhost:8000/student
        [HttpGet("/student")]
        [OAuth2Filter]
        [JsonReturn]
        ITask<List<Student>> GetAllStudents();
        
        
        //POST http://localhost:8000/student
        [HttpPost("/student")]
        [OAuth2Filter]
        ITask<string> NewStudent([JsonContent] Student student);
    }
}