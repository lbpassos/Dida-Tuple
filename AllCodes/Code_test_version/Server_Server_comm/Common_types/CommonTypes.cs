using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_types
{
    // Server service to other servers
    public interface IServerServices
    {
        bool isRoot(); //check if node is root
        void Ping();
        Object getImage();
        
    }


    //Client service to servers
    public interface IClientServices
    {
        //Object Read(int idx);
        //void add(string s);
        //void take();
        Object commandInterpreter(Command c);

    }

    [Serializable]
    public class Command
    {
        private string cmd;
        private Object payload;

        public Command(string c, Object o)
        {
            cmd = c;
            payload = o;
        }

        public string getCommand()
        {
            return cmd;
        }

        public Object getPayload()
        {
            return payload;
        }
    }


    [Serializable]
    public class Image
    {
        private List<string> s;

        public Image()
        {
            s = new List<string>();
        }

        public string Read(int i)
        {
            return s[i];
        }

        public void Add(string str)
        {
            s.Add(str);
        }

        public void Take(string str)
        {
            s.Remove(str);
        }

        public override string ToString()
        {
            string res = "";
            foreach (string i in s)
            {
                res += i + ", ";
            }

            return res;
        }

    }


}
