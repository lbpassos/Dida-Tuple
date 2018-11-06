using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace Server
{
    class Server
    {
        //Dummy Server - Echo for testing
        //********************************************************************
        static int id;
        static Uri uri;
        static int min_delay;
        static int max_delay;

        static void Main(string[] args)
        {
           

            if (args.Length != 4)
            {
                Console.WriteLine("Insuficient arguments: SERVER_ID URL MIN_DELAY MAX_DELAY");
                Console.ReadLine();
                return;
            }

            id = Int32.Parse(args[0]);
            try
            {
                uri = new Uri(args[1]);
            }
            catch (UriFormatException e)
            {
                Console.WriteLine("Invalid URL: {0}", args[1]);
                Console.ReadLine();
                return;
            }

            min_delay = Int32.Parse(args[2]);
            max_delay = Int32.Parse(args[3]);


            TcpChannel channel = new TcpChannel( uri.Port);
            ChannelServices.RegisterChannel(channel, true);


            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerService),
                "MyRemoteObjectName",
                WellKnownObjectMode.Singleton);

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();







            

            //Console.WriteLine("Server ON in port " + port);
            /*while (true)
            {
                string inputLine = "";
                while (inputLine != null)
                {
                    inputLine = reader.ReadLine();
                    writer.WriteLine(inputLine);
                    Console.WriteLine(inputLine);
                }
                Console.WriteLine("Server saw disconnect from client.");
            }*/ 
        }// END
         //********************************************************************
    }
}
