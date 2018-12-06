using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    

    class SenderPool
    {
        private Thread t;
        private Command c;

        public SenderPool(Thread thr, Command com)
        {
            t = thr;
            c = com;
        }

        public Thread GetThread()
        {
            return t;
        }

        public Command GetCommand()
        {
            return c;
        }

        public override bool Equals(object obj)
        {
            SenderPool a = obj as SenderPool;
            if (a == null)
            {
                return false;
            }
            if(t.Equals(a.GetThread())==true && c.Equals(a.GetCommand()) == true)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return t.GetHashCode() + c.GetHashCode();
        }
    }
}
