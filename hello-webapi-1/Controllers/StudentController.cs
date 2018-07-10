using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using hello_webapi.Repository;
using hello_webapi.Models;
using Newtonsoft.Json;

namespace hello_webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        [HttpPost]
        public JsonResult Post(dynamic obj)
        {
            using(var repository = new StudentRepository())
            {
                var data = new Student()
                {
                    Name=obj.Name,
                    Age = obj.Age,
                    Sex = obj.Sex,
                };
                repository.Students.Add(data);

                repository.SaveChanges();

                return new JsonResult(new {Status="Success",Data=data,CreateTine=DateTime.UtcNow});
            }
        }

        public ActionResult<IEnumerable<Student>> Get()
        {
            using(var repository = new StudentRepository())
            {
                return repository.Students.ToList();
            }
        }
    }
}