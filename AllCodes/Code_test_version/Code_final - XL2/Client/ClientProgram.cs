﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Remoting;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;

namespace Projeto_DAD
{

    public class ThreadWithState
    {
        // State information used in the task.
        private Command c;
        private int serverPos;
        private IServerServices ss;
        private bool FlagStop;

        public delegate bool MyDelegate(Command c);


        // The constructor obtains the state information.
        public ThreadWithState(Command comm, int sPos)
        {
            c = comm;
            serverPos = sPos;
            ss = (IServerServices)Activator.GetObject(typeof(IServerServices), ClientProgram.AllServers[serverPos] + "MyRemoteObjectName");
            FlagStop = false;
        }

        public void Kill_hread()
        {
            FlagStop = true;
        }

        // The thread procedure performs the task
        public void TX_Command_thread()
        {
            Console.WriteLine("comecei ");
            AsyncCallback cb = new AsyncCallback(MyCallBack);
            MyDelegate d = new MyDelegate(ss.RX_Command);
            while (!FlagStop)
            {
                Thread.Sleep(2000);//2 seconds
                try
                {
                    Console.WriteLine("Trying to connect to: " + ClientProgram.AllServers[serverPos]);

                    
                    IAsyncResult ar = d.BeginInvoke(c, cb, null);
                    
                    Console.WriteLine("Connected to :" + ClientProgram.AllServers[serverPos]);

                }
                catch (System.Net.Sockets.SocketException e)
                {
                    //Keep
                }
                //Thread.Sleep(60000);//2 seconds
                if (c.GetCommand().Equals("take") || c.GetCommand().Equals("remove"))
                {
                    //If the command is Take. Send only one
                    FlagStop = true;
                }
            }
            
            Console.WriteLine("Terminei de mandar para: " + ClientProgram.AllServers[serverPos]);
        }

        public static void MyCallBack(IAsyncResult ar)
        {
            MyDelegate d = (MyDelegate)((AsyncResult)ar).AsyncDelegate;
            d.EndInvoke(ar);
            //Console.WriteLine( "Ola" + );
            
        }
    }


    class ClientProgram
    {
        private IServerServices ss;
        private ClientServices cs;

        private int ServerToConnect; //Server to connect
        public static int SequenceNumber = 0; //Sequence number of the commands
        public static List<SenderPool> ThreadsInAction;

        public static ManualResetEvent Pending_SignalEvent = new ManualResetEvent(false); //Pending thread can start
        public static ManualResetEvent Take_SignalEvent = new ManualResetEvent(false); //Pending thread can start

        private static bool TerminateTakenActivity = false;

        public static List<string> AllServers;    //All servers present in the pool
        private const string path = "..\\..\\..\\Filedatabase\\database.txt"; //database of all servers
        private const int Timeout = 5000; //miliseconds

        private const int STATE_CLIENT_ROOT_DISCOVER = 0;
        private const int STATE_CLIENT_COMMAND_INTERPRETATION = 1;
        private const int STATE_CLIENT_COMMAND_CYCLE = 2;
        private const int STATE_CLIENT_COMMAND_EXECUTION = 3;
        private const int STATE_READ_CLIENT_SCRIPT = 4;

        private int STATE_CLIENT = STATE_CLIENT_ROOT_DISCOVER;


        private const int STATE_COMMAND = 0;
        private const int STATE_WAIT_FOR_REPLY_READ = 1;
        private const int STATE_WAIT_FOR_REPLY_ADD = 2;
        private const int STATE_WAIT_FOR_REPLY_TAKE = 3;

        private static int STATE_EXECUTE = STATE_COMMAND;
        private int TIMEOUT_FOR_READ = 5000; //5 s

        private bool START_CICLE = false;
        private int NumberOfCycles;
        private List<Command> CommandsInCycle = new List<Command>();

        private string[] results;
        private MyTuple tuple = null;

        public static Uri MyAddress;


        private string clientScript_path;
        private List<string> script_comands = new List<string>();



        public ClientProgram(Uri uri, String clientScript_path)
        {
            MyAddress = uri;
            cs = new ClientServices();
            AllServers = new List<string>();

            this.clientScript_path = clientScript_path;

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

            ThreadsInAction = new List<SenderPool>();
        }


        

