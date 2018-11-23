using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsInDictionary
{
    class Program
    {

        static void Main(string[] args)
        {
            Dictionary<ProcessSupport, string> ClientProcess = new Dictionary<ProcessSupport, string>();

            ProcessSupport ps1 = new ProcessSupport("um", "tcp:\\localhost:9000");
            ProcessSupport ps2 = new ProcessSupport("dois", "tcp:\\localhost:9001");
            ProcessSupport ps3 = new ProcessSupport("tres", null);

            ClientProcess.Add(ps1, "A");
            ClientProcess.Add(ps2, "B");
            ClientProcess.Add(ps3, "C");

            try
            {
                Console.WriteLine("RES: " + ClientProcess[ps1]);
                ClientProcess.Remove(ps2);
                Console.WriteLine("RES: " + ClientProcess[ps1]);

                if (ClientProcess.ContainsKey(ps3) )
                {
                    Console.WriteLine("Tenho: " + ClientProcess[ps3]);
                }

                Console.ReadLine();
            }
            catch (Exception)
            {
                Console.WriteLine("Não Existe");
                Console.ReadLine();
            }
        }
    }

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
