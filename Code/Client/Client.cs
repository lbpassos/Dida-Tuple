using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace Projeto_DAD
{
    class Client
    {

        static void Main(string[] args)
        {
            //connect
            TcpChannel channel = new TcpChannel(8087);
            ChannelServices.RegisterChannel(channel, false);
            IServerServices ss = (IServerServices)Activator.GetObject(typeof(IServerServices), "tcp://localhost:8089/Project");
            RemotingServices.Marshal(new ClientServices(), "MCM", typeof(ClientServices));
        }
    }

    class ClientServices: MarshalByRefObject, IClientServices
    {
        //TODO
    }



}
