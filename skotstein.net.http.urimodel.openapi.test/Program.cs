using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skotstein.net.http.urimodel.openapi.test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(args[0]);

            UriModel model = UriModelFactory.Instance.Create(args[0]);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(model);
            Console.ReadLine();
        }
    }
}

