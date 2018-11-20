using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Text.RegularExpressions;

namespace Projeto_DAD
{
    class Client
    {
        private static IServerServices ss;
        private static ClientServices cs = new ClientServices();
        private static string RootServer;
        public static List<string> AllServers;    //All servers present in the pool
        private const string path = "..\\..\\..\\Filedatabase\\database.txt"; //database of all servers

        private const int STATE_CLIENT_ROOT_DISCOVER = 0;
        private const int STATE_CLIENT_COMMAND_INTERPRETATION = 1;
        private const int STATE_CLIENT_COMMAND_CYCLE = 2;

        private static int STATE_CLIENT = STATE_CLIENT_ROOT_DISCOVER;

        private static bool START_CICLE = false;
        private static int NumberOfCycles;
        private static List<Command> CommandsInCycle = new List<Command>();



       
        /* =========================================================================
                                AINDA SEM CALLBACK
           =========================================================================
        */
        private static void ADD_Execute(MyTuple t)
        {
            ss.Add(t);
        }
        private static void READ_Execute(MyTuple t)
        {
            Console.WriteLine("TUPLO LIDO: " + ss.Read(t) );
        }
        private static void TAKE_Execute(MyTuple t)
        {
            Console.WriteLine("TUPLO REMOVIDO: " + ss.Take(t));
        }




