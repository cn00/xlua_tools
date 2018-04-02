using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StreamRedirect
{
    class StreamRedirectTest
    {
        static void Main(string[] args)
        {
            var stream = new StreamRedirect("./test1.txt");
            stream.AutoFlush = true;
            stream.Add("./test2.txt");
            stream.Add("./test3.txt");
            stream.Add(Console.Out);
            Console.SetOut(stream);

            Console.Write("test1 ..{0}...", 78987);
            Console.Write(123456789);
            Console.WriteLine("WriteLine .....");
            stream.Flush();
            Console.ReadLine();
        }
    }
}
