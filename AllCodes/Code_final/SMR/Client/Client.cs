﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace Projeto_DAD
{
    class Client
    {
        private static IServerServices ss;
        private static ClientServices cs = new ClientServices();
        private static string RootServer;
        public static List<string> AllServers;    //All servers present in the pool
        private const string path = "..\\..\\..\\Filedatabase\\database.txt"; //database of all servers

        private const int STATE_CLIENT_DISCOVER = 0;
        private const int STATE_CLIENT_INTERACT = 1;

        private static int STATE_CLIENT = STATE_CLIENT_DISCOVER;


        static void Main(string[] args)
        {
            string command = "";
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
            //Tentar ligar-se ao server ROOT
            while (true)
            {
                switch (STATE_CLIENT)
                {
                    case STATE_CLIENT_DISCOVER:
                        int i = 0;
                        while (i < AllServers.Count)
                        {
                            try
                            {
                                Console.WriteLine("Trying to connect to: " + AllServers[i]);
                                ss = (IServerServices)Activator.GetObject(typeof(IServerServices), AllServers[i] + "MyRemoteObjectName");
                                if (ss.isRoot() == true)
                                {
                                    Console.WriteLine("Connected to :" + AllServers[i]);
                                    RootServer = AllServers[i];
                                    Console.WriteLine(RootServer + " is the ROOT Server");
                                    RemotingServices.Marshal(cs, "MyRemoteObjectName", typeof(ClientServices));
                                    new Thread(() => PingLoop()).Start();
                                    STATE_CLIENT = STATE_CLIENT_INTERACT;
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("Connected to :" + AllServers[i]);
                                    Console.WriteLine(AllServers[i] + "is NOT the ROOT Server");
                                }

                            }
                            catch (System.Net.Sockets.SocketException e)
                            {
                                Console.WriteLine("The server: " + AllServers[i] + " is not available");
                            }

                            i = (i + 1) % AllServers.Count;
                        }
                        break;
                    case STATE_CLIENT_INTERACT:
                        Console.Write("Command: ");
                        command = Console.ReadLine();
                        string[] results;
                        try
                        {
                            //results = CheckCommands(command).Split(' ');
                            command = command.Replace("<", "");
                            command = command.Replace(">", "");
                            results = command.Split(' ');
                            results[1] = results[1].Replace(",D", " D");
                            results[1] = results[1].Replace("),", ") ");
                            Console.WriteLine( results[0] + " " + results[1]);
                            //if (results[0] == "add")
                            //{
                            //    if (results[2] == "DADTestA") ss.Add(results[1], Int32.Parse(results[3]), results[4]);
                            //    if (results[2] == "DADTestB") ss.Add(results[1], Int32.Parse(results[3]), results[4], Int32.Parse(results[5]));
                            //    if (results[2] == "DADTestC") ss.Add(results[1], Int32.Parse(results[3]), results[4], results[5]);
                            //}
                            //else if (results[0] == "read")
                            //{
                            //    //Console.WriteLine(string.Join(" ", results));
                            //    if (results.Length <= 3)
                            //    {
                            //        ss.Read(results[1], results[2], null, null, null, null);
                            //    }
                            //    else
                            //    {
                            //        if (results[2] == "DADTestA")
                            //        {
                            //            ss.Read(results[1], results[2], results[3], results[4], null, null);
                            //        }
                            //        else if (results[2] == "DADTestB")
                            //        {
                            //            ss.Read(results[1], results[2], results[3], results[4], results[5], null);
                            //        }
                            //        else if (results[2] == "DADTestC")
                            //        {
                            //            ss.Read(results[1], results[2], results[3], results[4], null, results[5]);
                            //        }
                            //    }
                            //}
                            //else if (results[0] == "take")
                            //{
                            //    //TODO
                            //}
                            //else if (results[0] == "wait")
                            //{
                            //    cs.Wait(int.Parse(results[1]));
                            //}
                            //else if (results[0] == "ShowA")
                            //{
                            //    ss.ShowA();
                            //}
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Console.WriteLine("Wrong type of command!");
                            //continue;
                        }
                        break;
                }

            }


        }

        //public static string CheckCommands(string command)
        //{
        //    string[] words = command.Split(' ');
        //    if (words[0] == "add")
        //    {
        //        words[1] = words[1].Replace(',', ' ');
        //        words[1] = words[1].Replace('(', ' ');
        //        words[1] = words[1].Trim(new Char[] { '<', '>', ')', '\"' });
        //        words[1] = words[1].Replace("\"", "");
        //        string[] variaveis = words[1].Split(' ');
        //        if (variaveis[1] == "DADTestA")
        //        {
        //            return (words[0] + " " + variaveis[0] + " " + variaveis[1] + " " + variaveis[2] + " " + variaveis[3]);
        //        }
        //        else if (variaveis[1] == "DADTestB" || variaveis[1] == "DADTestC")
        //        {
        //            return (words[0] + " " + variaveis[0] + " " + variaveis[1] + " " + variaveis[2] + " " + variaveis[3] + " " + variaveis[4]);
        //        }
        //    }
        //    else if(words[0] == "read")
        //    {
        //        words[1] = words[1].Replace(',', ' ');
        //        words[1] = words[1].Replace('(', ' ');
        //        words[1] = words[1].Trim(new Char[] { '<', '>', ')' });
        //        words[1] = words[1].Replace("\"", "");
        //        string[] variaveis = words[1].Split(' ');
        //        if (variaveis.Length <= 2)
        //        {
        //            //Console.WriteLine(string.Join(" ", variaveis));
        //            return (words[0] + " " + variaveis[0] + " " + variaveis[1]);
        //        }
        //        else
        //        {
        //            if (variaveis[1] == "DADTestA")
        //            {
        //                return (words[0] + " " + variaveis[0] + " " + variaveis[1] + " " + variaveis[2] + " " + variaveis[3]);
        //            }
        //            else if (variaveis[1] == "DADTestB" || variaveis[1] == "DADTestC")
        //            {
        //                return (words[0] + " " + variaveis[0] + " " + variaveis[1] + " " + variaveis[2] + " " + variaveis[3] + " " + variaveis[4]);
        //            }
        //        }
        //    }
        //    else if (words[0] == "wait")
        //    {
        //        return (words[0] + words[1]);
        //    }
        //    else if (words[0] == "ShowA")
        //    {
        //        return words[0];
        //    } 
        //    return null;
        //}

        private static void PingLoop()
        {
            while (true)
            {
                //ping to root
                try
                {
                    //ServerService obj = (ServerService)Activator.GetObject(typeof(ServerService), Server.AllServers[i].UID.AbsoluteUri + "MyRemoteObjectName");

                    //ss = (IServerServices)Activator.GetObject(typeof(IServerServices), AllServers[Int32.Parse(RootServer)-1] + "MyRemoteObjectName");
                    ss.Ping();
                    //Console.WriteLine("ROOT IS ALIVE");
                }
                catch (Exception e)
                {
                    //Console.WriteLine("DEAD: {0}", Server.AllServers[i].UID.AbsoluteUri);
                    Console.WriteLine();
                    Console.WriteLine("ROOT IS DEAD, PRESS <ENTER> TO SEARCH FOR A NEW SERVER");
                    //Console.WriteLine(e);
                    RemotingServices.Disconnect(cs);
                    STATE_CLIENT = STATE_CLIENT_DISCOVER;
                    break;
                }

                Thread.Sleep(1000);
            }
        }
    }
}