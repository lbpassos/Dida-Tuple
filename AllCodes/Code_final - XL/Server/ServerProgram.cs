using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting.Messaging;


namespace Projeto_DAD
{
    class ServerProgram
    {

        /* XL */
        private const int STATE_MACHINE_NETWORK_START = 0;
        private const int STATE_MACHINE_WAIT_REPLY = 1;
        private const int STATE_MACHINE_WAIT_ACCEPTANCE = 2;
        private const int STATE_MACHINE_PROCESS_ACCEPTANCE = 3;
        private const int STATE_MACHINE_KEEP_ALIVE = 4;
        private const int STATE_MACHINE_IM_ALONE = 5;

        private static int STATE_MACHINE_NETWORK;

        private static View CurrentViewID;
        private static View ProposedViewID;
        private static View TmpView; //temporary view received in an INVITE

        private static Uri MyAddress;
        private static int MyID;

        private static List<Timeout> PendingReplyFromServers; //Servers in the current view
        private static List<int> ID_SendCommandsToReplica_thread;
        private static List<CommandReplicas> Accepted; //Accepted Commands Received
        private static List<Timeout> Hello; //Hello Commands Received
        private static List<Timeout> Commits; //Hello Commands Received

        private static CommunicationLayerReplica AnswerToMyCommandsFromReplicas;

        private static int Sent_INVITEMessagesCount = 0; //Number os sent messages
        private static int NotSent_INVITEMessagesCount = 0; //Number os sent messages

        //private static Semaphore Semaphore_pending;  //Semaphore for pending thread vs 
        private static ManualResetEvent Pending_SignalEvent = new ManualResetEvent(false);

        private static readonly object PendingMessagesCountLock = new object(); //Lock
        private static readonly object HelloMessagesCountLock = new object(); //Lock
        private static readonly object ChannelLock = new object(); //Lock


        public static View GetCurrentViewID()
        {
            return CurrentViewID;
        }
        public static void SetCurrentViewID(View cur)
        {
            CurrentViewID = cur;
        }
        public static Uri GetMyAddress()
        {
            return MyAddress;
        }
        /* END XL */

        public static void InsertCommand(CommandReplicas cmd)
        {
            AnswerToMyCommandsFromReplicas.InsertCommand(cmd);
        }



        //THread safe INsert In The Pendings List
        //===============================================================================
        public static void InsertInPending(Timeout element) //Insert in Pendings List
        {
            lock (PendingMessagesCountLock)
            {
                PendingReplyFromServers.Add(element);
            }
        }
        public static bool GetElementInPending(int element) //Check and removes element from the Pendings List only if TIMEOUT
        {
            lock (PendingMessagesCountLock)
            {
                if (PendingReplyFromServers[element].IsTimeOut())
                {
                    PendingReplyFromServers.RemoveAt(element);
                    return true;
                }
                return false;
            }
        }
       
        public static int GetPendingsSize()
        {
            lock (PendingMessagesCountLock)
            {
                return PendingReplyFromServers.Count;
            }

        }
        public static void ErasePendings()
        {
            lock (PendingMessagesCountLock)
            {
                PendingReplyFromServers.Clear();
            }

        }

        //THread safe INsert In The Hello List
        //===============================================================================
        public static void InsertInHello(Timeout element) //Insert in Pendings List
        {
            lock (HelloMessagesCountLock)
            {
                Hello.Add(element);
            }
        }
        public static int GetHelloSize()
        {
            lock (HelloMessagesCountLock)
            {
                return Hello.Count;
            }
        }
        public static void EraseHellos()
        {
            lock (HelloMessagesCountLock)
            {
                Hello.Clear();
            }
        }
        public static bool ElementInHelloIsTimeout(int element) //Check and removes element from the Pendings List if it exists
        {
            lock (HelloMessagesCountLock)
            {
                if (Hello[element].IsTimeOut() == true)
                {
                    return true;
                }
                else
                {
                    Hello[element].ResetTimeOut();
                    return false;
                }
                
            }
        }


        

