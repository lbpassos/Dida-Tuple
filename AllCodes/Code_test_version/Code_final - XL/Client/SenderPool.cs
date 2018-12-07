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
        private ThreadWithState ts;
        private Thread t;
        private Command c;
        private Uri destination;

        public SenderPool(ThreadWithState thrS, Thread thr, Uri dest, Command com)
        {
            ts = thrS;
            t = thr;
            c = com;
            destination = dest;
        }

        public Thread GetThread()
        {
            return t;
        }
        public ThreadWithState GetThreadState()
        {
            return ts;
        }

        public Command GetCommand()
        {
            return c;
        }

        public Uri GetUri()
        {
            return destination;
        }

        /// <summary>
        /// Parameters of comparison: destination URI e nome do comando
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            SenderPool a = obj as SenderPool;
            if (a == null)
            {
                return false;
            }
            if( (destination.Equals(a.GetCommand().GetUriFromSender())==true) && (c.GetCommand().Equals(a.GetCommand().GetPrevCommand())==true) )
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return destination.GetHashCode() + c.GetCommand().GetHashCode();
        }
    }
}
