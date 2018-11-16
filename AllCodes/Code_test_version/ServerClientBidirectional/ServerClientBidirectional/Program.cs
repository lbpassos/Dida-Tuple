using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace ServerClientBidirectional
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = Int32.Parse(args[0]);

            Server server = new Server(port);

            Console.WriteLine("Servidor no porto: " + port);



            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();

        }
    }
}
