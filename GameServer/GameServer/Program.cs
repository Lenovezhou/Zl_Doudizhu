using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AhpilyServer;

namespace GameServerN
{
    class Program
    {
        static void Main(string[] args)
        {
            Serverpeer server = new Serverpeer();
            server.Start(6666, 100);
            Console.ReadKey();
        }
    }
}
