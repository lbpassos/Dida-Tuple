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
