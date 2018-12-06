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
using System.Text.RegularExpressions;

namespace Projeto_DAD
{

    public class EachServer
    {
        private Uri uid;  //address
        private int id;   //process id given in configuration file (must be unique)

        public EachServer(Uri address, int server_id)
        {
            uid = address;
            id = server_id;
        }

        public Uri UID
        {
            get { return uid; }
            set { uid = value; }
        }

        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public override bool Equals(object obj)
        {
            EachServer a = obj as EachServer;
            if (a == null)
            {
                return false;
            }
            return id == a.ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode() + UID.GetHashCode();
        }
    }


    class Server
    {
        public static List<EachServer> AllServers;    //All servers present in the pool
        public static EachServer My_Identification;   //Current Server
        private const string path = "..\\..\\..\\Filedatabase\\database.txt"; //database of all servers

        private static TcpChannel channel;

        private static int delay_messages;

        static void Main(string[] args)
        {
            int id;
            Uri uri;
            int min_delay;
            int max_delay;

            if (args.Length != 4)
            {
                Console.WriteLine("Insuficient arguments: SERVER_ID URL MIN_DELAY MAX_DELAY");
                args = Console.ReadLine().Split(' ');

            }
            
            id = Int32.Parse(args[0].Substring(1)); //catch number from the 1 position

 
            
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
            Random rnd = new Random();
            delay_messages = rnd.Next(min_delay, max_delay+1);

            //open file with database of all Servers in the system
            AllServers = new List<EachServer>();
            My_Identification = new EachServer(uri, id);

            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');

                int tmp_id = Int32.Parse(words[0]);
                Uri uri_tmp;
                try
                {
                    uri_tmp = new Uri(words[1]);
                    AllServers.Add(new EachServer(uri_tmp, tmp_id));
                }
                catch (UriFormatException e)
                {
                    Console.WriteLine("Invalid URL: {0}", words[1]);
                    //Console.ReadLine();
                    return;
                }
            }

            channel = new TcpChannel(uri.Port);
            ChannelServices.RegisterChannel(channel, false);

            new Thread(() => Server_thread()).Start();
            new Thread(() => Client_thread(uri, id)).Start();
            new Thread(() => ServerService.CheckCommandsInQueue_thread()).Start();

            System.Console.WriteLine("I'm server: " + args[1]);

            //teste tuple space
            object[] tupleOBJ = new object[1];
            tupleOBJ[0] = new StringEmulator("a");
            ServerService.ts.Add( new MyTuple(tupleOBJ) );

            System.Console.ReadLine();
        }

        public static void Server_thread()
        {
            
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerService),
                "MyRemoteObjectName",
                WellKnownObjectMode.Singleton);

            while (true) ;

        }

        public static void Client_thread(Uri uri, int id) //Client in Server
        {

            new ServerProgram(uri,id);

            while (true) ;

        }

        

        

    }
}
