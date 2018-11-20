using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Projeto_DAD
{
    public interface IServerServices
    {
        void Add(MyTuple mt);

        object Read(MyTuple mt);

        object Take(MyTuple mt);

        bool isRoot(); //check if node is root

        void Ping();
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




    /* ===============================================================================================================
     *                            Supported Objects in the Tuple Space 
     * ===============================================================================================================
     */                           
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
        public override bool Equals(object o)
        {

            if (o == null)
            {
                return true;
            }

            string t = o as string;
            if (t != null)
            {
                if (t.Equals("DADTestA"))
                {
                    return true;
                }
            }


            DADTestA a = o as DADTestA;
            if (a == null)
            {
                return false;
            }
            else
            {
                return ((this.i1 == a.i1) && (this.s1.Equals(a.s1)));
            }
        }
        public override int GetHashCode()
        {
            return i1.GetHashCode() + s1.GetHashCode();
        }

        public override string ToString()
        {
            return "DADTestA(" + i1 + "," + s1 + ")";
        }


    }

    [Serializable]
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

        public override bool Equals(object o)
        {

            if (o == null)
            {
                return true;
            }

            string t = o as string;
            if (t != null)
            {
                if (t.Equals("DADTestB"))
                {
                    return true;
                }
            }


            DADTestB b = o as DADTestB;
            if (b == null)
            {
                return false;
            }
            else
            {
                return ((this.i1 == b.i1) && (this.s1.Equals(b.s1)) && (this.i2 == b.i2));
            }
        }
        public override int GetHashCode()
        {
            return i1.GetHashCode() + s1.GetHashCode() + i2.GetHashCode();
        }

        public override string ToString()
        {
            return "DADTestB(" + i1 + "," + s1 + "," + "\"" + i2 + ")";
        }

    }

    [Serializable]
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

        public override bool Equals(object o)
        {
            if (o == null)
            {
                return true;
            }

            string t = o as string;
            if (t != null)
            {
                if (t.Equals("DADTestC"))
                {
                    return true;
                }
            }


            DADTestC c = o as DADTestC;
            if (c == null)
            {
                return false;
            }
            else
            {
                return ((this.i1 == c.i1) && (this.s1.Equals(c.s1)) && (this.s2.Equals(c.s2)));
            }
        }

        public override int GetHashCode()
        {
            return i1.GetHashCode() + s1.GetHashCode() + s2.GetHashCode();
        }

        public override string ToString()
        {
            return "DADTestC(" + i1 + "," + s1 + "," + s2 + ")";
        }

    }

    [Serializable]
    public class StringEmulator
    {
        public string s1;


        public StringEmulator(string ps1)
        {

            s1 = ps1;

        }

        public int GetSize()
        {
            return s1.Length;
        }

        public string GetString()
        {
            return s1;
        }
        public override bool Equals(object o)
        {
            StringEmulator a = o as StringEmulator;
            if (a == null)
            {
                return false;
            }
            else
            {
                if (this.s1.Equals(a.s1) == true)
                {
                    return true;
                }
                else
                {
                    string pattern = "";

                    //Console.WriteLine("DEBUG: {0}", a.GetString());
                    if (a.GetString()[0] == '*') //In the beginning
                    {
                        if (a.GetSize() == 1) //all
                        {
                            return true;
                        }
                        else
                        {
                            pattern += a.GetString().Substring(1) + "$";
                            Regex rx = new Regex(@pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                            MatchCollection matches = rx.Matches(s1);
                            if (matches.Count > 0)
                            {
                                return true;
                            }

                        }
                    }
                    if (a.GetString()[a.GetSize() - 1] == '*') //In the end
                    {
                        pattern += "^" + a.GetString().Substring(0, a.GetSize() - 2);
                        Regex rx = new Regex(@pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        MatchCollection matches = rx.Matches(s1);
                        if (matches.Count > 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }
        public override int GetHashCode()
        {
            return s1.GetHashCode();
        }

        public override string ToString()
        {
            return "\"" + s1 + "\"";
        }

    }


    /* ===============================================================================================================
     *                            Tuple Representation
     * ===============================================================================================================
     */
    [Serializable]
    public class MyTuple
    {
        List<object> ObjTuple;

        public MyTuple(object[] obj)
        {
            ObjTuple = new List<object>();

            for (int i = 0; i < obj.Length; ++i)
            {
                ObjTuple.Add(obj[i]);
            }
        }


        public int GetSize()
        {
            return ObjTuple.Count;
        }

        public object GetValue(int pos)
        {
            return ObjTuple[pos];
        }



        /// <summary>
        /// Completely Equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            MyTuple a = obj as MyTuple;
            if (a == null || this.GetSize() != a.GetSize())
            {
                return false;
            }
            else
            {
                for (int i = 0; i < this.GetSize(); ++i)
                {
                    if (this.GetValue(i).Equals(a.GetValue(i)))
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {

            string tmp = "<";

            for (int i = 0; i < ObjTuple.Count; ++i)
            {
                try
                {
                    tmp += ObjTuple[i].ToString();
                }
                catch (NullReferenceException e)
                {
                    tmp += "null";
                }

                if (i + 1 < ObjTuple.Count)
                {
                    tmp += ",";
                }
            }
            return tmp + ">";
        }


    }

    [Serializable]
    public class Command
    {
        private string cmd;
        private object payload;

        public Command(string command, object tuple)
        {
            cmd = command;
            payload = tuple;
        }

        public string GetCommand()
        {
            return cmd;
        }

        public object GetPayload()
        {
            return payload;
        }
    }
}
