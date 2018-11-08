﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    public interface IServerServices
    {
        void Add(int i1, string s1); //Adds a DADTestA

        void Add(int i1, string s1, int i2);    //Adds a DADTestB

        void Add(int i1, string s1, string s2);    //Adds a DADTestC

        Tuple<int, string> Read(Predicate<Tuple<int, string>> tuple);  //???????????

        Tuple<int, string> Take(Predicate<Tuple<int, string>> tuple); // ??????????????????

        void Wait(int milliseconds); //Delays the execution of the next command for x milliseconds.

        void Begin_Repeat(int repetitions, string command); //Repeats x number of times the commands following this command and before the next end-repeat.It is not possible to have another begin-repeat command before this loop is closed by a end-repeat command.

        void End_repeat(); //Closes a repeat loop.

        //To Finish?

        bool isRoot(); //check if node is root
        void Ping();

    }

    public interface IClientServices
    {
        //TODO
    }

    public interface IPuppetMasterServices
    {
        //TODO
    }

    public interface IPCSServices
    {
        //TODO
    }

    public class MyRemoteObject : MarshalByRefObject
    {
        private Dictionary<string, Process> processes = new Dictionary<string, Process>();
        public const string serverPath = "..\\..\\..\\ConsoleApp1\\bin\\Debug\\ConsoleApp1.exe";
        public const string clientPath = "..\\..\\..\\Client\\bin\\Debug\\Client.exe";

        public void StartServer(string id, string min_delay, string max_delay)
        {
            Console.WriteLine("# StartServer:");

            if (processes.ContainsKey(id))
            {
                if (processes[id].HasExited) //If process is terminated
                {
                    processes.Remove(id);
                    //urlByPid.Remove(id);
                }
            }
            if (!processes.ContainsKey(id))
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = serverPath;

                    p.Start();
                    processes.Add(id, p);

                }
                catch (InvalidOperationException) { Console.WriteLine("FileName specified is not valid"); }
                catch (Win32Exception e) { Console.WriteLine("Couldn't Initialize the Server"); Console.WriteLine(e); }
            }
            else
                Console.WriteLine("\nThe pid specified already exists : {0}", id);
        }

        public void StartClient(string id, string msec, string script_filel)
        {
            Console.WriteLine("# StartClient:");


            if (processes.ContainsKey(id))
                if (processes[id].HasExited)
                {
                    processes.Remove(id);
                }
            if (!processes.ContainsKey(id))
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = clientPath;


                    p.Start();
                    processes.Add(id, p);

                }
                catch (InvalidOperationException) { Console.WriteLine("FileName specified is not valid"); }
                catch (Win32Exception) { Console.WriteLine("Couldn't Initialize the Client"); }
            }
            else
                Console.WriteLine("\nThe pid specified already exists : {0}", id);
        }
    }


}
