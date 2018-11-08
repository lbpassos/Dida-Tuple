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

        public void Add(Tuple<int, string> tuple)
        {
            throw new NotImplementedException();
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
    }
}
