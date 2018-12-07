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
        private static int NumOfRefusedReceived = 0;
        private static int NumOfAcceptedReceived = 0;
        private static int NumOfAckReceived = 0;
        private static List<Command> AcceptedReply = new List<Command>(); //All acepted messages

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
                    //MyTuple payload = (MyTuple)mt.GetPayload();
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
                                    flag = false;

                                    //Majority of ADD Acknowledge received
                                }
                            }
                            break;
                        case "refuse":
                            ++NumOfRefusedReceived;
                            if( (NumOfRefusedReceived+NumOfAcceptedReceived) == ClientProgram.AllServers.Count)
                            {
                                //At least One refused. Start al over again
                                NumOfRefusedReceived = 0;
                                NumOfAcceptedReceived = 0;
                                ClientProgram.Pending_SignalEvent.Set();
                            }
                            break;
                        case "accept":
                            ++NumOfAcceptedReceived;
                            AcceptedReply.Add(mt);
                            if ( NumOfAcceptedReceived== ClientProgram.AllServers.Count)
                            {
                                //All accpeted
                                
                                HashSet<MyTuple> Intersection = (HashSet<MyTuple>)AcceptedReply[0].GetPayload();
                                for (int i=1; i< AcceptedReply.Count; ++i)
                                {

                                    HashSet<MyTuple> mySet2 = (HashSet<MyTuple>)AcceptedReply[i].GetPayload();
                                    Intersection.Intersect(mySet2);
                                }

                                //Test if null
                                if (Intersection.Count == 0)
                                {
                                    //Restart FAse 1
                                    ClientProgram.Pending_SignalEvent.Set();
                                }
                                else
                                {
                                    foreach (MyTuple i in Intersection)
                                    {
                                        //Console.WriteLine("(ClientServices) TAKE Success: " + i.ToString());
                                        ClientProgram.FinnishTake();
                                        ClientProgram.Pending_SignalEvent.Set();

                                        

                                        //Console.WriteLine("Antes do Remove");
                                        

                                        

                                        //Console.WriteLine("Depois do Remove");
                                        break;
                                    }
                                }                               
                            }
                            break;
                        case "ack": //FASE 2
                            ++NumOfAckReceived;
                            if(NumOfAckReceived == ClientProgram.AllServers.Count)
                            {
                                //All received. Finnish
                                NumOfAckReceived = 0;
                                Console.WriteLine("(ClientServices) TAKE Success in all");
                                ClientProgram.FinnishTake();
                                
                            }
                            ClientProgram.Pending_SignalEvent.Set();
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
