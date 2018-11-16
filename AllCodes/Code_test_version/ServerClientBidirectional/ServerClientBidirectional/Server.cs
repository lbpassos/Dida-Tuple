using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ServerClientBidirectional
{
    class Server
    {
        public Server(int port)
        {
            RegisterChannel(port);

        }

        
        

        private void RegisterChannel(int port)
        {
            // Sink 
            BinaryServerFormatterSinkProvider serverFormatter = new BinaryServerFormatterSinkProvider(); //canal de retorno
            serverFormatter.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
            

            Hashtable ht = new Hashtable();
            ht["name"] = "ServerChannel";
            ht["port"] = port;

            TcpChannel channel = new TcpChannel(ht, null, serverFormatter); //sink channel
            ChannelServices.RegisterChannel(channel, true);


            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerTalk),
                "MyRemoteObject",
                WellKnownObjectMode.Singleton);

           

            Thread t = new Thread( new ThreadStart(CheckClientToServerQueue) );
            t.Start();
        }

        
        private void CheckClientToServerQueue()
        {
            while (true)
            {
                Thread.Sleep(50);   
                if ( ServerTalk.HasCommands()==true )
                {
                    ServerTalk.SendToClient();
                    
                }
            }
        }

        



    }
}
