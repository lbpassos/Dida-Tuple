using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Projeto_DAD
{
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

        private List<DADTestA> dADTestA = new List<DADTestA>();
        private List<DADTestB> dADTestB = new List<DADTestB>();
        private List<DADTestC> dADTestC = new List<DADTestC>();

        private static bool Root = false;
        private bool Repeat = false;

        public static void setRoot(bool value)
        {
            Root = value;
        }

        //Implement Interface IServerToServer
        public void Ping()
        {
            return;
        }
        public bool isRoot()
        {
            return Root;
        }

        public void Add(int i1, string s1)
        {
            dADTestA.Add(new DADTestA(i1, s1));
        }

        public void Add(int i1, string s1, int i2)
        {
            dADTestB.Add(new DADTestB(i1, s1, i2));
        }

        public void Add(int i1, string s1, string s2)
        {
            dADTestC.Add(new DADTestC(i1, s1, s2));
        }

        public void Wait(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        public void End_repeat()
        {
           Repeat = false;
        }

        //TODO

        public Tuple<int, string> Read(Predicate<Tuple<int, string>> tuple)
        {
            throw new NotImplementedException();
        }

        public Tuple<int, string> Take(Predicate<Tuple<int, string>> tuple)
        {
            throw new NotImplementedException();
        }

        public void Begin_Repeat(int repetitions, string command)
        {
            Repeat = true;
            while(Repeat == true)
            {
                for (int i = 0; i < repetitions; i++)
                {
                    //command;
                }
            }
        }
    }
}
