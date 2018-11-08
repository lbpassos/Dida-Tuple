using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    class ServerService : MarshalByRefObject, IServerServices
    {

        private static bool Root = false;

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

        public Tuple<int, string> Read(Predicate<Tuple<int, string>> tuple)
        {
            throw new NotImplementedException();
        }

        public Tuple<int, string> Take(Predicate<Tuple<int, string>> tuple)
        {
            throw new NotImplementedException();
        }

        public void Wait(int milliseconds)
        {
            throw new NotImplementedException();
        }

        public void Begin_Repeat(int repetitions)
        {
            throw new NotImplementedException();
        }

        public void End_repeat()
        {
            throw new NotImplementedException();
        }

        public void Add(int i1, string s1)
        {
            throw new NotImplementedException();
        }

        public void Add(int i1, string s1, int i2)
        {
            throw new NotImplementedException();
        }

        public void Add(int i1, string s1, string s2)
        {
            throw new NotImplementedException();
        }
    }
}
