using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Projeto_DAD
{
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

        public MyObject Read(string key, string tipo, string si1, string s1, string si2, string s2)
        {
            if (tipo == "DADTestA")
            {
                MyObject objA = new MyObject();
                foreach (Dictionary<string, DADTestA> dic in ListDADTestA)
                {
                    if (dic.ContainsKey(key) && si1 == null && s1 == null)              //O client NÃO sabe o valor dos dados
                    {
                        Console.WriteLine(dic[key].ToString());
                        objA._objsA = dic[key];
                        return objA;
                    }
                    else if (dic.ContainsKey(key) && int.Parse(si1) == dic[key].i1 && s1.Equals(dic[key].s1))     //O client SABE o valor dos dados
                    {
                        Console.WriteLine(dic[key].ToString());
                        objA._objsA = dic[key];
                        return objA;
                    }
                }
                return objA;
            }
            else if (tipo == "DADTestB")
            {
                MyObject objB = new MyObject();
                foreach (Dictionary<string, DADTestB> dic in ListDADTestB)
                {
                    if (dic.ContainsKey(key) && si1 == null && s1 == null && si2 == null)              //O client NÃO sabe o valor dos dados
                    {
                        Console.WriteLine(dic[key].ToString());
                        objB._objsB = dic[key];
                        return objB;
                    }
                    else if (dic.ContainsKey(key) && int.Parse(si1) == dic[key].i1 && s1.Equals(dic[key].s1) && int.Parse(si2) == dic[key].i2)     //O client SABE o valor dos dados
                    {
                        Console.WriteLine(dic[key].ToString());
                        objB._objsB = dic[key];
                        return objB;
                    }
                }
                return objB;
            }
            else if (tipo == "DADTestC")
            {
                MyObject objC = new MyObject();
                foreach (Dictionary<string, DADTestC> dic in ListDADTestC)
                {
                    if (dic.ContainsKey(key) && si1 == null && s1 == null && s2 == null)              //O client NÃO sabe o valor dos dados
                    {
                        Console.WriteLine(dic[key].ToString());
                        objC._objsC = dic[key];
                        return objC;
                    }
                    else if (dic.ContainsKey(key) && int.Parse(si1) == dic[key].i1 && s1.Equals(dic[key].s1) && s2.Equals(dic[key].s2))     //O client SABE o valor dos dados
                    {
                        Console.WriteLine(dic[key].ToString());
                        objC._objsC = dic[key];
                        return objC;
                    }
                }
                return objC;
            }
            return null;
        }

        //public Tuple<int, string> Take(Predicate<Tuple<int, string>> tuple)
        //{
        //    throw new NotImplementedException();
        //}

    }

}
