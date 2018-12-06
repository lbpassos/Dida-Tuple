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
        
        // ============================ TIMEOUT DEFINITION FOR EACH MESSAGE (TTL)
        private const int TTL = 1000;
        private const long TimeOut_ACCEPTANCE_const = 1000 * 30 * 1; //Starts with two minutes
        private static Timeout TimeOut_ACCEPTANCE;


        // ============================ EVENTS
        private static ManualResetEvent StartConfiguration_SignalEvent = new ManualResetEvent(false); //Pending thread can start
        private static ManualResetEvent StartConfiguration_I_SignalEvent = new ManualResetEvent(false); //Pending thread can start
        private static ManualResetEvent Hello_SignalEvent = new ManualResetEvent(false); //List of pendings is Empty

        // ============================ TEMPORARY STORAGE
        private static CommandReplicas CReplica_tmp;
        private static Timeout Timeout_tmp;



        private const int STATE_MACHINE_NETWORK_START = 0;
        private const int STATE_MACHINE_WAIT_REPLY = 1;
        private const int STATE_MACHINE_WAIT_ACCEPTANCE = 2;
        private const int STATE_MACHINE_PROCESS_ACCEPTANCE = 3;
        private const int STATE_MACHINE_KEEP_ALIVE = 4;
        private const int STATE_MACHINE_IM_ALONE = 5;
        private const int STATE_MACHINE_WAIT_COMMIT = 6;
        private const int STATE_MACHINE_SEND_ACCEPTANCE = 7;
        private const int STATE_MACHINE_TEST_KEEP_ALIVE = 8;
        private const int STATE_MACHINE_NETWORK_START_BEFORE_STABILIZATION = 9;
        private const int STATE_MACHINE_NETWORK_START_IM_ALONE = 10;
        private const int STATE_MACHINE_NETWORK_START_WE_TWO = 11;

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
        /*public static void InsertInPending(Timeout element) //Insert in Pendings List
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

        }*/

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

            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START_BEFORE_STABILIZATION;
            PendingReplyFromServers = new List<Timeout>();
            ID_SendCommandsToReplica_thread = new List<int>();
            Accepted = new List<CommandReplicas>();
            Hello = new List<Timeout>();
            MyID = id;

            //MANAGER_MODE_STATE = MANAGER_MODE_STATE_SEND_INVITATION;
            //Semaphore_pending = new Semaphore(0, 1); ;

            /* END XL */





            //new Thread(() => PingLoop()).Start();
            new Thread(() => NetworkStatusLoop()).Start();
            //new Thread(() => CheckPendings_thread()).Start();

            //new Thread(() => ManagerMODE_thread()).Start();
            //new Thread(() => CheckPendings_thread()).Start();

            //new Thread(() => CheckCommandsInQueueFromReplica_thread()).Start();
            new Thread(() => CheckHellos_thread()).Start();

        }

        
        private static void NetworkStatusLoop()
        {

            while (true)
            {
                NetworkStatusStateMachine();

                Thread.Sleep(1000);
            }
           
        }


        //===================================================================
        //                       INTERPRET COMMANDS THREAD (RECEIVED FROM OTHER REPLICA)
        //===================================================================
        public static CommandReplicas GetCommandsInQueueFromReplica()
        {
            CommandReplicas cmd=null;
            if (ServerService.CommLayer_forReplica.GetQueueSize() > 0) //if there is commands
            {
                cmd = ServerService.CommLayer_forReplica.RemoveFromCommandQueue();

                switch (cmd.GetCommand())
                {
                    case "START_CONFIGURATION_II":
                        CurrentViewID.AddNodeInView(cmd.GetID());
                        //StartConfiguration_SignalEvent.Set();
                        break;
                    case "START_CONFIGURATION_III":
                        CurrentViewID.AddNodeInView(cmd.GetID());
                        //StartConfiguration_I_SignalEvent.Set();
                        break;
                }



            }
            return cmd;
        }



       
        //===================================================================
        //                       SURVEY THE PENDING LIST (INVITATION SEND)
        //===================================================================
        /*private void CheckPendings_thread()
        {
            while (true)
            {
                
                Console.WriteLine("Checking Pendings Waiting for Event");

                Pending_SignalEvent.WaitOne(); //There is at least one Pending answer after INVITE
                Pending_SignalEvent.Reset();

                while (true)
                {
                    Console.WriteLine("Checking Pendings has the Semaphore");
                    Console.WriteLine("NUmber of Pendings: " + GetPendingsSize());
                    if (GetPendingsSize() > 0 )
                    {
                        for (int i = 0; i < GetPendingsSize(); ++i)
                        {
                           
                            GetElementInPending(i);
                        }
                    }
                    else
                    {
                        Console.WriteLine("ALL Pendings Solved");
                        //PendingEmpty_SignalEvent.Set();
                        break;
                        //Semaphore_pending.Release(1);

                    }
                    Thread.Sleep(30000);
                }
            }
        }*/

        //Survey the Hello Queue (Hello Commands we must receive in the current view)
        private void CheckHellos_thread()
        {
            while (true)
            {
                Thread.Sleep(50);

                Hello_SignalEvent.WaitOne();
                Hello_SignalEvent.Reset();

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

                        obj.RX_ReplicaCommand(cmd); //Send INVITES
                        //InsertInPending( new Timeout(cmd.GetID(), TTL) ); //Timeout 1 [sec]. Push to Pendings
                        //++Sent_INVITEMessagesCount;

                        /*if(GetPendingsSize() == 1)
                        {
                            Pending_SignalEvent.Set(); //Signal at least one message sent
                        }*/
                        break;
                    case "ACCEPTANCE":
                        obj.RX_ReplicaCommand(cmd); //Send 
                        //UnderlingStart_SignalEvent.Set();
                        break;
                    case "HELLO":
                        obj.RX_ReplicaCommand(cmd);
                        if (GetHelloSize() == 1)
                        {
                            Hello_SignalEvent.Set(); //Signal at least one message sent
                        }                        
                        break;
                    case "REFUSAL":
                        obj.RX_ReplicaCommand(cmd);
                        break;
                    case "COMMIT":
                        obj.RX_ReplicaCommand(cmd);
                        break;
                    case "START_CONFIGURATION_I":
                        obj.RX_ReplicaCommand(cmd);
                        break;
                    case "START_CONFIGURATION_II":
                        obj.RX_ReplicaCommand(cmd);
                        break;
                    case "START_CONFIGURATION_III":
                        obj.RX_ReplicaCommand(cmd);
                        break;
                    default:
                        break;

                }
            }
            catch(Exception e)
            {
                ++NotSent_INVITEMessagesCount;
            }
        }


        





        //===================================================================
        //                       Main program
        //===================================================================
        private static void NetworkStatusStateMachine()
        {

            //Console.WriteLine("STATE: " + STATE_MACHINE_NETWORK);
            CommandReplicas c;

            switch (STATE_MACHINE_NETWORK)
            {
                case STATE_MACHINE_NETWORK_START_BEFORE_STABILIZATION: //Minimun 3
                    Console.WriteLine("=====================STATE_MACHINE_NETWORK_START_BEFORE_STABILIZATION====================");
                    Console.WriteLine("Waiting in Start Randomly");
                    Thread.Sleep(1000 + new Random().Next(1000, 60000));

                    NotSent_INVITEMessagesCount = 0;
                    int NotSent_INVITEMessagesCount_prev = NotSent_INVITEMessagesCount;
                    int[] idx = { 0, 0 };
                    int IndexForIdx = 0;

                    for (int i = 0; i < Server.AllServers.Count; i++)
                    {
                        if (Server.AllServers[i].ID == MyID) //Avoid ping himself
                        {
                            continue;
                        }

                        SendCommandsToReplica_thread(new CommandReplicas("START_CONFIGURATION_I", null, null, MyAddress, MyID), Server.AllServers[i].UID);
                        if(NotSent_INVITEMessagesCount_prev == NotSent_INVITEMessagesCount)
                        {
                            idx[IndexForIdx++] = i;
                        }
                        NotSent_INVITEMessagesCount_prev = NotSent_INVITEMessagesCount;
                    }
                    if (NotSent_INVITEMessagesCount == Server.AllServers.Count-1)
                    {
                        //I´m the first node
                        STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START_IM_ALONE;
                    }
                    else
                    {
                        if (NotSent_INVITEMessagesCount == Server.AllServers.Count - 2)
                        {
                            //there is another node
                            NotSent_INVITEMessagesCount = 0;
                            SendCommandsToReplica_thread(new CommandReplicas("START_CONFIGURATION_II", null, null, MyAddress, MyID), Server.AllServers[ idx[0] ].UID);
                            if (NotSent_INVITEMessagesCount == 0)
                            {
                                CurrentViewID.AddNodeInView(Server.AllServers[ idx[0] ].ID);
                                
                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START_WE_TWO;
                            }
                        }
                        else
                        {
                            if(NotSent_INVITEMessagesCount == Server.AllServers.Count - 3)
                            {
                                for(int i=0; i<2; ++i)
                                {
                                    SendCommandsToReplica_thread(new CommandReplicas("START_CONFIGURATION_III", null, null, MyAddress, MyID), Server.AllServers[idx[i]].UID);
                                }
                                CurrentViewID.AddNodeInView(Server.AllServers[idx[0]].ID);
                                CurrentViewID.AddNodeInView(Server.AllServers[idx[1]].ID);
                                STATE_MACHINE_NETWORK = STATE_MACHINE_KEEP_ALIVE;
                            }
                        }
                    }
                    break;
                case STATE_MACHINE_NETWORK_START_IM_ALONE:
                    Console.WriteLine("ALONE with VIEW: " + CurrentViewID);
                    Thread.Sleep(1000);

                    c = GetCommandsInQueueFromReplica();
                    if (c == null)
                    {
                        break;
                    }
                    if (c.GetCommand() == "START_CONFIGURATION_II")
                    {
                        STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START_WE_TWO;
                    }
                    
                    //StartConfiguration_SignalEvent.WaitOne();
                    //StartConfiguration_SignalEvent.Reset();
                    
                    
                    break;
                case STATE_MACHINE_NETWORK_START_WE_TWO:
                    Console.WriteLine("Two Guys with VIEW: " + CurrentViewID);

                    Thread.Sleep(1000);

                    c = GetCommandsInQueueFromReplica();
                    if (c == null)
                    {
                        break;
                    }
                    if (c.GetCommand() == "START_CONFIGURATION_III")
                    {
                        STATE_MACHINE_NETWORK = STATE_MACHINE_KEEP_ALIVE;
                    }

                    //StartConfiguration_I_SignalEvent.WaitOne();
                    //StartConfiguration_I_SignalEvent.Reset();
                    break;




                case STATE_MACHINE_NETWORK_START: // Send INVITE for all

                    //ServerService.IgnoreInbound = true; //Stop Receive commands
                    //ServerService.CommLayer_forReplica.Erase(); //Erase all received


                    Console.WriteLine("Waiting in Start Randomly");
                    Thread.Sleep(1000 + new Random().Next(1000, 60000));
                    Console.WriteLine("IN STATE_MACHINE_NETWORK_START: Sending INVITES: " + ProposedViewID);

                    ProposedViewID.IncSequence(); //Propose viewID+1
                    PendingReplyFromServers.Clear();
                    Accepted.Clear();
                    //ErasePendings(); //Erase pendings
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

                    //Console.WriteLine("A Finalizar STATE_MACHINE_NETWORK_START");
                    TimeOut_ACCEPTANCE = new Timeout(0, TimeOut_ACCEPTANCE_const);

                    //ServerService.IgnoreInbound = false; //Start Receive commands

                    STATE_MACHINE_NETWORK = STATE_MACHINE_WAIT_ACCEPTANCE;
                    break;
                case STATE_MACHINE_WAIT_ACCEPTANCE:
                    //Thread.Sleep(5);//Rate of 5 ms
                    if (TimeOut_ACCEPTANCE.IsTimeOut() == true)
                    {
                        Console.WriteLine("=============Timeout");
                        if ( Accepted.Count>(Server.AllServers.Count-1)/2 + 1 ) //maioria recebida
                        {
                            int pos_tmp = 0;
                            int sequence_tmp = CurrentViewID.GetSequence();
                            bool flag = false;

                            for (int i = 0; i < Accepted.Count; ++i)
                            {
                                if (Accepted[i].GetProposedView().GetSequence() < CurrentViewID.GetSequence())
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
                                STATE_MACHINE_NETWORK = STATE_MACHINE_KEEP_ALIVE;
                            }                          
                            else
                            {
                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START; //The majority of expected ACCEPTANCE did not arrive
                            }
                        }
                        else
                        {
                            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START; //The majority of expected ACCEPTANCE did not arrive
                        }
                        break;

                        ////////////////////

                    }
                    else
                    {
                        c = GetCommandsInQueueFromReplica(); //Check Command
                        if (c == null)
                        {
                            break;
                        }
                        Console.WriteLine("==========STATE_MACHINE_WAIT_ACCEPTANCE====: " + c);

                        if (c.GetCommand() == "ACCEPTANCE")
                        {
                            Console.WriteLine("============ Received ACCEPTANCE from: " + c.GetURI());
                            //check if timeout
                            //Timeout t = new Timeout(c.GetID(), 0); //just for the equals
                            Accepted.Add(c);
                        }
                        else
                        {
                            if (c.GetCommand() == "REFUSE")
                            {
                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START; //At Least one refused. Must increase The proposed view ID 
                            }
                            else
                            {
                                if (c.GetCommand() == "INVITE")
                                {
                                    Console.WriteLine("Proposed View: " + c.GetProposedView().GetSequence());
                                    Console.WriteLine("My View: " + CurrentViewID.GetSequence());
                                    if (c.GetProposedView().GetSequence() <= CurrentViewID.GetSequence())
                                    {
                                        //I Must send REFUSAL
                                        Console.WriteLine("Send REFUSAL ");
                                        SendCommandsToReplica_thread(new CommandReplicas("REFUSAL", CurrentViewID, null, MyAddress, MyID), c.GetURI());
                                    }
                                    else
                                    {

                                        CReplica_tmp = c;
                                        //TmpView = c.GetProposedView();//temporary view
                                        //Console.WriteLine("Sending ACCEPTANCE: " + CurrentViewID);
                                        //SendCommandsToReplica_thread(new CommandReplicas("ACCEPTANCE", CurrentViewID, null, MyAddress, MyID), c.GetURI());
                                        STATE_MACHINE_NETWORK = STATE_MACHINE_SEND_ACCEPTANCE; //At Least one refused. Must increase The proposed view ID 
                                    }

                                }
                            }
                        }
                    }
                    break;
                case STATE_MACHINE_SEND_ACCEPTANCE:
                    TmpView = CReplica_tmp.GetProposedView();//temporary view

                    Console.WriteLine("Sending ACCEPTANCE: " + CurrentViewID);

                    SendCommandsToReplica_thread(new CommandReplicas("ACCEPTANCE", CurrentViewID, null, MyAddress, MyID), CReplica_tmp.GetURI());
                    Timeout_tmp = new Timeout(0, TimeOut_ACCEPTANCE_const);

                    STATE_MACHINE_NETWORK = STATE_MACHINE_WAIT_COMMIT; 
                    break;

                case STATE_MACHINE_WAIT_COMMIT:
                    c = GetCommandsInQueueFromReplica();

                    Console.WriteLine("=========== STATE_MACHINE_WAIT_COMMIT =========" + c);
                    if (c != null )
                    {
                        if (c.GetCommand() == "COMMIT")
                        {
                            //I must initialized everything
                            ServerService.ts = c.GetTSS(); //Update Tuplespace
                            CurrentViewID.ClearView();
                            for (int i = 0; i < c.GetProposedView().GetSizeOfView(); ++i)
                            {
                                CurrentViewID.AddNodeInView(c.GetProposedView().GetElementOfView(i));
                            }
                        }
                        else
                        {
                            if (c.GetCommand() == "INVITE")
                            {
                                if (c.GetProposedView().GetSequence() < CurrentViewID.GetSequence())
                                {
                                    //I Must send REFUSAL
                                    Console.WriteLine("Send REFUSAL ");
                                    SendCommandsToReplica_thread(new CommandReplicas("REFUSAL", CurrentViewID, null, MyAddress, MyID), c.GetURI());
                                }
                                else
                                {

                                    CReplica_tmp = c;
                                    //TmpView = c.GetProposedView();//temporary view
                                    //Console.WriteLine("Sending ACCEPTANCE: " + CurrentViewID);
                                    //SendCommandsToReplica_thread(new CommandReplicas("ACCEPTANCE", CurrentViewID, null, MyAddress, MyID), c.GetURI());
                                    STATE_MACHINE_NETWORK = STATE_MACHINE_SEND_ACCEPTANCE; //At Least one refused. Must increase The proposed view ID 
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Timeout_tmp.IsTimeOut() == true) // No Commit arrived
                        {
                            Console.WriteLine("No Commit ARRIVED");
                            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START;
                        }
                    }

                    break;

                case STATE_MACHINE_KEEP_ALIVE:
                    Thread.Sleep(500);//Send keep alive in 0.5 seconds
                    //Console.WriteLine("Current View: " + CurrentViewID);
                    //StartConfiguration_SignalEvent.WaitOne();

                    for(int i=0; i<CurrentViewID.GetSizeOfView(); ++i)
                    {
                        int tmp = CurrentViewID.GetElementOfView(i);
                        Console.WriteLine("======TMP: " + tmp);

                        if (tmp == MyID)
                        {
                            continue;
                        }
                        //Hello.Add( new Timeout(tmp, 60000) );
                        SendCommandsToReplica_thread(new CommandReplicas("HELLO", null, null, MyAddress, MyID), Server.AllServers[tmp-1].UID);
                        Console.WriteLine("Hello Sent to = " + Server.AllServers[tmp-1].UID);
                    }
                    TimeOut_ACCEPTANCE = new Timeout(0, TimeOut_ACCEPTANCE_const);
                    STATE_MACHINE_NETWORK = STATE_MACHINE_TEST_KEEP_ALIVE;
                    break;
                case STATE_MACHINE_TEST_KEEP_ALIVE:

                    //Console.WriteLine("Current View: " + CurrentViewID);
                    //StartConfiguration_SignalEvent.WaitOne();
                    Console.WriteLine("=============STATE_MACHINE_TEST_KEEP_ALIVE==================");

                    Thread.Sleep(3000);
                    if (TimeOut_ACCEPTANCE.IsTimeOut() == true)
                    {
                        for(int i=0; i<CurrentViewID.GetSizeOfView(); ++i)
                        {
                            if (CurrentViewID.GetNodeID() == MyID)
                            {
                                continue;
                            }
                            if ( Accepted.Contains( new CommandReplicas(null, null, null, null, CurrentViewID.GetNodeID()))==false)
                            {
                                //something wrong
                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START;
                                return;
                            }
                        }
                        STATE_MACHINE_NETWORK = STATE_MACHINE_KEEP_ALIVE;
                    }

                    c = GetCommandsInQueueFromReplica();
                    if (c != null)
                    {
                        if (c.GetCommand() == "INVITE")
                        {
                            if (c.GetProposedView().GetSequence() <= CurrentViewID.GetSequence())
                            {
                                //I Must send REFUSAL
                                Console.WriteLine("Send REFUSAL ");
                                SendCommandsToReplica_thread(new CommandReplicas("REFUSAL", CurrentViewID, null, MyAddress, MyID), c.GetURI());
                            }
                            else
                            {

                                CReplica_tmp = c;
                                //TmpView = c.GetProposedView();//temporary view
                                //Console.WriteLine("Sending ACCEPTANCE: " + CurrentViewID);
                                //SendCommandsToReplica_thread(new CommandReplicas("ACCEPTANCE", CurrentViewID, null, MyAddress, MyID), c.GetURI());
                                STATE_MACHINE_NETWORK = STATE_MACHINE_SEND_ACCEPTANCE; //At Least one refused. Must increase The proposed view ID 
                            }
                        }
                        else
                        {
                            if (c.GetCommand() == "HELLO")
                            {
                                Accepted.Add(c);
                            }
                        }
                    }
                        break;
                default:
                    break;



            }
        }
        
    }
}
