using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    public class MyRemoteObject : MarshalByRefObject
    {

        private Dictionary<string, Process> processes = new Dictionary<string, Process>();
        public const string serverPath = "..\\..\\..\\ConsoleApp1\\bin\\Debug\\ConsoleApp1.exe";
        //private Dictionary<string, string> urlByPid = new Dictionary<string, string>();
        //private Dictionary<string, ISlaveControl> slaveByPid = new Dictionary<string, ISlaveControl>();

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
                    // endpoint msec numPlayers 
                    //p.StartInfo.Arguments = server_url + " " + msec + " " + num_players;
                    p.Start();
                    processes.Add(id, p);
                    //urlByPid.Add(id, url); slaveByPid.Add(pid, (ISlaveControl)Activator.GetObject(typeof(ISlaveControl), urlByPid[pid]));
                }
                catch (InvalidOperationException) { Console.WriteLine("FileName specified is not valid"); }
                catch (Win32Exception e) { Console.WriteLine("Couldn't Initialize the Server"); Console.WriteLine(e); }
            }
            else
                Console.WriteLine("\nThe pid specified already exists : {0}", id);
        }
    }
}

