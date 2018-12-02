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
        // ============================ MANAGER
        private static int MANAGER_MODE_STATE;
        private const int MANAGER_MODE_STATE_BLOCK = 0;
        private const int MANAGER_MODE_STATE_SEND_INVITATION = 1;
        private const int MANAGER_MODE_STATE_WAIT_FOR_ACCEPTANCE = 2;
        private const int MANAGER_MODE_STATE_KEEP_ALIVE = 3;

        // ============================ UNDERLING
        private static int UNDERLING_MODE_STATE;
        private const int UNDERLING_MODE_STATE_BLOCK = 0;
        private const int UNDERLING_MODE_STATE_RECEIVE_INVITATION = 1;

        // ============================ TIMEOUT DEFINITION FOR EACH MESSAGE (TTL)
        private const int TTL = 1000;


        // ============================ EVENTS
        private static ManualResetEvent Pending_SignalEvent = new ManualResetEvent(false); //Pending thread can start
        private static ManualResetEvent PendingEmpty_SignalEvent = new ManualResetEvent(false); //List of pendings is Empty
        private static ManualResetEvent UnderlingStart_SignalEvent = new ManualResetEvent(false); //List of pendings is Empty
        private static ManualResetEvent UnderlingMustNotStart_SignalEvent = new ManualResetEvent(false); //List of pendings is Empty


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
        public static bool IsElementInPending(Timeout element) //Check and removes element from the Pendings List only if TIMEOUT
        {
            lock (PendingMessagesCountLock)
            {
                if (PendingReplyFromServers.Contains(element))
                {
                    return true;
                }
                return false;
            }
        }
        public static void RemoveElementInPending(int pos) //Check and removes element from the Pendings List only if TIMEOUT
        {
            lock (PendingMessagesCountLock)
            {
                PendingReplyFromServers.RemoveAt(pos);
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

            MANAGER_MODE_STATE = MANAGER_MODE_STATE_SEND_INVITATION;
            //Semaphore_pending = new Semaphore(0, 1); ;

            /* END XL */





            //new Thread(() => PingLoop()).Start();
            //new Thread(() => NetworkStatusLoop()).Start();
            new Thread(() => CheckPendings_thread()).Start();

            new Thread(() => ManagerMODE_thread()).Start();
            //new Thread(() => CheckPendings_thread()).Start();

            new Thread(() => CheckCommandsInQueueFromReplica_thread()).Start();
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


        //===================================================================
        //                       INTERPRET COMMANDS THREAD (RECEIVED FROM OTHER REPLICA)
        //===================================================================
        public static void CheckCommandsInQueueFromReplica_thread()
        {
            while (true)
            {
                
                Thread.Sleep(50);//Min time to check commands
                
                if (ServerService.CommLayer_forReplica.GetQueueSize() > 0) //if there is commands
                {
                    CommandReplicas cmd = ServerService.CommLayer_forReplica.RemoveFromCommandQueue();
                    switch (cmd.GetCommand())
                    {
                        case "INVITE": //Underling Mode
                            Console.WriteLine("Received an INVITE ");
                            //UnderlingStart_SignalEvent.Set();

                            if (cmd.GetProposedView().GetSequence() < CurrentViewID.GetSequence())
                            {
                                //I Must send REFUSAL
                                Console.WriteLine("Send REFUSAL ");
                                SendCommandsToReplica_thread(new CommandReplicas("REFUSAL", CurrentViewID, null, MyAddress, MyID), cmd.GetURI() );
                            }
                            else
                            {
                                MANAGER_MODE_STATE = MANAGER_MODE_STATE_BLOCK; //Blocks manager

                                TmpView = cmd.GetProposedView();//temporary view
                                Console.WriteLine("Sending ACCEPTANCE: " + CurrentViewID);
                                SendCommandsToReplica_thread(new CommandReplicas("ACCEPTANCE", CurrentViewID, null, MyAddress, MyID), cmd.GetURI() );
                            }
                            break;
                        case "ACCEPTANCE":
                            Timeout t = new Timeout(cmd.GetID(), 0); //just to test if is in Pendings List
                            
                            //I´m waiting for ACCPETD REply
                            for (int i = 0; i < GetPendingsSize(); ++i)
                            {
                                if (IsElementInPending(t) == true)
                                {
                                    RemoveElementInPending(i); //Remove dos pengings
                                    Accepted.Add(cmd);
                                }
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
                            //STATE_MACHINE_NETWORK = STATE_MACHINE_KEEP_ALIVE;
                            break;
                    }
                }
            }
        }

       
        //===================================================================
        //                       SURVEY THE PENDING LIST (INVITATION SEND)
        //===================================================================
        private void CheckPendings_thread()
        {
            while (true)
            {
                
                Console.WriteLine("Checking Pendings Waiting for Event");

                Pending_SignalEvent.WaitOne(); //There is at least one Pending answer after INVITE
                Pending_SignalEvent.Reset();

                while (true)
                {
                    Console.WriteLine("Checking Pendings has the Semaphore");
                    Console.WriteLine("NUmber of Pendings" + GetPendingsSize());
                    if (GetPendingsSize() > 0 && MANAGER_MODE_STATE==MANAGER_MODE_STATE_WAIT_FOR_ACCEPTANCE )
                    {
                        for (int i = 0; i < GetPendingsSize(); ++i)
                        {
                            GetElementInPending(i);
                        }
                    }
                    else
                    {
                        Console.WriteLine("ALL Pendings Solved");
                        PendingEmpty_SignalEvent.Set();
                        break;
                        //Semaphore_pending.Release(1);

                    }
                    Thread.Sleep(50);
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




        //===================================================================
        //                       SEND COMMANDS TO REPLICAS
        //===================================================================
        private static void SendCommandsToReplica_thread(CommandReplicas cmd, Uri End)
        {
            ServerService obj = (ServerService)Activator.GetObject(typeof(ServerService), End.AbsoluteUri + "MyRemoteObjectName");

            try
            {
                switch (cmd.GetCommand())
                {
                    case "INVITE":
                        Console.WriteLine("Sending INVITES to " + End.AbsoluteUri);

                        obj.RX_ReplicaCommand(cmd); //Send
                        InsertInPending( new Timeout(cmd.GetID(), TTL) ); //Timeout 1 [sec]. Push to Pendings
                        ++Sent_INVITEMessagesCount;

                        if(GetPendingsSize() == 1)
                        {
                            Pending_SignalEvent.Set(); //Signal at least one message sent
                        }
                        break;
                    case "ACCEPTANCE":
                        obj.RX_ReplicaCommand(cmd); //Send 
                        UnderlingStart_SignalEvent.Set();
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
                switch (cmd.GetCommand())
                {
                    case "INVITE":
                        ++NotSent_INVITEMessagesCount;
                        if (NotSent_INVITEMessagesCount == Server.AllServers.Count - 1)//Test if all messages failed
                        {
                            PendingEmpty_SignalEvent.Set();
                        }
                        break;
                    case "ACCEPTANCE":
                        UnderlingMustNotStart_SignalEvent.Set();
                        break;
                }
            }
        }


        //===================================================================
        //                       UNDERLING 
        //===================================================================
        private static void UnderlingMODE()
        {

            while (true)
            {
                UnderlingStart_SignalEvent.WaitOne(); //When power up. It starts in Manager Mode
                UnderlingStart_SignalEvent.Reset();

                //I'm waiting for a commit



            }
        }


        //===================================================================
        //                       Manager
        //===================================================================
        private static void ManagerMODE_thread()
        {
            while (true)
            {
                switch (MANAGER_MODE_STATE)
                {
                    case MANAGER_MODE_STATE_BLOCK:
                        UnderlingMustNotStart_SignalEvent.WaitOne(); //When power up. It starts in Manager Mode
                        UnderlingMustNotStart_SignalEvent.Reset();
                        MANAGER_MODE_STATE = MANAGER_MODE_STATE_SEND_INVITATION;
                        break;
                    case MANAGER_MODE_STATE_SEND_INVITATION:
                        Console.WriteLine("IN Manager MODE: Sending INVITES");

                        ProposedViewID.IncSequence(); //Propose viewID+1
                        PendingReplyFromServers.Clear();
                        Accepted.Clear();
                        ErasePendings(); //Erase pendings
                        Sent_INVITEMessagesCount = 0;
                        NotSent_INVITEMessagesCount = 0;
                        EraseHellos();

                        for (int i = 0; i < Server.AllServers.Count; i++)
                        {
                            if (Server.AllServers[i].ID == Server.My_Identification.ID) //Avoid ping himself
                            {
                                continue;
                            }

                            SendCommandsToReplica_thread(new CommandReplicas("INVITE", ProposedViewID, null, MyAddress, MyID), Server.AllServers[i].UID);
                        }
                        MANAGER_MODE_STATE = MANAGER_MODE_STATE_WAIT_FOR_ACCEPTANCE;
                        break;
                    case MANAGER_MODE_STATE_WAIT_FOR_ACCEPTANCE:
                        //Thread.Sleep(30000);//30 seg to receive all ACCEPTED
                        //Bloqueado à espera de evento de pendings EMPTY
                        Console.WriteLine("Waiting for ACCEPTANCE");
                        PendingEmpty_SignalEvent.WaitOne();
                        PendingEmpty_SignalEvent.Reset();
                        Console.WriteLine("PROCESSING ACCEPTANCE");

                        if (Accepted.Count > (Sent_INVITEMessagesCount / 2))
                        {
                            int pos_tmp = 0;
                            int sequence_tmp = CurrentViewID.GetSequence();
                            bool flag = false;

                            for (int i = 0; i < Accepted.Count; ++i)
                            {
                                if (Accepted[i].GetProposedView().GetSequence() <= CurrentViewID.GetSequence())
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
                                for (int i = 0; i < Accepted[pos_tmp].GetProposedView().GetSizeOfView(); ++i)
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
                                    SendCommandsToReplica_thread(new CommandReplicas("COMMIT", CurrentViewID, ServerService.ts, MyAddress, MyID), Server.AllServers[i].UID);
                                }
                                MANAGER_MODE_STATE = MANAGER_MODE_STATE_KEEP_ALIVE;
                            }
                        }
                        else
                        {
                            Random r = new Random();
                            int delay = r.Next(10000);
                            Console.WriteLine("Alone Node. Will retry again in {0} miliseconds", delay);
                            Thread.Sleep(delay);
                            MANAGER_MODE_STATE = MANAGER_MODE_STATE_SEND_INVITATION; //Starts all over again with new Proposed ViewID
                        }
                        break;
                    case MANAGER_MODE_STATE_KEEP_ALIVE:
                        Thread.Sleep(500);//Send keep alive in 0.5 seconds
                        for (int i = 0; i < CurrentViewID.GetSizeOfView(); ++i)
                        {
                            int tmp = CurrentViewID.GetElementOfView(i);
                            if (tmp == MyID)
                            {
                                continue;
                            }
                            Hello[i] = new Timeout(tmp, 1000);
                            //new Thread(() => SendCommandsToReplica_thread(new CommandReplicas("HELLO", CurrentViewID, ServerService.ts, MyAddress, tmp))).Start();
                            SendCommandsToReplica_thread(new CommandReplicas("HELLO", CurrentViewID, null, MyAddress, MyID), Server.AllServers[i].UID);
                        }
                        break;
                    default:
                        Console.WriteLine("============= MANAGER_MODE ============= DEFAULT");
                        break;
                }
            }
        }






        //===================================================================
        //                       Main program
        //===================================================================
        private static void NetworkStatusStateMachine()
        {

            //Console.WriteLine("STATE: " + STATE_MACHINE_NETWORK);
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

                        SendCommandsToReplica_thread(new CommandReplicas("INVITE", ProposedViewID, null, MyAddress, MyID), Server.AllServers[i].UID);
                    }
                    STATE_MACHINE_NETWORK = STATE_MACHINE_WAIT_ACCEPTANCE;
                    break;
                case STATE_MACHINE_WAIT_ACCEPTANCE:
                    Console.WriteLine("Num NOT SENT " + NotSent_INVITEMessagesCount);
                    if( NotSent_INVITEMessagesCount==Server.AllServers.Count-1) //I´m ALONE
                    {
                        NotSent_INVITEMessagesCount = 0;
                        Console.WriteLine("I´m ALONE");
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
                                //new Thread(() => SendCommandsToReplica_thread(new CommandReplicas("COMMIT", CurrentViewID, ServerService.ts, MyAddress, tmp))).Start();
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
                        //SendCommandsToReplica_thread(new CommandReplicas("HELLO", CurrentViewID, ServerService.ts, MyAddress, tmp));
                    }
                    break;
                case STATE_MACHINE_IM_ALONE:
                    
                    
                    break;

                default:
                    break;



            }
        }
        
    }
}
