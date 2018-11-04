/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
*/

//Echo Client
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Projeto_DAD
{
    class Client
    {
        //Echo Client
        //********************************************************************
        static void Main(string[] args)   //START
        {
            Console.WriteLine("Starting echo client...");

            int port = 1234;
            TcpClient client = new TcpClient("localhost", port);
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

            while (true)
            {
                Console.Write("Enter to send: ");
                string lineToSend = Console.ReadLine();
                Console.WriteLine("Sending to server: " + lineToSend);
                writer.WriteLine(lineToSend);
                string lineReceived = reader.ReadLine();
                Console.WriteLine("Received from server: " + lineReceived);
            }
        }// END
        //********************************************************************
    }

    /*static void Main(string[] args)
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
}*/



}