        public static void FinnishTake()
        {
            if(STATE_EXECUTE==STATE_WAIT_FOR_REPLY_TAKE)
            {
                TerminateTakenActivity = true;
            }
        }
        
        /// <summary>
        /// Execute the command. Sends to Server
        /// </summary>
        /// <param name="command"></param>
        /// <param name="t"></param>
        public static void Execute(string command, MyTuple t)
        {
            Console.WriteLine("EXECUTAR");
            Command c = new Command(command, t, MyAddress, ++SequenceNumber);
            Stopwatch sw = new Stopwatch();

            while (true)
            {
                switch (STATE_EXECUTE)
                {
                    case STATE_COMMAND:
                        switch (command)
                        {
                            case "read":
                                //Console.WriteLine("========================================");
                                for (int i = 0; i < AllServers.Count; i++)
                                {
                                    ThreadWithState tws = new ThreadWithState(c, i);
                                    Thread td = new Thread(new ThreadStart(tws.TX_Command_thread));
                                    
                                    ThreadsInAction.Add( new SenderPool(tws, td, new Uri(AllServers[i]), c) );
                                    td.Start();
                                }
                                STATE_EXECUTE = STATE_WAIT_FOR_REPLY_READ;
                                break;
                            case "add":
                                Console.WriteLine("========================================");
                                for (int i = 0; i < AllServers.Count; i++)
                                {
                                    ThreadWithState tws = new ThreadWithState(c, i);
                                    Thread td = new Thread(new ThreadStart(tws.TX_Command_thread));

                                    ThreadsInAction.Add(new SenderPool(tws, td, new Uri(AllServers[i]), c));
                                    td.Start();
                                }
                                STATE_EXECUTE = STATE_WAIT_FOR_REPLY_ADD;
                                break;
                            case "take":  //FASE 1
                                Console.WriteLine("========================================");
                                for (int i = 0; i < AllServers.Count; i++)
                                {
                                    ThreadWithState tws = new ThreadWithState(c, i);
                                    Thread td = new Thread(new ThreadStart(tws.TX_Command_thread));

                                    ThreadsInAction.Add(new SenderPool(tws, td, new Uri(AllServers[i]), c));
                                    td.Start();
                                }
                                STATE_EXECUTE = STATE_WAIT_FOR_REPLY_TAKE;
                                break;
                            case "remove":  //FASE 2
                                Console.WriteLine("========================================");
                                for (int i = 0; i < AllServers.Count; i++)
                                {
                                    ThreadWithState tws = new ThreadWithState(c, i);
                                    Thread td = new Thread(new ThreadStart(tws.TX_Command_thread));

                                    ThreadsInAction.Add(new SenderPool(tws, td, new Uri(AllServers[i]), c));
                                    td.Start();
                                }
                                STATE_EXECUTE = STATE_WAIT_FOR_REPLY_TAKE;
                                break;

                        }
                        break;
                    case STATE_WAIT_FOR_REPLY_READ:
                        Pending_SignalEvent.WaitOne();
                        Pending_SignalEvent.Reset();
                        STATE_EXECUTE = STATE_COMMAND;               
                        return;
                    case STATE_WAIT_FOR_REPLY_ADD:
                        Pending_SignalEvent.WaitOne();
                        Pending_SignalEvent.Reset();
                        STATE_EXECUTE = STATE_COMMAND;
                        return;
                    case STATE_WAIT_FOR_REPLY_TAKE:
                        Console.WriteLine("============INICIO======================STATE_WAIT_FOR_REPLY_TAKE");

                        Pending_SignalEvent.WaitOne();
                        Pending_SignalEvent.Reset();

                        Console.WriteLine("============ENTREI======================STATE_WAIT_FOR_REPLY_TAKE: " + TerminateTakenActivity.ToString());
                        STATE_EXECUTE = STATE_COMMAND;
                        if (TerminateTakenActivity==true && command=="take")
                        {
                            //Console.WriteLine("============ACABEI======================STATE_WAIT_FOR_REPLY_TAKE");
                            TerminateTakenActivity = false;
                            command = "remove";
                            c = new Command(command, t, MyAddress, ++SequenceNumber);
                            //Take_SignalEvent.Set();
                            //return;
                        }
                        else
                        {
                            if ( TerminateTakenActivity == true && command == "remove")
                            {
                                TerminateTakenActivity = false;
                                return;
                            }
                        }
                        break;
                   

                }



                /*try
                {
                    Command c = new Command(command, t, MyAddress);
                    ss.RX_Command(c);
                    while (BlockUntilAnswer == false) ;
                    BlockUntilAnswer = false;
                    break;
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    Console.WriteLine("AQUI");
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    while (SearchForRootServer() == false) //Search for ROOT
                    {
                        if (sw.ElapsedMilliseconds > Timeout)
                        {
                            Console.WriteLine("No Server available.");
                            Console.WriteLine("Retrying in {0} [s]", Timeout);
                            Thread.Sleep(Timeout);
                            //break;
                        }
                    }
                }*/
            }
        }
        

