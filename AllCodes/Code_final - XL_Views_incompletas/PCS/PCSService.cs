using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    class PCSService: MarshalByRefObject, IPCSServices
    {
        private Dictionary<ProcessSupport, Process> ServerProcess = new Dictionary<ProcessSupport, Process>();
        private Dictionary<ProcessSupport, Process> ClientProcess = new Dictionary<ProcessSupport, Process>();

        public const string serverPath = "..\\..\\..\\Server\\bin\\Debug\\Server.exe";
        public const string clientPath = "..\\..\\..\\Client\\bin\\Debug\\Client.exe";


        public void StartServer(string id, string url, string min_delay, string max_delay)
        {
            Console.WriteLine("# StartServer:");

            string serverID = id; //s1
            ProcessSupport ProcessServer = new ProcessSupport(serverID, url);

            if (ServerProcess.ContainsKey(ProcessServer))
            {
                if (ServerProcess[ProcessServer].HasExited) //If process is terminated
                {
                    ServerProcess.Remove(ProcessServer);
                }
            }
            if (!ServerProcess.ContainsKey(ProcessServer))
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = serverPath;
                    p.StartInfo.Arguments = serverID + " " + url + " " + min_delay + " " + max_delay;
                    p.Start();
                    ServerProcess.Add(ProcessServer, p);
                    Console.WriteLine("Starting the server");

                }
                catch (InvalidOperationException) { Console.WriteLine("FileName specified is not valid"); }
                catch (Win32Exception e) { Console.WriteLine("Couldn't Initialize the Server"); Console.WriteLine(e); }
            }
            else
                Console.WriteLine("\nThe pid specified already exists : {0}", serverID);
        }



        public void StartClient(string id, string url, string script_file)
        {
            Console.WriteLine("# StartClient:");

            string clientID = id; //c1
            ProcessSupport ProcessClient = new ProcessSupport(clientID, url);

            if (ClientProcess.ContainsKey(ProcessClient))
            {
                if (ClientProcess[ProcessClient].HasExited)
                {
                    ClientProcess.Remove(ProcessClient);
                }
            }
            if (ClientProcess.ContainsKey(ProcessClient) ==false)
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = clientPath;

                    p.StartInfo.Arguments = clientID + " " + url + " " + script_file;
                    p.Start();
                    ClientProcess.Add(ProcessClient, p);

                }
                catch (InvalidOperationException) { Console.WriteLine("FileName specified is not valid"); }
                catch (Win32Exception) { Console.WriteLine("Couldn't Initialize the Client"); }
            }
            else
                Console.WriteLine("\nThe pid specified already exists : {0}", clientID);

        }

        
        public void Status()
        {
            Console.WriteLine("Status");
            Console.WriteLine("Number of Servers Started: " + ServerProcess.Count);

            int Count_terminated = 0;
            int Count_on = 0;

            List<string> alive = new List<string>();
            List<string> dead = new List<string>();

            foreach (KeyValuePair<ProcessSupport, Process> kvp in ServerProcess)
            {
                if (kvp.Value.HasExited)
                {
                    ++Count_terminated;
                    dead.Add(kvp.Key.GetProcessname());
                }
                else
                {
                    ++Count_on;
                    alive.Add(kvp.Key.GetProcessname());
                }
            }
            Console.WriteLine("Number of Servers Alive: " + Count_on);
            foreach(string s in alive)
            {
                Console.Write(s + "  ");
            }
            Console.WriteLine();

            Console.WriteLine("Number of Servers Dead: " + Count_terminated);
            foreach (string s in dead)
            {
                Console.Write(s + "  ");
            }

            Console.WriteLine();


            Console.WriteLine("Number of Clients Started: " + ClientProcess.Count);

            Count_terminated = 0;
            Count_on = 0;
            alive.Clear();
            dead.Clear();

            foreach (KeyValuePair<ProcessSupport, Process> kvp in ClientProcess)
            {
                if (kvp.Value.HasExited)
                {
                    ++Count_terminated;
                    dead.Add(kvp.Key.GetProcessname());
                }
                else
                {
                    ++Count_on;
                    alive.Add(kvp.Key.GetProcessname());
                }
            }
            Console.WriteLine("Number of Clients Alive: " + Count_on);
            foreach (string s in alive)
            {
                Console.Write(s + "  ");
            }
            Console.WriteLine();

            Console.WriteLine("Number of Clients Dead: " + Count_terminated);
            foreach (string s in dead)
            {
                Console.Write(s + "  ");
            }

            Console.WriteLine();
        }


        public void Crash(string processname)
        {
            Process p;
            ProcessSupport ProcessInUse = new ProcessSupport(processname, "tcp:\\localhost:9000"); //dummyaddress
            try
            {
                if (processname[0] == 's') //is server?
                {
                    p = ServerProcess[ProcessInUse];
                    ServerProcess.Remove(ProcessInUse);
                }
                else {
                    if ( (processname[0] == 'c') ) //is client?
                    {
                        p = ClientProcess[ProcessInUse];
                        ClientProcess.Remove(ProcessInUse);
                    }
                    else
                    {
                        return;
                    }
                }

                if (p.HasExited == false)
                {
                    p.Kill(); //crash the process
                }

            }
            catch (Exception e)
            {
                return;
            }
            Console.WriteLine("Process " + processname + "Sucessfully crashed");

        }

        
        public void Freeze(string processname)
        {
            Uri uri = FindProcessAddress(processname, processname[0]); //Get address of the process

            if (processname[0] == 's')
            {
                IServerServices obj = (IServerServices)Activator.GetObject(typeof(IServerServices), uri.AbsoluteUri + "MyRemoteObjectName"); //Send msg to Server to freeze
                obj.freeze();
            }
        }

        public void Unfreeze(string processname)
        {
            Uri uri = FindProcessAddress(processname, processname[0]); //Get address of the process

            if (processname[0] == 's')
            {
                IServerServices obj = (IServerServices)Activator.GetObject(typeof(IServerServices), uri.AbsoluteUri + "MyRemoteObjectName"); //Send msg to Server to freeze
                obj.unfreeze();
            }
        }


        /// <summary>
        /// Search Process
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Uri FindProcessAddress(string id, char type)
        {
            if (type == 's')
            {
                foreach (KeyValuePair<ProcessSupport, Process> kvp in ServerProcess)
                {
                    if( kvp.Key.Equals(id)==true )
                    {
                        return kvp.Key.GetUri();
                    }
                }
            }
            if (type == 'c')
            {
                foreach (KeyValuePair<ProcessSupport, Process> kvp in ClientProcess)
                {
                    if (kvp.Key.Equals(id) == true)
                    {
                        return kvp.Key.GetUri();
                    }
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Class to correlate Process_id and Url in dictionary
    /// To use in Freeze and Unfreeze
    /// </summary>
    class ProcessSupport
    {
        private string Processname;
        private Uri uri;

        public ProcessSupport(string PID, string url)
        {
            Processname = PID;
            
            try
            {
                uri = new Uri(url);
            }
            catch (UriFormatException e)
            {
                Console.WriteLine("------ProcessSupport-------- Invalid URL: {0}", url);
                uri = null;
                return;
            }
        }

        public string GetProcessname()
        {
            return Processname;
        }

        public Uri GetUri()
        {
            return uri;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            ProcessSupport t = obj as ProcessSupport;
            if (t == null)
            {
                return false;
            }
            return Processname.Equals(t.GetProcessname());
        }
        public override int GetHashCode()
        {
            return Processname.GetHashCode();
        }
    }
}
