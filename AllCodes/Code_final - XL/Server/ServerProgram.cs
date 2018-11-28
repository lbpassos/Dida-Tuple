using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;

namespace Projeto_DAD
{
    class Timeout
    {
        private Stopwatch st;
        private int id;
        private int Timeout_ms;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i">ServerID</param>
        /// <param name="t">Timeout constant</param>
        public Timeout(int i, int t)
        {
            st = new Stopwatch();
            id = i;
            Timeout_ms = t;
        }

        public bool IsTimeOut()
        {
            if(st.ElapsedMilliseconds>Timeout_ms)
            {
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            Timeout t = obj as Timeout;
            if (t == null)
            {
                return false;
            }
            return this.id == t.id;
        }
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

    }


    class ServerProgram
    {

       

        //private const int STATE_MACHINE_NETWORK_KEEP_ALIVE = 1;
        //private const int STATE_MACHINE_NETWORK_IM_ROOT = 2;
        //private const int STATE_MACHINE_NETWORK_END = 3;

        

        
        private int Root_id;

        private Random rand;

        /* XL */
        private const int STATE_MACHINE_NETWORK_START = 0;
        private const int STATE_MACHINE_WAIT_REPLY = 1;

        private static int STATE_MACHINE_NETWORK;

        private static View CurrentViewID;
        private static View ProposedViewID;
        private static Uri MyAddress;

        private static Queue PendingReplyFromServers; //Servers in the current view
        private static List<int> ID_SendCommandsToReplica_thread;
;
        private static CommunicationLayerReplica AnswerToMyCommandsFromReplicas;

        private int SendMessagesCount = 0; //Number os sent messages
        private int MessagesAcceptedCount = 0; //Number os ACCEPTED messages
        private int MessagesRefusedCount = 0; //Number os REFUSED messages
        //private bool AllServersDown = false; //No message was sent
        private readonly object SendMessagesCountLock = new object(); //Lock


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



        


       

        public ServerProgram(Uri uri)
        {

            /* XL */
            CurrentViewID = new View(Server.My_Identification.ID); //View initialized CurrentViewID=ProposedViewID. Sequence Number = 0
            ProposedViewID = new View(Server.My_Identification.ID);
            MyAddress = uri;
            AnswerToMyCommandsFromReplicas = new CommunicationLayerReplica();

            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START;
            PendingReplyFromServers = Queue.Synchronized(new Queue());
            ID_SendCommandsToReplica_thread = new List<int>();

            /* END XL */



            Root_id = 0;
            rand = new Random();

            //new Thread(() => PingLoop()).Start();
            new Thread(() => NetworkStatusLoop()).Start();

        }

        
        private void NetworkStatusLoop()
        {
            /*while (STATE_MACHINE_NETWORK != STATE_MACHINE_NETWORK_END)
            {
                NetworkStatusStateMachine();
                //Thread.Sleep(1000);
            }*/
        }


        //Survey the Pending Queue (Commands without Reply)
        private void CheckPendings_thread()
        {
            while (true)
            {
                Thread.Sleep(50);
                if (PendingReplyFromServers.Count > 0)
                {
                    Timeout tmp = (Timeout)PendingReplyFromServers.Dequeue();
                    if (tmp.IsTimeOut()==false) //If timeout not expire. Push to Queue 
                    {
                        PendingReplyFromServers.Enqueue(tmp);
                    }
                }
            }
        }

        //Send Commands to replicas
        private void SendCommandsToReplica_thread(CommandReplicas cmd, int Server_id)
        {
            ServerService obj = (ServerService)Activator.GetObject(typeof(ServerService), Server.AllServers[Server_id].UID.AbsoluteUri + "MyRemoteObjectName");

            try
            {
                switch (cmd.GetCommand())
                {
                    case "INVITE":
                        
                        obj.RX_ReplicaCommand(cmd);
                        PendingReplyFromServers.Enqueue(new Timeout(Server_id, 1000)); //Timeout 1 [sec]
                        lock (SendMessagesCountLock)
                        {
                            ++SendMessagesCount;
                        }
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

            }
        }


        private void NetworkStatusStateMachine()
        {

            switch (STATE_MACHINE_NETWORK)
            {
                case STATE_MACHINE_NETWORK_START: // Send INVITE for all
                    //Console.WriteLine("IN STATE_MACHINE_NETWORK_START: BEGIN");
                    ProposedViewID.IncSequence(); //Propose viewID+1
                    PendingReplyFromServers.Clear();

                    for (int i = 0; i < Server.AllServers.Count; i++)
                    {
                        if (Server.AllServers[i].ID == Server.My_Identification.ID) //Avoid ping himself
                        {
                            continue;
                        }
                        Thread newThread = new Thread( new ThreadStart(SendCommandsToReplica_thread) );

                            //new Thread(() => SendCommandsToReplica_thread(new CommandReplicas("INVITE", ProposedViewID, null, MyAddress), i)).Start();
                        ID_SendCommandsToReplica_thread;

                    }
                    STATE_MACHINE_NETWORK = STATE_MACHINE_WAIT_REPLY;
                    break;
                case STATE_MACHINE_WAIT_REPLY:
                    break;
            }
        }
        
    }
}