        //=========================================================================================
        //                                      MAIN
        //=========================================================================================
        public ServerProgram(Uri uri, int id)
        {

            /* XL */
            CurrentViewID = new View(Server.My_Identification.ID); //View initialized CurrentViewID=ProposedViewID. Sequence Number = 0
            ProposedViewID = new View(Server.My_Identification.ID);
            MyAddress = uri;
            AnswerToMyCommandsFromReplicas = new CommunicationLayerReplica();

            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START;
            PendingReplyFromServers = new List<Timeout>();
            ID_SendCommandsToReplica_thread = new List<int>();
            Accepted = new List<CommandReplicas>();
            Hello = new List<Timeout>();
            MyID = id;
            //Semaphore_pending = new Semaphore(0, 1); ;

            /* END XL */





            //new Thread(() => PingLoop()).Start();
            new Thread(() => NetworkStatusLoop()).Start();
            new Thread(() => CheckPendings_thread()).Start();
            //new Thread(() => CheckHellos_thread()).Start();
            //new Thread(() => CheckHellos_thread()).Start();

        }

        
        private static void NetworkStatusLoop()
        {

            while (true)
            {
                NetworkStatusStateMachine();

                Thread.Sleep(50);
            }
           
        }


        public static void CheckCommandsInQueueFromReplica_thread() //Check commands send by the replicas 
        {
            while (true)
            {
                
                Thread.Sleep(50);//Min time to check commands
                
                if (ServerService.CommLayer_forReplica.GetQueueSize() > 0) //if there is commands
                {
                    CommandReplicas cmd = ServerService.CommLayer_forReplica.RemoveFromCommandQueue();
                    switch (cmd.GetCommand())
                    {
                        case "INVITE":
                            if(cmd.GetProposedView().GetSequence() <= CurrentViewID.GetSequence())
                            {
                                //I Must send REFUSAL
                                SendCommandsToReplica_thread(new CommandReplicas("REFUSAL", CurrentViewID, null, MyAddress, cmd.GetProposedView().GetNodeID()) );
                            }
                            else
                            {
                                TmpView = cmd.GetProposedView();//temporary view
                                SendCommandsToReplica_thread(new CommandReplicas("ACCEPTANCE", CurrentViewID, null, MyAddress, cmd.GetProposedView().GetNodeID()));
                            }
                            break;
                        case "ACCEPTANCE":
                            if (GetElementInPending(cmd.GetID())==true)//Is it in pendings
                            {
                                Accepted.Add(cmd);
                            }                          
                            if (GetPendingsSize() == 0)
                            {
                                //No more answers to receive
                                STATE_MACHINE_NETWORK = STATE_MACHINE_PROCESS_ACCEPTANCE;

                            }
                            break;
                        case "REFUSAL":
                            if (GetElementInPending(cmd.GetID()) == true)//Is it in pendings
                            {
                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START; //Must start with new Ptopose viewId
                            }
                            break;
                        case "HELLO":
                            if(STATE_MACHINE_NETWORK == STATE_MACHINE_KEEP_ALIVE)
                            {
                                //Only process HELLO if in KEep alive mode
                                for(int i=0; i< GetHelloSize(); ++i)
                                {
                                    if( Hello[i].Equals(new Timeout(cmd.GetID(),0) ) == true)
                                    {
                                        if( ElementInHelloIsTimeout(i)==true)
                                        {
                                            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START; //Must start with new Ptopose viewId
                                        }
                                    }
                                }
                            }
                            break;
                        case "COMMIT":

                            ServerService.ts = cmd.GetTSS(); //Update Tuplespace
                            CurrentViewID.ClearView();
                            for (int i = 0; i < cmd.GetProposedView().GetSizeOfView(); ++i)
                            {
                                CurrentViewID.AddNodeInView(cmd.GetProposedView().GetElementOfView(i));
                            }
                            STATE_MACHINE_NETWORK = STATE_MACHINE_KEEP_ALIVE;
                            break;
                    }
                }
            }
        }

