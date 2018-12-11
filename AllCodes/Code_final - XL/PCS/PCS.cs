using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.ComponentModel;
//using services;
using System.Threading;
using System.Collections;

namespace Projeto_DAD
{
    class PCS
    {
        private const int port = 10000;
       
        static void Main(string[] args)
        {

            IDictionary RemoteChannelProperties = new Hashtable();
            RemoteChannelProperties["port"] = port.ToString();
            RemoteChannelProperties["name"] = "tcp1";

            TcpServerChannel channel = new TcpServerChannel(RemoteChannelProperties, null, null);//
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(PCSService),
                "MyRemoteObjectName",
                WellKnownObjectMode.Singleton);
                
            //new Thread(() => Client_thread()).Start();
            //new Thread(() => Service_thread()).Start();

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }


        public static void Service_thread()
        {

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(PCSService),
                "MyRemoteObjectName",
                WellKnownObjectMode.Singleton);

            //while (true) ;
            Thread.Sleep(Timeout.Infinite);
        }

       
    }
    
    






}