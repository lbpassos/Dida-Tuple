using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;


namespace Projeto_DAD
{
    class Server
    {
        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(8089);
            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerServices),
                "ServerServices",
                WellKnownObjectMode.Singleton);

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }

    class ServerServices : MarshalByRefObject, IServerServices
    {
        private List<Tuple<int, string>> LocalTuples = new List<Tuple<int, string>>();

        public void Add(Tuple<int,string> tuple)
        {
            LocalTuples.Add(tuple);
        }

        public Tuple<int, string> Read(Predicate<Tuple<int, string>> tuple)
        {
            return LocalTuples.Find(tuple);
        }

        public Tuple<int, string> Take(Predicate<Tuple<int, string>> tuple)
        {
            Tuple<int, string> returner = LocalTuples.Find(tuple);
            LocalTuples.Remove(returner);
            return returner;
        }

        public void Wait(int milliseconds)
        {
            throw new NotImplementedException();
        }

        public void Begin_Repeat(int repetitions)
        {
            throw new NotImplementedException();
        }

        public void End_repeat()
        {
            throw new NotImplementedException();
        }
    }
}
