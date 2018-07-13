using System;
using WebApiClient;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using WebApiClient.Defaults;
using WebApiClient.Parameterables;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace hello_retrofit
{
    class Program
    {
        static void Main(string[] args)
        {
            //调用Values Service
            using (var client = HttpApiClient.Create<IValuesApiCaller>())
            {
                Console.WriteLine("-----Invoke Values Service-----");
                var results = client.GetValues().InvokeAsync().Result;
                Console.WriteLine($"results is {results}");
                var result = client.GetValue(10).InvokeAsync().Result;
                Console.WriteLine($"result is {result}");
            }

            //调用Student Service
            using (var client = HttpApiClient.Create<IStudentApiCaller>())
            {
                Console.WriteLine("-----Invoke Student Service-----");
                var students = client.GetAllStudents().InvokeAsync().Result;
                students.ToList().ForEach(student =>
                {
                    Console.WriteLine($"student: {student.Name}");
                });
                var stu = new Student() { Name = "Payne", Age = 26 };
                var result = client.NewStudent(stu).InvokeAsync().Result;
                Console.WriteLine($"result is {result}");
            }

            //调用Files Service
            using (var client = HttpApiClient.Create<IFilesApiCaller>())
            {
                Console.WriteLine("-----Invoke File Service-----");
                var files = new string[]
                {
                    @"C:\Users\PayneQin\Videos\Rec 0001.mp4",
                    @"C:\Users\PayneQin\Videos\Rec 0002.mp4",
                }
                .Select(f=>new MulitpartFile(File.Open(f,FileMode.Open),Path.GetFileName(f)))
                .ToList();
                var result = client.Upload(files).InvokeAsync().Result;
                Console.WriteLine(result);

                var json = JObject.Parse(result);
                var fileId = json[0]["FileId"].Value<string>();
                using(var fileStram = new FileStream("Output/Video001.mp4",FileMode.Create))
                {
                    var stream = client.Download(fileId).InvokeAsync().Result;
                    stream.CopyToAsync(fileStram);
                }
            }
    }
}
