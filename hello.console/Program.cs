using System;
using System.IO;
using System.Text;
using System.Linq;

namespace hello.console
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

            
string temp = ((Double)System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1048576).ToString("N2") + "M";
string temp1 = ((TimeSpan)System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime).TotalSeconds.ToString("N0");

        Console.WriteLine(temp);
        Console.WriteLine(temp1);

         var temp2 = System.Diagnostics.Process.GetProcesses().Select(e=>e.WorkingSet64/1048576).Sum();
Console.WriteLine(temp2);
        }
    }
}
