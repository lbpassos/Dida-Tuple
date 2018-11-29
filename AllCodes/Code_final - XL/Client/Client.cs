using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Text.RegularExpressions;

namespace Projeto_DAD
{
    class Client
    {

        private static TcpChannel channel;

        static void Main(string[] args)
        {


            int id;
            Uri uri;
            string path = null;

            if (args.Length < 2 || args.Length >= 4)
            {
                Console.WriteLine("Wrong arguments: CLIENT_ID URL SCRIPT_FILE");
                args = Console.ReadLine().Split(' ');

            }

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


            try
            {
                id = Int32.Parse(args[0].Substring(1)); //catch number from the 1 position
                path = args[2]; //Depois tem que se fazer a leitura do ficheiro ************************
            }
            catch (Exception e)
            {
                if (e is IndexOutOfRangeException)
                {
                }
                else
                {
                    Console.WriteLine(e);
                }
            }


            channel = new TcpChannel(uri.Port);
            ChannelServices.RegisterChannel(channel, false);

            new Thread(() => ClientCallbck_thread()).Start();
            new Thread(() => Client_thread(uri, path)).Start();


        }

        public static void ClientCallbck_thread() //Server will use to sink information
        {

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ClientServices),
                "MyRemoteClient",
                WellKnownObjectMode.Singleton);

            while (true) ;

        }

        public static void Client_thread(Uri uri, String path)
        {

            Console.WriteLine(uri);

            ClientProgram cp = new ClientProgram(uri, path);
            cp.ClientStateMachine(); //while infinite



        }








    }
}
