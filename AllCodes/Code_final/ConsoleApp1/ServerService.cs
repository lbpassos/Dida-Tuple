using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Projeto_DAD
{
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


    class ServerService : MarshalByRefObject, IServerServices
    {
        private List<Dictionary<string, DADTestA>> ListDADTestA = new List<Dictionary<string, DADTestA>>();
        private List<Dictionary<string, DADTestB>> ListDADTestB = new List<Dictionary<string, DADTestB>>();
        private List<Dictionary<string, DADTestC>> ListDADTestC = new List<Dictionary<string, DADTestC>>();

        private static bool Root = false;
        private bool Repeat = false;

        public static void setRoot(bool value)
        {
            Root = value;
        }

        public void Ping()
        {
            return;
        }
        public bool isRoot()
        {
            return Root;
        }

        public void Add(string key, int i1, string s1)
        {
            Dictionary<string, DADTestA> dic = new Dictionary<string, DADTestA>();
            dic.Add(key, new DADTestA(i1, s1));
            ListDADTestA.Add(dic);
        }

        public void Add(string key, int i1, string s1, int i2)
        {
            Dictionary<string, DADTestB> dic = new Dictionary<string, DADTestB>();
            dic.Add(key, new DADTestB(i1, s1, i2));
            ListDADTestB.Add(dic);
        }

        public void Add(string key, int i1, string s1, string s2)
        {
            Dictionary<string, DADTestC> dic = new Dictionary<string, DADTestC>();
            dic.Add(key, new DADTestC(i1, s1, s2));
            ListDADTestC.Add(dic);
        }

        public void ShowA()
        {
            foreach (Dictionary<string, DADTestA> dic in ListDADTestA)
            {
                Console.WriteLine(dic.First());
            }
        }

        //TODO

        public void Read(string key)
        {
            MyObjectA objA = new MyObjectA();
            foreach(Dictionary<string, DADTestA> dic in ListDADTestA)
            {
                if(dic.ContainsKey(key))
                {
                    Console.WriteLine(dic[key].i1 + " " + dic[key].s1);
                    //objA._objsA.Add(dic[key]);
                }
            }
        }

        public Tuple<int, string> Take(Predicate<Tuple<int, string>> tuple)
        {
            throw new NotImplementedException();
        }

    }

    [Serializable]
    public class MyObjectA
    {
        public List<DADTestA> _objsA;
    }
}