        public void ClientStateMachine()
        {
            string command = "";

            while (true)
            {
                switch (STATE_CLIENT)
                {
                    case STATE_CLIENT_ROOT_DISCOVER:
                        if (SearchForRootServer() == true)
                        {
                            //new Thread(() => PingLoop()).Start(); SERA ***********************************
                            if (clientScript_path != null)
                                STATE_CLIENT = STATE_READ_CLIENT_SCRIPT;
                            else
                                STATE_CLIENT = STATE_CLIENT_COMMAND_INTERPRETATION;
                        }
                        break;
                    case STATE_READ_CLIENT_SCRIPT:          //Está martelado, fica assim, caso tenhamos tempo otimizo depois
                        /*script_comands = ReadScriptFile(clientScript_path);
                        foreach (string linha in script_comands)
                        {
                            results = linha.Split(' ');
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

                                switch (results[0])
                                {
                                    case "add":
                                        Execute("add", tuple);
                                        if (START_CICLE == true)
                                        {
                                            CommandsInCycle.Add(new Command("add", tuple, null));
                                        }
                                        break;
                                    case "read":
                                        Execute("read", tuple);
                                        if (START_CICLE == true)
                                        {
                                            CommandsInCycle.Add(new Command("read", tuple, null));
                                        }
                                        break;
                                    case "take":
                                        Execute("take", tuple);
                                        if (START_CICLE == true)
                                        {
                                            CommandsInCycle.Add(new Command("take", tuple, null));
                                        }
                                        break;
                                    case "wait":
                                        Thread.Sleep(Int32.Parse(results[1]));
                                        if (START_CICLE == true)
                                        {
                                            CommandsInCycle.Add(new Command("wait", results[1], null));
                                        }
                                        break;
                                    case "begin-repeat":
                                        START_CICLE = true;
                                        NumberOfCycles = Int32.Parse(results[1]);
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
                        }*/

                        STATE_CLIENT = STATE_CLIENT_COMMAND_INTERPRETATION;
                        break;
                    case STATE_CLIENT_COMMAND_INTERPRETATION:
                        Console.Write("Command: ");
                        command = Console.ReadLine();
                        //string[] results;
                        //MyTuple tuple = null;
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
                        STATE_CLIENT = STATE_CLIENT_COMMAND_EXECUTION;
                        break;
                    case STATE_CLIENT_COMMAND_EXECUTION:
                       
                        switch (results[0])
                        {
                            case "add":
                                Execute("add", tuple);
                                if (START_CICLE == true)
                                {
                                    CommandsInCycle.Add(new Command("add", tuple, null, -1));
                                }
                                STATE_CLIENT = STATE_CLIENT_COMMAND_INTERPRETATION;
                                break;
                            case "read":
                                Execute("read",tuple);
                                if (START_CICLE == true)
                                {
                                    CommandsInCycle.Add(new Command("read", tuple, null, -1));
                                }
                                STATE_CLIENT = STATE_CLIENT_COMMAND_INTERPRETATION;
                                break;
                            case "take":
                                Execute("take",tuple);
                                if (START_CICLE == true)
                                {
                                    CommandsInCycle.Add(new Command("take", tuple, null, -1));
                                }
                                STATE_CLIENT = STATE_CLIENT_COMMAND_INTERPRETATION;
                                break;
                            case "wait":
                                Thread.Sleep(Int32.Parse(results[1]));
                                if (START_CICLE == true)
                                {
                                    CommandsInCycle.Add(new Command("wait", results[1], null, -1));
                                }
                                STATE_CLIENT = STATE_CLIENT_COMMAND_INTERPRETATION;
                                break;
                            case "begin-repeat":
                                START_CICLE = true;
                                NumberOfCycles = Int32.Parse(results[1]);
                                STATE_CLIENT = STATE_CLIENT_COMMAND_INTERPRETATION;
                                break;
                            case "end-repeat":
                                START_CICLE = false;
                                --NumberOfCycles;
                                STATE_CLIENT = STATE_CLIENT_COMMAND_CYCLE;
                                break;
                            default:
                                Console.WriteLine("Wrong type of command!");
                                STATE_CLIENT = STATE_CLIENT_COMMAND_INTERPRETATION;
                                break;
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
                            for (int i = 0; i < CommandsInCycle.Count; ++i)
                            {
                                switch (CommandsInCycle[i].GetCommand())
                                {
                                    case "read":
                                        Console.WriteLine("READ");

                                        Execute("read", (MyTuple)CommandsInCycle[i].GetPayload());
                                        break;
                                    case "add":
                                        Console.WriteLine("ADD");

                                        Execute("add", (MyTuple)CommandsInCycle[i].GetPayload());
                                        break;
                                    case "take":
                                        Console.WriteLine("TAKE");
                                        Execute("take", (MyTuple)CommandsInCycle[i].GetPayload());
                                        break;
                                    case "wait":
                                        Console.WriteLine("WAIT");

                                        Thread.Sleep(Int32.Parse((string)CommandsInCycle[i].GetPayload()));
                                        break;
                                }
                            }
                        }
                        break;
                }

            }


        }

