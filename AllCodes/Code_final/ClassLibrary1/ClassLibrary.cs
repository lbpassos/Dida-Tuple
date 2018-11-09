using System;
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
        void Add(string key, int i1, string s1);                //Adds a DADTestA

        void Add(string key, int i1, string s1, int i2);        //Adds a DADTestB

        void Add(string key, int i1, string s1, string s2);     //Adds a DADTestC

        MyObject Read(int target, string key, string tipo, string i1, string s1, string i2, string s2);                //Reads all objects of a type from the key

        //Tuple<int, string> Take(Predicate<Tuple<int, string>> tuple); // ??????????????????

        bool isRoot(); //check if node is root
        void Ping();

        void ShowA();

    }



    public interface IClientServices
    {
        void Wait(int milliseconds); //Delays the execution of the next command for x milliseconds.

        void Begin_Repeat(int repetitions, string command); //Repeats x number of times the commands following this command and before the next end-repeat.It is not possible to have another begin-repeat command before this loop is closed by a end-repeat command.

        void End_repeat(); //Closes a repeat loop.
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


    [Serializable]
    public class MyObject
    {
        public List<DADTestA> _objsA;
        public List<DADTestB> _objsB;
        public List<DADTestC> _objsC;
    }

    [Serializable]
    public class DADTestA
    {
        public int i1;
        public string s1;

        public DADTestA(int pi1, string ps1)
        {
            i1 = pi1;
            s1 = ps1;
        }
        public bool Equals(DADTestA o)
        {
            if (o == null)
            {
                return false;
            }
            else
            {
                return ((this.i1 == o.i1) && (this.s1.Equals(o.s1)));
            }
        }
    }

    public class DADTestB
    {
        public int i1;
        public string s1;
        public int i2;

        public DADTestB(int pi1, string ps1, int pi2)
        {
            i1 = pi1;
            s1 = ps1;
            i2 = pi2;
        }

        public bool Equals(DADTestB o)
        {
            if (o == null)
            {
                return false;
            }
            else
            {
                return ((this.i1 == o.i1) && (this.s1.Equals(o.s1)) && (this.i2 == o.i2));
            }
        }
    }

    public class DADTestC
    {
        public int i1;
        public string s1;
        public string s2;

        public DADTestC(int pi1, string ps1, string ps2)
        {
            i1 = pi1;
            s1 = ps1;
            s2 = ps2;
        }

        public bool Equals(DADTestC o)
        {
            if (o == null)
            {
                return false;
            }
            else
            {
                return ((this.i1 == o.i1) && (this.s1.Equals(o.s1)) && (this.s2.Equals(o.s2)));
            }
        }
    }

}
