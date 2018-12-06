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
        private static bool flag = false;
        

        private static int NumOfAddReceived = 0; //Number of Add received
        private static int TotalNum;

        private static CommunicationLayer commLayer = new CommunicationLayer();


        private static bool AlreadyEvolvedFromAdd = false; //if N/2+1 ADD Ack received

        public void sink(Command mt) //Receive answers
        {

            while (MustFreeze == true) ; //Freeze

            commLayer.InsertCommand(mt);
            
        }

        public static void ProcessReceivedReplys_thread()
        {

            while (true)
            {
                Thread.Sleep(50);//Min time to check commands
                if (commLayer.GetQueueSize() > 0) //if there is commands
                {
                    Command mt = commLayer.RemoveFromCommandQueue();
                    MyTuple payload = (MyTuple)mt.GetPayload();
                    Object tmp;

                    switch (mt.GetCommand())
                    {
                        case "read":
                            for (int i = 0; i < ClientProgram.ThreadsInAction.Count; ++i)
                            {
                                if (ClientProgram.ThreadsInAction[i].Equals(new SenderPool(null, null, mt.GetUriFromSender(), mt)) == true)
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
                        case "add":
                            for (int i = 0; i < ClientProgram.ThreadsInAction.Count; ++i)
                            {
                                if (ClientProgram.ThreadsInAction[i].Equals(new SenderPool(null, null, mt.GetUriFromSender(), mt)) == true) //Se está OK
                                {
                                    //ClientProgram.ThreadsInAction[i].GetThreadState().Kill_hread();
                                    //ClientProgram.ThreadsInAction[i].GetThread().Join();
                                    //ClientProgram.ThreadsInAction.RemoveAt(i);


                                    if (++NumOfAddReceived == ClientProgram.AllServers.Count)
                                    {
                                        NumOfAddReceived = 0;
                                        for (int j = 0; j < ClientProgram.ThreadsInAction.Count; ++j)
                                        {
                                            ClientProgram.ThreadsInAction[j].GetThreadState().Kill_hread();
                                            ClientProgram.ThreadsInAction[j].GetThread().Join();
                                            flag = false; ;
                                        }
                                        ClientProgram.ThreadsInAction.Clear(); //todos recebidos
                                    }

                                    if (AlreadyEvolvedFromAdd == false)
                                    {
                                        flag = true;
                                    }
                                    break;
                                }
                            }
                            if (flag == true)
                            {
                                if (NumOfAddReceived >= (ClientProgram.AllServers.Count / 2 + 1))
                                {
                                    AlreadyEvolvedFromAdd = true;
                                    ClientProgram.Pending_SignalEvent.Set(); //Signal UpperLayer that it can evolve
                                    Console.WriteLine("(ClientServices) ADD Success in the majority of replicas: ");
                                    flag = true;

                                    //Majority of ADD Acknowledge received
                                }
                            }
                            break;

                    }
                }
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
