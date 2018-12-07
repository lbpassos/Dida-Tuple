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

        private static string CommandInUse;//Comando que espera por resposta
        private static List<string> currentView;
        private static List<Uri> ServersInTheView = new List<Uri>();

        private static int NumOfAddReceived = 0; //Number of Add received
        
        private static int NumOfAcceptedReceived = 0;

        private static int NumOfAckReceived = 0;
        private static int NumOfRefusedReceived = 0;

        private static List<Command> AcceptedReply = new List<Command>(); //All acepted messages
        

        private static int TotalNum;

        private static CommunicationLayer commLayer = new CommunicationLayer();


        private static bool AlreadyEvolvedFromAdd = false; //if N/2+1 ADD Ack received



        public static void SetCurrentCommandAndView(string c, List<string> current)
        {
            CommandInUse = c;
            currentView = current;
            ServersInTheView.Clear();
        }

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

                    Console.WriteLine("=================" + mt.GetCommand());
                    switch (mt.GetCommand())
                    {
                        case "ack":
                            if (CommandInUse == "read" && mt.GetPrevCommand() == "read")
                            {
                                for (int i = 0; i < ClientProgram.ThreadsInAction.Count; ++i)
                                {
                                    if (ClientProgram.ThreadsInAction[i].Equals(new SenderPool(null, null, mt.GetUriFromSender(), mt)) == true)
                                    {
                                        ClientProgram.ThreadsInAction[i].GetThreadState().Kill_hread();
                                        ClientProgram.ThreadsInAction[i].GetThread().Join();
                                        ClientProgram.ThreadsInAction.RemoveAt(i);
                                        ClientProgram.Read_SignalEvent.Set(); //Read Ok. At least One received
                                        Console.WriteLine("Value: " + mt.GetPayload().ToString());
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
                            }
                            else
                            {
                                if (CommandInUse == "add" && mt.GetPrevCommand() == "add") //===================================================
                                {
                                    for (int i = 0; i < ClientProgram.ThreadsInAction.Count; ++i)
                                    {
                                        if (ClientProgram.ThreadsInAction[i].Equals(new SenderPool(null, null, mt.GetUriFromSender(), mt)) == true) //Se está OK
                                        {

                                            if (ServersInTheView.Contains(mt.GetUriFromSender()) == false)
                                            {
                                                ServersInTheView.Add(mt.GetUriFromSender());

                                                if (ServersInTheView.Count >= (currentView.Count / 2 + 1))
                                                {
                                                    //Can progress
                                                    for (int j = 0; j < ClientProgram.ThreadsInAction.Count; ++j)
                                                    {
                                                        for (int k = 0; k < ServersInTheView.Count; ++k)
                                                        {
                                                            if (ClientProgram.ThreadsInAction[j].GetUri().Equals(ServersInTheView[k])){
                                                                ClientProgram.ThreadsInAction[j].GetThreadState().Kill_hread();
                                                                ClientProgram.ThreadsInAction[j].GetThread().Join();
                                                                ClientProgram.ThreadsInAction.RemoveAt(j);
                                                                j = -1;
                                                                break;
                                                            }
                                                        }

                                                    }
                                                    ClientProgram.Add_SignalEvent.Set(); //Signal UpperLayer that it can evolve
                                                    Console.WriteLine("(ClientServices) ADD Success in the majority of replicas: ");
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (CommandInUse == "take" && mt.GetPrevCommand() == "take") //===================================================
                                    {
                                        for (int i = 0; i < ClientProgram.ThreadsInAction.Count; ++i)
                                        {
                                            if (ClientProgram.ThreadsInAction[i].Equals(new SenderPool(null, null, mt.GetUriFromSender(), mt)) == true) //Se está OK
                                            {

                                                if (ServersInTheView.Contains(mt.GetUriFromSender()) == false)
                                                {
                                                    ServersInTheView.Add(mt.GetUriFromSender());

                                                    if (ServersInTheView.Count == currentView.Count)
                                                    {
                                                        //Can progress
                                                        for (int j = 0; j < ClientProgram.ThreadsInAction.Count; ++j)
                                                        {
                                                            for (int k = 0; k < ServersInTheView.Count; ++k)
                                                            {
                                                                if (ClientProgram.ThreadsInAction[j].GetUri().Equals(ServersInTheView[k]))
                                                                {
                                                                    ClientProgram.ThreadsInAction[j].GetThreadState().Kill_hread();
                                                                    ClientProgram.ThreadsInAction[j].GetThread().Join();
                                                                    ClientProgram.ThreadsInAction.RemoveAt(j);
                                                                    j = -1;
                                                                    break;
                                                                }
                                                            }

                                                        }

                                                        //ClientProgram.SendToView(new Command("remove", t, MyAddress, ++SequenceNumber, null));

                                                        ClientProgram.Take_SignalEvent.Set(); //Signal UpperLayer that it can evolve






                                                        Console.WriteLine("(ClientServices) ADD Success in the majority of replicas: ");
                                                    }
                                                }
                                                break;
                                            }
                                        }

                                    }
                                }
                            }
                            break;
                        case "refuse":
                            if (CommandInUse == "refuse" && mt.GetPrevCommand() == "read")
                            {
                            }
                            else
                            {
                                if (CommandInUse == "refuse" && mt.GetPrevCommand() == "take")
                                {
                                }
                            }
                            break;
                        
                        /*case "refuse":
                            NumOfRefusedReceived = 0;
                            NumOfAcceptedReceived = 0;
                            ClientProgram.Take_SignalEvent.Set();*/

                            /*if (NumOfRefusedReceived == 0)
                            {
                                //At least One refused. Start al over again
                                ++NumOfRefusedReceived;
                                
                                ClientProgram.Take_SignalEvent.Set();
                            }
                            else
                            {
                                ++NumOfRefusedReceived;
                                if ((NumOfRefusedReceived + NumOfAcceptedReceived) == ClientProgram.AllServers.Count)
                                {
                                    NumOfRefusedReceived = 0;
                                    NumOfAcceptedReceived = 0;
                                }
                            }*/
                            //break;
                        case "accept":
                            ++NumOfAcceptedReceived;
                            AcceptedReply.Add(mt);
                            if ( NumOfAcceptedReceived== ClientProgram.AllServers.Count)
                            {
                                //All accpeted
                                NumOfAcceptedReceived = 0;


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
                                    ClientProgram.Take_SignalEvent.Set();
                                }
                                else
                                {
                                    foreach (MyTuple i in Intersection)
                                    {
                                        //Console.WriteLine("(ClientServices) TAKE Success: " + i.ToString());
                                        ClientProgram.FinnishTake();
                                        ClientProgram.Take_SignalEvent.Set();

                                        

                                        //Console.WriteLine("Antes do Remove");
                                        

                                        

                                        //Console.WriteLine("Depois do Remove");
                                        break;
                                    }
                                }                               
                            }
                            break;
                        /*case "ack": //FASE 2
                            ++NumOfAckReceived;
                            if(NumOfAckReceived == ClientProgram.AllServers.Count)
                            {
                                //All received. Finnish
                                NumOfAckReceived = 0;
                                Console.WriteLine("(ClientServices) TAKE Success in all");
                                ClientProgram.FinnishTake();
                                ClientProgram.Take_SignalEvent.Set();

                            }
                            //ClientProgram.Pending_SignalEvent.Set();
                            break;*/
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