        //Survey the Pending Queue (Commands without Reply)
        private void CheckPendings_thread()
        {
            while (true)
            {
                
                Console.WriteLine("Checking Pendings Waiting for Event");

                //Semaphore_pending.WaitOne();
                Pending_SignalEvent.WaitOne(); //There is at least one Pending answer after INVITE
                Pending_SignalEvent.Reset();

                while (true)
                {
                    Console.WriteLine("Checking Pendings has the Semaphore");
                    Console.WriteLine("NUmber of Pendings" + GetPendingsSize());
                    if (GetPendingsSize() > 0)
                    {
                        for (int i = 0; i < GetPendingsSize(); ++i)
                        {
                            GetElementInPending(i);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Checking Pendings FREED the Semaphore");
                        //Semaphore_pending.Release(1);

                    }
                    Thread.Sleep(500);
                }
            }
        }

        //Survey the Hello Queue (Hello Commands we must receive in the current view)
        private void CheckHellos_thread()
        {
            while (true)
            {
                Thread.Sleep(50);
                if(STATE_MACHINE_NETWORK== STATE_MACHINE_KEEP_ALIVE)
                {
                    if (GetHelloSize() > 0)
                    {
                        for(int i=0; i<GetHelloSize(); ++i)
                        {
                            if (ElementInHelloIsTimeout(i) == true)
                            {
                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START; //At least one istimeout. Start View search
                                break;
                            }
                        }
                    }
                }
            }
        }

        //Send Commands to replicas
        private static void SendCommandsToReplica_thread(CommandReplicas cmd)
        {

            //Console.WriteLine("Sending INVITES to " + Server.AllServers[cmd.GetID()-1].UID.AbsoluteUri);
            //Channel(cmd);
            ServerService obj = (ServerService)Activator.GetObject(typeof(ServerService), Server.AllServers[cmd.GetID() - 1].UID.AbsoluteUri + "MyRemoteObjectName");
          

            try
            {
                switch (cmd.GetCommand())
                {
                    case "INVITE":
                        Console.WriteLine("Sending INVITES to " + Server.AllServers[cmd.GetID()-1].UID.AbsoluteUri);

                        obj.RX_ReplicaCommand(cmd); //Send
                        InsertInPending( new Timeout(cmd.GetID(), 1000) ); //Timeout 1 [sec]. Push to Pendings
                        ++Sent_INVITEMessagesCount;

                        if(GetPendingsSize() == 1)
                        {
                            Pending_SignalEvent.Set(); //Signal at least one message sent
                        }
                        
                        

                        break;
                    case "ACCEPTANCE":
                        obj.RX_ReplicaCommand(cmd); //Send
                        break;
                    case "HELLO":
                        obj.RX_ReplicaCommand(cmd);
                        break;
                    case "REFUSAL":
                        obj.RX_ReplicaCommand(cmd);
                        break;
                    case "COMMIT":
                        obj.RX_ReplicaCommand(cmd);
                        break;
                    default:
                        break;

                }
            }
            catch(Exception e)
            {
                if (cmd.GetCommand() == "INVITE")
                {
                    ++NotSent_INVITEMessagesCount;
                }
                //Console.WriteLine(e);
            }
        }


        //===================================================================
        //                       Main program
        //===================================================================
        private static void NetworkStatusStateMachine()
        {

            Console.WriteLine("STATE: " + STATE_MACHINE_NETWORK);
            switch (STATE_MACHINE_NETWORK)
            {
                case STATE_MACHINE_NETWORK_START: // Send INVITE for all
                    Console.WriteLine("IN STATE_MACHINE_NETWORK_START: Sending INVITES");

                    ProposedViewID.IncSequence(); //Propose viewID+1
                    PendingReplyFromServers.Clear();
                    Accepted.Clear();
                    ErasePendings(); //Erase pendings
                    Sent_INVITEMessagesCount = 0;
                    EraseHellos();

                    for (int i = 0; i < Server.AllServers.Count; i++)
                    {
                        if (Server.AllServers[i].ID == Server.My_Identification.ID) //Avoid ping himself
                        {
                            continue;
                        }

                        SendCommandsToReplica_thread(new CommandReplicas("INVITE", ProposedViewID, null, MyAddress, Server.AllServers[i].ID));
                    }
                    STATE_MACHINE_NETWORK = STATE_MACHINE_WAIT_ACCEPTANCE;
                    break;
                case STATE_MACHINE_WAIT_ACCEPTANCE:
                    Console.WriteLine("Num NOT SENT " + NotSent_INVITEMessagesCount);
                    if( NotSent_INVITEMessagesCount==Server.AllServers.Count-1) //I´m ALONE
                    {
                        NotSent_INVITEMessagesCount = 0;
                        STATE_MACHINE_NETWORK = STATE_MACHINE_IM_ALONE;                         
                    }

                    break;
                case STATE_MACHINE_PROCESS_ACCEPTANCE:
                    if(Accepted.Count>(Sent_INVITEMessagesCount / 2) )
                    {
                        int pos_tmp = 0;
                        int sequence_tmp=CurrentViewID.GetSequence();
                        bool flag = false;
                        for (int i=0; i<Accepted.Count; ++i)
                        {
                            if( Accepted[i].GetProposedView().GetSequence()<= CurrentViewID.GetSequence())
                            {
                                continue; //Ignores ViewID inferior than the current
                            }
                            else
                            {
                                if (Accepted[i].GetProposedView().GetSequence() > sequence_tmp)
                                {
                                    sequence_tmp = Accepted[i].GetProposedView().GetSequence();
                                    pos_tmp = i;
                                    flag = true;
                                }
                            }
                        }
                        //Initializes everything
                        if (flag == true)
                        {
                            ServerService.ts = Accepted[pos_tmp].GetTSS(); //Update Tuplespace
                            CurrentViewID.ClearView();
                            for(int i=0; i< Accepted[pos_tmp].GetProposedView().GetSizeOfView(); ++i)
                            {
                                CurrentViewID.AddNodeInView(Accepted[pos_tmp].GetProposedView().GetElementOfView(i));
                            }

                            ProposedViewID = CurrentViewID;
                            for (int i = 0; i < CurrentViewID.GetSizeOfView(); ++i)
                            {
                                int tmp = CurrentViewID.GetElementOfView(i);
                                if (tmp == MyID)
                                {
                                    continue;
                                }
                                new Thread(() => SendCommandsToReplica_thread(new CommandReplicas("COMMIT", CurrentViewID, ServerService.ts, MyAddress, tmp))).Start();
                            }                               
                            STATE_MACHINE_NETWORK = STATE_MACHINE_KEEP_ALIVE;
                        }
                    }
                    STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START;
                    break;
                case STATE_MACHINE_KEEP_ALIVE:
                    Thread.Sleep(500);//Send keep alive in 0.5 seconds
                    for(int i=0; i<CurrentViewID.GetSizeOfView(); ++i)
                    {
                        int tmp = CurrentViewID.GetElementOfView(i);
                        if (tmp == MyID)
                        {
                            continue;
                        }
                        Hello[i] = new Timeout(tmp, 1000);
                        //new Thread(() => SendCommandsToReplica_thread(new CommandReplicas("HELLO", CurrentViewID, ServerService.ts, MyAddress, tmp))).Start();
                        SendCommandsToReplica_thread(new CommandReplicas("HELLO", CurrentViewID, ServerService.ts, MyAddress, tmp));
                    }
                    break;
                case STATE_MACHINE_IM_ALONE:
                    Console.WriteLine("I´m ALONE");
                    
                    break;

                default:
                    break;



            }
        }
        
    }
}
