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
using System.Threading;

namespace Server
{
    class Server
    {
        //Dummy Server - Echo for testing
        //********************************************************************
        public static int id;
        public static Uri uri;
        public static int min_delay;
        public static int max_delay;

        private static TcpChannel channel;

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


            /*TcpChannel channel = new TcpChannel( uri.Port);
            ChannelServices.RegisterChannel(channel, true);


            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerService),
                "MyRemoteObjectName",
                WellKnownObjectMode.Singleton);
                */

            //new Thread(Server_thread).Start();
            channel = new TcpChannel(uri.Port);
            ChannelServices.RegisterChannel(channel, true);

            new Thread(() => Server_thread()).Start();
            new Thread(() => Client_thread()).Start();

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
         public static void Server_thread()
        {
            //TcpChannel channel = new TcpChannel(uri.Port);
            //ChannelServices.RegisterChannel(channel, true);


            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerService),
                "MyRemoteObjectName",
                WellKnownObjectMode.Singleton);

            while (true) ;

        }

        public static void Client_thread()
        {

            new ServerProgram(Server.uri, Server.id);

                        

            while (true) ;

        }

    }
}