        public List<string> ReadScriptFile(string filePath)
        {
            List<string> commandList = new List<string>();

            //Scanner for .txt file
            const Int32 BufferSize = 128;
            using (var fileStream = File.OpenRead(filePath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                String line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    commandList.Add(line);
                }

            }

            return commandList;
        }


        private static List<string> FiltroInput(string text)
        {
            List<string> myList = new List<string>();


            Regex rx = new Regex(@"(""\**\w*\**"")|(DADTestA(\(\d+,""\w+""\))*)|(DADTestB(\(\d+,""\w+"",\d+\))*)|(DADTestC(\(\d+,""\w+"",""\w+""\))*)|(null)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Find matches.
            MatchCollection matches = rx.Matches(text);
          
            // Report on each match.          
            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;

                myList.Add(groups[0].ToString());
            }

            //Console.WriteLine("Encontrei {0}", idx.Length);

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




        private bool SearchForRootServer()
        {
            /*Random r = new Random();
            ServerToConnect = r.Next(0, AllServers.Count); //connect to random server in pool

            int i = 0;
            while (i < AllServers.Count)
            {
                
                try
                {
                    Console.WriteLine("Trying to connect to: " + AllServers[ServerToConnect]);
                    ss = (IServerServices)Activator.GetObject(typeof(IServerServices), AllServers[i] + "MyRemoteObjectName");
                    ss.Ping();
                    Console.WriteLine("Connected to :" + AllServers[ServerToConnect]);

                    RemotingServices.Marshal(cs, "MyRemoteObjectName", typeof(ClientServices));
                    break;
                    

                }
                catch (System.Net.Sockets.SocketException e)
                {
                    Console.WriteLine("The server: " + AllServers[ServerToConnect] + " is not available");
                    i = (i + 1) % AllServers.Count;
                    if (i == 0)
                    {
                        return false;
                    }
                    ServerToConnect = (ServerToConnect + 1) % AllServers.Count;
                }
            }*/
            return true;
        }


        private bool IsRootAlive()
        {
            //ping to root
            try
            {
                //ss.Ping();
                //Console.WriteLine("ROOT IS ALIVE");
                return true;
            }
            catch (Exception e)
            {
                //Console.WriteLine("DEAD: {0}", Server.AllServers[i].UID.AbsoluteUri);
                Console.WriteLine();
                //Console.WriteLine("ROOT IS DEAD, PRESS <ENTER> TO SEARCH FOR A NEW SERVER");
                //Console.WriteLine(e);
                RemotingServices.Disconnect(cs);
                //STATE_CLIENT = STATE_CLIENT_ROOT_DISCOVER;
                //SearchForRootServer(); //Falta implementar um timeout Provavelmente
                //break;
            }
            return false;
        }

        /*private void PingLoop()
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
        }*/
    }

}

