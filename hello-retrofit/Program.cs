using System;
using WebApiClient;
using System.Collections.Generic;
using System.Linq;

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

        }
    }
}
