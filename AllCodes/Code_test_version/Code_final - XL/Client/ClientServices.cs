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
        private bool flag = false;

        public void sink(Command mt) //Receive answers
        {

            while (MustFreeze == true) ; //Freeze


            switch (mt.GetCommand())
            {
                case "read":
                    for (int i = 0; i < ClientProgram.ThreadsInAction.Count; ++i)
                    {
                        if (ClientProgram.ThreadsInAction[i].Equals(new SenderPool(null, null, mt.GetUriFromSender(), mt) ) == true)
                        {
                            ClientProgram.ThreadsInAction[i].GetThreadState().Kill_hread();
                            ClientProgram.ThreadsInAction[i].GetThread().Join();
                            ClientProgram.ThreadsInAction.RemoveAt(i);
                            ClientProgram.Pending_SignalEvent.Set();
                            Console.WriteLine("(ClientServices) RECEBI: " + mt.GetPayload().ToString());
                            flag = true;
                            break;
                        }
                    }
                    if (flag == true)
                    {
                        for (int i = 0; i < ClientProgram.ThreadsInAction.Count; ++i)
                        {
                            ClientProgram.ThreadsInAction[i].GetThreadState().Kill_hread();
                            ClientProgram.ThreadsInAction[i].GetThread().Join();
                            flag = false; ;
                        }
                        ClientProgram.ThreadsInAction.Clear();
                    }
                    break;
            }
            


            
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
