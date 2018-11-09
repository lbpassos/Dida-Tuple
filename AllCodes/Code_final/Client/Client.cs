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
                    Console.WriteLine("Trying to connect to: " + servidor);
                    ss = (IServerServices)Activator.GetObject(typeof(IServerServices), servidor + "MyRemoteObjectName");
                    
                    if (ss.isRoot() == true)
                    {
                        Console.WriteLine("Connected to :" + servidor);
                        RootServer = servidor;
                        Console.WriteLine(RootServer + " is the ROOT Server");
                        RemotingServices.Marshal(new ClientServices(), "MyRemoteObjectName", typeof(ClientServices));
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Connected to :" + servidor);
                        Console.WriteLine(servidor + "is NOT the ROOT Server");
                    }

                }
                catch (System.Net.Sockets.SocketException e)
                {
                    Console.WriteLine("The server: " + servidor + " is not available");
                }

            }

            while (true)
            {
                Console.Write("Command: ");
                string command = Console.ReadLine();
                string[] results;
                try {
                    results = CheckCommands(command).Split(' ');
                }
                catch(Exception e)
                {
                    Console.WriteLine("Wrong type of command!");
                    continue;
                }
                if (results[0] == "add")
                {
                    if (results[2] == "DADTestA") ss.Add(results[1] ,Int32.Parse(results[3]), results[4]);
                    if (results[2] == "DADTestB") ss.Add(results[1], Int32.Parse(results[4]), results[4], Int32.Parse(results[5]));
                    if (results[2] == "DADTestC") ss.Add(results[1], Int32.Parse(results[3]), results[4], results[5]);
                }
                else if (results[0] == "read")
                {
                    //Console.WriteLine(string.Join(" ", results));
                    if(results.Length <= 3)
                    {
                        ss.Read(1, results[1], results[2], null, null, null, null);
                    }
                    else
                    {
                        if (results[2] == "DADTestA")
                        {
                            ss.Read(0, results[1], results[2], results[3], results[4], null, null);
                        }
                        else if (results[2] == "DADTestB")
                        {
                            ss.Read(0, results[1], results[2], results[3], results[4], results[5], null);
                        }
                        else if (results[2] == "DADTestC")
                        {
                            ss.Read(0, results[1], results[2], results[3], results[4], null, results[5]);
                        }
                    }
                }
                else if (results[0] == "take")
                {
                    //TODO
                }
                else if(results[0] == "ShowA")
                {
                    ss.ShowA();
                }
            }
        }

        public static string CheckCommands(string command)
        {
            string[] words = command.Split(' ');
            if (words[0] == "add")
            {
                words[1] = words[1].Replace(',', ' ');
                words[1] = words[1].Replace('(', ' ');
                words[1] = words[1].Trim(new Char[] { '<', '>', ')', '\"' });
                words[1] = words[1].Replace("\"", "");
                string[] variaveis = words[1].Split(' ');
                if (variaveis[1] == "DADTestA")
                {
                    return (words[0] + " " + variaveis[0] + " " + variaveis[1] + " " + variaveis[2] + " " + variaveis[3]);
                }
                else if (variaveis[1] == "DADTestB" || variaveis[1] == "DADTestC")
                {
                    return (words[0] + " " + variaveis[0] + " " + variaveis[1] + " " + variaveis[2] + " " + variaveis[3] + " " + variaveis[4]);
                }
            }
            else if(words[0] == "read")
            {
                words[1] = words[1].Replace(',', ' ');
                words[1] = words[1].Replace('(', ' ');
                words[1] = words[1].Trim(new Char[] { '<', '>', ')' });
                words[1] = words[1].Replace("\"", "");
                string[] variaveis = words[1].Split(' ');
                if (variaveis.Length <= 2)
                {
                    //Console.WriteLine(string.Join(" ", variaveis));
                    return (words[0] + " " + variaveis[0] + " " + variaveis[1]);
                }
                else
                {
                    if (variaveis[1] == "DADTestA")
                    {
                        return (words[0] + " " + variaveis[0] + " " + variaveis[1] + " " + variaveis[2] + " " + variaveis[3]);
                    }
                    else if (variaveis[1] == "DADTestB" || variaveis[1] == "DADTestC")
                    {
                        return (words[0] + " " + variaveis[0] + " " + variaveis[1] + " " + variaveis[2] + " " + variaveis[3] + " " + variaveis[4]);
                    }
                }
            }
            else if (words[0] == "ShowA")
            {
                return words[0];
            }
            return null;
        }

    }
}
