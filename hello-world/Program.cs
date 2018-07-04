using System;
using System.IO;
using System.Text;

namespace hello_world
{
    class Program
    {
        static void Main(string[] args)
        {
            var msg = "Hello .NET Core";
            using(var fileStream = new FileStream("output.txt",FileMode.Create,FileAccess.Write))
            {
                var bytes = Encoding.UTF8.GetBytes(msg);
                fileStream.Write(bytes,0,bytes.Length);
                Console.WriteLine(msg);
            }

        }
    }
}
