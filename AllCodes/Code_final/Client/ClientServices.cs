using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Projeto_DAD
{
    class ClientServices : MarshalByRefObject, IClientServices
    {
        public void Begin_Repeat(int repetitions, string command)
        {
            throw new NotImplementedException();
        }

        public void End_repeat()
        {
            throw new NotImplementedException();
        }

        public void Wait(int milliseconds)
        {
            Console.WriteLine("waiting " + milliseconds);
            Thread.Sleep(milliseconds);
        }
    }
}
