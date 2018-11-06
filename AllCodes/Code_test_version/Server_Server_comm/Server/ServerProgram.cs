using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;


namespace Server
{
    public class EachServer
    {
        private Uri uid;
        private int id;
        
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
    }

    class ServerProgram
    {
        private List<EachServer> AllServers;
        private EachServer My_Identification;
        private const string path = "..\\..\\..\\Filedatabase\\database.txt"; //database of all servers
        

        public ServerProgram(Uri uri, int server_id )
        {
            //open file with database of all Servers in the system
            AllServers = new List<EachServer>();
            My_Identification = new EachServer(uri, server_id);

            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');

                int tmp_id = Int32.Parse(words[0]);
                Uri uri_tmp;
                try
                {
                    uri_tmp = new Uri(words[1]);
                    AllServers.Add(new EachServer(uri_tmp, tmp_id) );
                }
                catch (UriFormatException e)
                {
                    Console.WriteLine("Invalid URL: {0}", words[1]);
                    //Console.ReadLine();
                    //return;
                }
            }
            //channel = new TcpChannel();
            //ChannelServices.RegisterChannel(channel, true);
            new Thread(() => PingLoop()).Start();

        }

        private void PingLoop()
        {
            while (true)
            {
                PingAllServers();
                Thread.Sleep(1000);
            }
        }

        private void PingAllServers()
        {
            
            if(AllServers.Count <= 1)
            {
                return;
            }

            for (int i = 0; i < AllServers.Count; i++)
            {
                if( AllServers[i].ID == My_Identification.ID) //Avoid ping to himself
                {
                    continue;
                }

                try
                {
                    
                    ServerService obj = (ServerService)Activator.GetObject(typeof(ServerService), AllServers[i].UID.AbsoluteUri + "MyRemoteObjectName");
                    obj.Ping();
                    Console.WriteLine("ALIVE: {0}", AllServers[i].UID.AbsoluteUri);

                }
                catch (Exception e)
                {
                    Console.WriteLine("DEAD: {0}", AllServers[i].UID.AbsoluteUri);
                    //Console.WriteLine(e);
                }
            }
            
        }
    }
}
