using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Projeto_DAD
{
    class ServerService : MarshalByRefObject, IServerServices
    {

        private List<object> Tuple_Space = new List<object>();

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

        public void Add(object tuple)
        {
            Tuple_Space.Add(tuple);
        }

        public object Read(object tuple)
        {
            foreach (object temp in Tuple_Space)
            {
                if (temp.Equals(tuple))
                {
                    return temp;
                }
            }

            return null;
        }

        public object Take(object tuple)
        {
            throw new NotImplementedException();
        }

    }

}
