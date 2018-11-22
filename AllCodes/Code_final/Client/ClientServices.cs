using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Projeto_DAD
{
    class ClientServices : MarshalByRefObject, IClientServices
    {
        public void sink(MyTuple mt)
        {
            
            if (mt != null)
            {
                Console.WriteLine("(ClientServices) RECEBI: " + mt.ToString());
            }
            else
            {
                Console.WriteLine("(ClientServices) NADA A RECEBER");
            }
            ClientProgram.AnswerIsReceived();
        }
    }
}
