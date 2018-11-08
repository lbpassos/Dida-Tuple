using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace Projeto_DAD
{
    class Client
    {
        private static IServerServices ss;
        private static string RootServer;
        public static List<string> AllServers;    //All servers present in the pool
        private const string path = "..\\..\\..\\Filedatabase\\database.txt"; //database of all servers

        static void Main(string[] args)
        {

            //Ler a lista de todos os servidores
            AllServers = new List<string>();
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');
                try
                {
                    AllServers.Add(words[1]);
                }
                catch (UriFormatException e)
                {
                    Console.WriteLine("Invalid URL: {0}", words[1]);
                    return;
                }
            }

            //Tentar ligar-se a todos os servidores
            foreach (string servidor in AllServers)
            {
                try
                {
                    Console.WriteLine("Trying to connect to :" + servidor);
                    TcpChannel channel = new TcpChannel();
                    ChannelServices.RegisterChannel(channel, false);
                    ss = (IServerServices)Activator.GetObject(typeof(IServerServices), servidor);
                    RemotingServices.Marshal(new ClientServices(), "MyRemoteObject", typeof(ClientServices));
                    //if (ss.isRoot() == true)
                    //{
                    //    RootServer = servidor;
                    //    Console.WriteLine(RootServer + " is the ROOT Server");
                    //    Console.WriteLine("Connected to :" + servidor);
                    break;
                    //}

                }
                catch (System.Net.Sockets.SocketException e)
                {
                    Console.WriteLine("The server: " + servidor + " is not available");
                }

            }
            while (true)
            {
                Console.WriteLine("Command: ");
                string command = Console.ReadLine();
                string[] words = command.Split(' ');
                if (words[0] == "add")
                {
                    string[] funcao = words[1].Split('(');
                    funcao[0] = funcao[0].Substring(1);
                    if (funcao[0] == "DADTestA")
                    {
                        funcao[1] = funcao[1].Trim(new Char[] { '<', '>', '\"', '\"', '(', ')' });
                        string[] variaveis = funcao[1].Split(',');
                        variaveis[1] = variaveis[1].Substring(1);
                        //Console.WriteLine("DADTestA - " + variaveis[0] + " - " + variaveis[1]);
                        //ss.Add(Int32.Parse(variaveis[0]), variaveis[1]);
                    }
                    else if (funcao[0] == "DADTestB")
                    {
                        funcao[1] = funcao[1].Trim(new Char[] { '<', '>', '\"', '\"', '(', ')' });
                        string[] variaveis = funcao[1].Split(',');
                        variaveis[1] = variaveis[1].Substring(1);
                        variaveis[1] = variaveis[1].Remove(variaveis[1].Length - 1);
                        //Console.WriteLine("DADTestB - " + variaveis[0] + " - " + variaveis[1] + " - " + variaveis[2]);
                        //ss.Add(Int32.Parse(variaveis[0]), variaveis[1], Int32.Parse(variaveis[2]));
                    }
                    else if (funcao[0] == "DADTestC")
                    {
                        funcao[1] = funcao[1].Trim(new Char[] { '<', '>', '\"', '\"', '(', ')' });
                        string[] variaveis = funcao[1].Split(',');
                        variaveis[1] = variaveis[1].Substring(1);
                        variaveis[1] = variaveis[1].Remove(variaveis[1].Length - 1);
                        variaveis[2] = variaveis[2].Substring(1);
                        //Console.WriteLine("DADTestC - " + variaveis[0] + " - " + variaveis[1] + " - " + variaveis[2]);
                        //ss.Add(Int32.Parse(variaveis[0]), variaveis[1], variaveis[2]));
                    }
                }
            }
        }
    }
}