        static void Main(string[] args)
        {
            string command = "";

            int port = Int32.Parse(args[0]);
           // CallbackModule cm = new CallbackModule(port); //Init CallBack Module ********************************


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
                    case STATE_CLIENT_ROOT_DISCOVER:
                        if (SearchForRootServer() == true)
                        {
                            new Thread(() => PingLoop()).Start();
                            STATE_CLIENT = STATE_CLIENT_COMMAND_INTERPRETATION;
                        }
                        break;
                    case STATE_CLIENT_COMMAND_INTERPRETATION:
                        Console.Write("Command: ");
                        command = Console.ReadLine();
                        string[] results;
                        MyTuple tuple = null;
                        try
                        {
                            results = command.Split(' ');
                            if (results[0].Equals("add") || results[0].Equals("read") || results[0].Equals("take"))
                            {
                                command = command.Replace(">", "");
                                command = command.Replace("<", "");


                                List<string> tupleArguments = FiltroInput(results[1]);

                                object[] tupleOBJ = new object[tupleArguments.Count];
                                for (int i = 0; i < tupleArguments.Count; ++i)
                                {
                                    tupleOBJ[i] = GetObjectFromString(tupleArguments[i]);
                                }
                                tuple = new MyTuple(tupleOBJ);

                            }

                            switch( results[0] )
                            {
                                case "add":
                                    ADD_Execute(tuple);
                                    if (START_CICLE == true)
                                    {
                                        CommandsInCycle.Add(new Command("add", tuple));
                                    }
                                    break;
                                case "read":
                                    READ_Execute(tuple);
                                    if (START_CICLE == true)
                                    {
                                        CommandsInCycle.Add(new Command("read", tuple));
                                    }
                                    break;
                                case "take":
                                    TAKE_Execute(tuple);
                                    if (START_CICLE == true)
                                    {
                                        CommandsInCycle.Add(new Command("take", tuple));
                                    }
                                    break;
                                case "wait":
                                    Thread.Sleep( Int32.Parse(results[1]) );
                                    if (START_CICLE == true)
                                    {
                                        CommandsInCycle.Add(new Command("wait", results[1]));
                                    }  
                                    break;
                                case "begin-repeat":
                                    START_CICLE = true;
                                    NumberOfCycles = Int32.Parse( results[1] );
                                    break;
                                case "end-repeat":
                                    START_CICLE = false;
                                    --NumberOfCycles;
                                    STATE_CLIENT = STATE_CLIENT_COMMAND_CYCLE;
                                    break;
                                default:
                                    Console.WriteLine("Wrong type of command!");
                                    break;

                            }
                            
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Console.WriteLine("Wrong type of command!");
                            //continue;
                        }
                        break;
                    case STATE_CLIENT_COMMAND_CYCLE:
                        Console.WriteLine();
                        Console.WriteLine("Inicio da {0} iteração", NumberOfCycles);

                        if (NumberOfCycles-- == 0)
                        {
                            CommandsInCycle.Clear();
                            STATE_CLIENT = STATE_CLIENT_COMMAND_INTERPRETATION;
                        }
                        else
                        {
                            for(int i=0; i<CommandsInCycle.Count; ++i)
                            {
                                switch (CommandsInCycle[i].GetCommand() )
                                {
                                    case "read":
                                        Console.WriteLine("READ");

                                        READ_Execute( (MyTuple)CommandsInCycle[i].GetPayload() );
                                        break;
                                    case "add":
                                        Console.WriteLine("ADD");

                                        ADD_Execute( (MyTuple)CommandsInCycle[i].GetPayload() );
                                        break;
                                    case "take":
                                        Console.WriteLine("TAKE");
                                        TAKE_Execute((MyTuple)CommandsInCycle[i].GetPayload() );
                                        break;
                                    case "wait":
                                        Console.WriteLine("WAIT");

                                        Thread.Sleep( Int32.Parse((string)CommandsInCycle[i].GetPayload()) );
                                        break;
                                }
                            }
                        }
                        break;
                }

            }


        }



        private static List<string> FiltroInput(string text)
        {
            List<string> myList = new List<string>();


            Regex rx = new Regex(@"(""\**\w*\**"")|(DADTestA(\(\d+,""\w+""\))*)|(DADTestB(\(\d+,""\w+"",\d+\))*)|(DADTestC(\(\d+,""\w+"",""\w+""\))*)|(null)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Find matches.
            MatchCollection matches = rx.Matches(text);

            //Indices
            int[] idx = new int[matches.Count];

            // Report on each match.
            int i = 0;
            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;

                //idx[i++] = groups[0].Index;
                myList.Add( groups[0].ToString() );


            }

            Console.WriteLine("Encontrei {0}", idx.Length);

            return myList;

        }

        /// <summary>
        /// s is without <>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static object GetObjectFromString(string s)
        {
            string obj_stringName;
            string obj_stringValues;
            string[] results;

            if (s[0] == '\"') //Its a string
            {
                s = s.Replace("\"", "");
                return new StringEmulator(s);
            }
            else
            {
                int idx = s.IndexOf('('); // Test if Don´t have ( 
                if (idx != -1)   //so its an Object
                {
                    obj_stringName = s.Substring(0, idx);
                    obj_stringValues = s.Substring(idx + 1, (s.Length - 1) - (idx + 1));
                    results = obj_stringValues.Split(',');
                }
                else
                {
                    if (s.Equals("null")) // its a wildcard for objects (null matches everything)
                    {
                        return null;
                    }
                    else
                    {
                        return s; // its a wildcard (word) 
                    }

                }
                switch (obj_stringName)
                {
                    case "DADTestA":
                        return new DADTestA(Int32.Parse(results[0]), results[1]);
                    case "DADTestB":
                        return new DADTestB(Int32.Parse(results[0]), results[1], Int32.Parse(results[2]));
                    case "DADTestC":
                        return new DADTestC(Int32.Parse(results[0]), results[1], results[2]);
                    default:
                        throw new Exception("Invalid Command: GetObjectFromString");
                }
            }
        }




        private static bool SearchForRootServer()
        {
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
                        //new Thread(() => PingLoop()).Start();
                        //STATE_CLIENT = STATE_CLIENT_COMMAND_INTERPRETATION;
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
            return true;

        }



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
                    //Console.WriteLine("ROOT IS DEAD, PRESS <ENTER> TO SEARCH FOR A NEW SERVER");
                    //Console.WriteLine(e);
                    RemotingServices.Disconnect(cs);
                    //STATE_CLIENT = STATE_CLIENT_ROOT_DISCOVER;
                    SearchForRootServer(); //Falta implementar um timeout Provavelmente
                    break;
                }

                Thread.Sleep(1000);
            }
        }
    }
}
