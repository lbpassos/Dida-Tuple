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
    class Program
    {
        private const int port = 1000;
       
        static void Main(string[] args)
        {
            

            

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);    

            

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(MyRemoteObject),
                "MyRemoteObjectName",
                WellKnownObjectMode.Singleton);

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
    
    






}