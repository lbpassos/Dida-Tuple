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

                    TcpChannel channel = new TcpChannel();
                    ChannelServices.RegisterChannel(channel, false);
            //Tentar ligar-se a todos os servidores
            foreach (string servidor in AllServers)
            {
                try
                {
                    Console.WriteLine("Trying to connect to: " + servidor + "MyRemoteObject");
                    ss = (IServerServices)Activator.GetObject(typeof(IServerServices), servidor + "MyRemoteObjectName");
                    RemotingServices.Marshal(new ClientServices(), "MyRemoteObjectName", typeof(ClientServices));
                    
                    if (ss.isRoot() == true)
                    {
                        RootServer = servidor;
                        Console.WriteLine(RootServer + " is the ROOT Server");
                        Console.WriteLine("Connected to :" + servidor);
                        break;
                    }

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
                string[] results = CheckCommands(command).Split(' ');
                if (results[0] == "add")
                {
                    if (results[1] == "DADTestA") ; //ss.Add(Int32.Parse(resultados[2]), resultados[3]);
                    if (results[1] == "DADTestB") ; //ss.Add(Int32.Parse(resultados[2]), resultados[3], Int32.Parse(resultados[3]));
                    if (results[1] == "DADTestC") ; //ss.Add(Int32.Parse(resultados[2]), resultados[3], resultados[2]));
                }
                else if (results[0] == "read")
                {
                    //TODO
                }
                else if (results[0] == "take")
                {
                    //TODO
                }
                else if (results[0] == "wait")
                {
                    ss.Wait(Int32.Parse(results[1]));
                }
                else if (results[0] == "Begin-repeat")
                {
                    //TODO
                }
                else if (results[0] == "End-repeat")
                {
                    ss.End_repeat();
                }
            }
        }

        public static string CheckCommands(string command)
        {
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
                        return ("add DADTestA " + variaveis[0] + " " + variaveis[1]);
                        //Console.WriteLine("DADTestA - " + variaveis[0] + " - " + variaveis[1]);
                    }
                    else if (funcao[0] == "DADTestB")
                    {
                        funcao[1] = funcao[1].Trim(new Char[] { '<', '>', '\"', '\"', '(', ')' });
                        string[] variaveis = funcao[1].Split(',');
                        variaveis[1] = variaveis[1].Substring(1);
                        variaveis[1] = variaveis[1].Remove(variaveis[1].Length - 1);
                        return ("add DADTestB " + variaveis[0] + " " + variaveis[1] + " " + variaveis[2]);
                        //Console.WriteLine("DADTestB - " + variaveis[0] + " - " + variaveis[1] + " - " + variaveis[2]);
                    }
                    else if (funcao[0] == "DADTestC")
                    {
                        funcao[1] = funcao[1].Trim(new Char[] { '<', '>', '\"', '\"', '(', ')' });
                        string[] variaveis = funcao[1].Split(',');
                        variaveis[1] = variaveis[1].Substring(1);
                        variaveis[1] = variaveis[1].Remove(variaveis[1].Length - 1);
                        variaveis[2] = variaveis[2].Substring(1);
                        return ("add DADTestC " + variaveis[0] + " " + variaveis[1] + " " + variaveis[2]);
                        //Console.WriteLine("DADTestC - " + variaveis[0] + " - " + variaveis[1] + " - " + variaveis[2]);
                    }
                }
                else if (words[0] == "Begin-repeat")
                {
                    
                    
                }
                else if (words[0] == "wait")
                {
                    return ("wait " + words[1]);
                    //Console.WriteLine("Wait - " + words[1]);
                }
            return null;
        }

    }
}
