using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Projeto_DAD
{
    class ClientServices : MarshalByRefObject, IClientServices
    {
        private bool MustFreeze = false;

        public void sink(MyTuple mt) //Receive answers
        {

            while (MustFreeze == true) ; //Freeze

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

        public void freeze()
        {
            MustFreeze = true;
        }

        public void unfreeze()
        {
            MustFreeze = false;
        }

    }

   
}
