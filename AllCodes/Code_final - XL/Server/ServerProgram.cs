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

        private static Uri MyAddress;
        private static int MyID;

        public const int STATE_MACHINE_NETWORK_START = 0;
        public const int STATE_MACHINE_NETWORK_WAIT_FOR_ANSWER_INIT = 1;
        public const int STATE_MACHINE_NETWORK_CHECK_ROOT = 2;
        public const int STATE_MACHINE_NETWORK_CHECK_RETRY = 3;
        public const int STATE_MACHINE_NETWORK_IM_ROOT = 4;
        public const int STATE_MACHINE_NETWORK_INFORM_VIEW_CHANGE = 5;
        public const int STATE_MACHINE_NETWORK_UPDATE_ROOT = 6;


        private static int STATE_MACHINE_NETWORK;
        private static int STATE_MACHINE_NETWORK_prev = -1;

        private static int RootIs; //Root ID
        private static Timeout Timeout_tmp;
        private const int NumOfRetryConst = 3;
        private static int NumOfRetry = 0;
        private static ArrayList serversAlive;
        /* XL */

        // ============================ TIMEOUT DEFINITION FOR EACH MESSAGE (TTL)
        private const int TTL = 1000;

        public static ManualResetEvent Pending_SignalEvent = new ManualResetEvent(false); //Pending thread can start
        public static ManualResetEvent CommandAvailable_SignalEvent = new ManualResetEvent(false); //Pending thread can start






        private const int STATE_MACHINE_WAIT_REPLY = 1;
        private const int STATE_MACHINE_WAIT_ACCEPTANCE = 2;
        private const int STATE_MACHINE_PROCESS_ACCEPTANCE = 3;
        private const int STATE_MACHINE_KEEP_ALIVE = 4;
        private const int STATE_MACHINE_IM_ALONE = 5;
        private const int STATE_MACHINE_WAIT_COMMIT = 6;
        private const int STATE_MACHINE_SEND_ACCEPTANCE = 7;
        private const int STATE_MACHINE_TEST_KEEP_ALIVE = 8;

        

        private static View CurrentViewID;
        private static View ProposedViewID;
       

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
        
        public static int GetStateMachine()
        {
            return STATE_MACHINE_NETWORK;
        }

        public static void SetStateMachine(int state)
        {
            STATE_MACHINE_NETWORK = state;
        }

        public static bool AmIRoot()
        {
            if (MyID == RootIs)
            {
                return true;
            }
            return false; ;
        }
        public static int GetMyId()
        {
            return MyID;
        }
        public static void DefineRootId(int id)
        {
            RootIs = id;
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
            serversAlive = new ArrayList();


            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START;
            
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
        //                       Main program
        //===================================================================
        private static void NetworkStatusStateMachine()
        {

            //Console.WriteLine("STATE: " + STATE_MACHINE_NETWORK);
            int id_tmp;
            int idx = 0;
            bool flag = false;

            switch (STATE_MACHINE_NETWORK)
            {
                case STATE_MACHINE_NETWORK_START:
                    //Start searching for the Root

                    Console.WriteLine("================ STATE_MACHINE_NETWORK_START =============");

                    serversAlive.Clear();

                    for (int i = 0; i < Server.AllServers.Count; i++)
                    {
                        Thread.Sleep(50);
                        if (Server.AllServers[i].ID == MyID) //Avoid ping himself
                        {
                            continue;
                        }
                        try
                        {
                            ServerService obj = (ServerService)Activator.GetObject(typeof(ServerService), Server.AllServers[i].UID.AbsoluteUri + "MyRemoteObjectName");
                            Console.WriteLine("CHECK: {0}", Server.AllServers[i].UID.AbsoluteUri);

                            if (obj.IsRoot() == true)
                            {
                                RootIs = Server.AllServers[i].ID; //ID of the current root node

                                
                                
                                obj.RX_ReplicaCommand(new CommandReplicas("REGISTER", null, null, MyAddress, MyID));
                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_WAIT_FOR_ANSWER_INIT;
                                Pending_SignalEvent.Set();
                                //object[] imageFromRoot = obj.getImage();
                                //ServerService.SetTupleSpace(new TupleSpace((List<MyTuple>)imageFromRoot[0])); //Novo tuplespace criado
                                //ServerService.SetCommunicationLayer((Queue)imageFromRoot[1], (List<Command>)imageFromRoot[2]);


                                //ServerService.SetTupleSpace( obj.getImage() ); //get the image of the root
                                //Console.WriteLine("Imagem: ");
                                //Console.WriteLine(ServerService.GetTupleSpaceRepresentation());



                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_CHECK_ROOT;
                                
                                flag = true;
                                

                                
                               
                                //Console.WriteLine("ROOT is {0}", Server.AllServers[i].UID.AbsoluteUri);

                               
                                break;
                            }
                            else
                            {
                                serversAlive.Add(i); //Server ID
                                //Console.WriteLine("ALIVE: {0}", Server.AllServers[i].UID.AbsoluteUri);

                            }
                        }
                        catch (Exception e)
                        {
                            //Console.WriteLine("IN STATE_MACHINE_NETWORK_START: EXCEPTION");
                            Console.WriteLine("DEAD: {0}", Server.AllServers[i].UID.AbsoluteUri);
                            //Console.WriteLine(e);
                        }
                    }
                    //Console.WriteLine("IN STATE_MACHINE_NETWORK_START: TEST_FLAG");
                    if (flag == false)
                    {

                        //Console.WriteLine("FLAG==FALSE: {0}", serversAlive.Count);
                        if (serversAlive.Count == 0) //check if anyone is ROOT
                        {
                            //ROOT
                            RootIs = MyID; //ServerService.setRoot(true);
                            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_IM_ROOT;
                        }
                        else
                        {
                            //Console.WriteLine("MY_IDENTIFICATION: {0}, {1}", Server.My_Identification.ID, (int)serversAlive[0]);
                            if ((Server.My_Identification.ID - 1) < (int)serversAlive[0])
                            {
                                //ROOT
                                //ServerService.setRoot(true);
                                RootIs = MyID;
                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_IM_ROOT;
                            }

                        }
                    }

                    break;
                case STATE_MACHINE_NETWORK_UPDATE_ROOT:

                    serversAlive.Clear();
                    for (int i = 0; i < CurrentViewID.GetSizeOfView(); i++)
                    {
                        Thread.Sleep(50);
                        id_tmp = CurrentViewID.GetElementOfView(i);
                        if (id_tmp == MyID) //Avoid ping himself
                        {
                            continue;
                        }
                        try
                        {
                            idx = Server.AllServers.IndexOf(new EachServer(null, id_tmp));
                            ServerService obj = (ServerService)Activator.GetObject(typeof(ServerService), Server.AllServers[idx].UID.AbsoluteUri + "MyRemoteObjectName");
                            //Console.WriteLine("CHECK: {0}", Server.AllServers[idx].UID.AbsoluteUri);

                            if (obj.IsRoot() == true)
                            {
                                RootIs = id_tmp; //Server.AllServers[i].ID; //ID of the current root node

                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_CHECK_ROOT;

                                flag = true;

                                break;
                            }
                            else
                            {
                                serversAlive.Add(id_tmp); //Server ID
                                //Console.WriteLine("ALIVE: {0}", Server.AllServers[i].UID.AbsoluteUri);

                            }
                        }
                        catch (Exception e)
                        {
                            //Console.WriteLine("IN STATE_MACHINE_NETWORK_START: EXCEPTION");
                           // Console.WriteLine("DEAD: {0}", Server.AllServers[i].UID.AbsoluteUri);
                            //Console.WriteLine(e);
                        }
                    }
                    //Console.WriteLine("IN STATE_MACHINE_NETWORK_START: TEST_FLAG");
                    if (flag == false)
                    {

                        //Console.WriteLine("FLAG==FALSE: {0}", serversAlive.Count);
                        if (CurrentViewID.GetSizeOfView() == 1) //check if anyone is ROOT
                        {
                            //ROOT
                            RootIs = MyID; //ServerService.setRoot(true);
                            //STATE_MACHINE_NETWORK_prev = STATE_MACHINE_NETWORK; //prev state
                            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_IM_ROOT;
                        }
                        else
                        {
                            //Console.WriteLine("MY_IDENTIFICATION: {0}, {1}", Server.My_Identification.ID, (int)serversAlive[0]);
                            serversAlive.Sort();
                            if (MyID < (int)serversAlive[0])
                            {
                                //ROOT
                                //ServerService.setRoot(true);
                                RootIs = MyID;
                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_IM_ROOT;
                            }

                        }
                    }

                    break;
                case STATE_MACHINE_NETWORK_WAIT_FOR_ANSWER_INIT:
                    CommandAvailable_SignalEvent.WaitOne();
                    CommandAvailable_SignalEvent.Reset();
                    Console.WriteLine("============= STATE_MACHINE_NETWORK_WAIT_FOR_ANSWER_INIT ==========");

                    break;
                case STATE_MACHINE_NETWORK_CHECK_ROOT:

                    Console.WriteLine("============= STATE_MACHINE_NETWORK_CHECK_ROOT ==========");
                    Console.WriteLine("========================View: " + CurrentViewID);
                    STATE_MACHINE_NETWORK_prev = STATE_MACHINE_NETWORK; //prev state
                    Thread.Sleep(TTL);
                    //id_tmp = CurrentViewID.GetElementOfView(i);
                    //int idx;
                    try
                    {
                        idx = Server.AllServers.IndexOf(new EachServer(null, RootIs));
                        IServerServices obj = (IServerServices)Activator.GetObject(typeof(IServerServices), Server.AllServers[idx].UID.AbsoluteUri + "MyRemoteObjectName");
                        Console.WriteLine("CHECK: {0}", Server.AllServers[idx].UID.AbsoluteUri);
                        obj.RX_ReplicaCommand(new CommandReplicas("CHECK_ROOT", null, null, MyAddress, MyID));
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine("IN STATE_MACHINE_NETWORK_START: EXCEPTION");
                        //Console.WriteLine("DEAD: {0}", Server.AllServers[idx].UID.AbsoluteUri);
                        //Timeout_tmp = new Timeout(MyID, TTL + new Random().Next(2000));
                        CurrentViewID.RemoveNode(RootIs);

                        STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_INFORM_VIEW_CHANGE;
                        //Console.WriteLine(e);
                    }

                    break;
                /*case STATE_MACHINE_NETWORK_CHECK_RETRY:

                    Console.WriteLine("CHECK_RETRY + Num: " + NumOfRetry);

                    if (NumOfRetry++ < NumOfRetryConst)
                    {
                        STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_CHECK_ROOT;
                    }
                    else
                    {
                        //Assume root deaad
                        NumOfRetry = 0;
                        STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_INFORM_VIEW_CHANGE;

                    }
                    Console.WriteLine("SAIR =============== CHECK_RETRY + Num: " + NumOfRetry);
                    break;
                    */

                case STATE_MACHINE_NETWORK_IM_ROOT:
                    Thread.Sleep(TTL);

                    Console.WriteLine("I´M ROOT");
                    Console.WriteLine("========================View: " + CurrentViewID);
                    STATE_MACHINE_NETWORK_prev = STATE_MACHINE_NETWORK_IM_ROOT; //prev state

                    for (int i = 0; i < CurrentViewID.GetSizeOfView(); i++)
                    {
                        id_tmp = CurrentViewID.GetElementOfView(i);
                        if (id_tmp == MyID) //Avoid ping himself
                        {
                            continue;
                        }
                        try
                        {
                            
                            idx = Server.AllServers.IndexOf( new EachServer(null,id_tmp) );
                            IServerServices obj = (IServerServices)Activator.GetObject(typeof(IServerServices), Server.AllServers[idx].UID.AbsoluteUri + "MyRemoteObjectName");
                            Console.WriteLine("CHECK: {0}", Server.AllServers[id_tmp-1].UID.AbsoluteUri);
                            obj.RX_ReplicaCommand(new CommandReplicas("CHECK", null, null, MyAddress, MyID));
                        }
                        catch (Exception e)
                        {
                            //Console.WriteLine("IN STATE_MACHINE_NETWORK_START: EXCEPTION");
                            //++Failed;
                            //One is failed. Must restart ViewChange
                            CurrentViewID.RemoveNode(id_tmp);

                            
                            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_INFORM_VIEW_CHANGE;

                            Console.WriteLine("DEAD: {0}", Server.AllServers[i].UID.AbsoluteUri);
                            //Console.WriteLine(e);
                        }
                    }
                    break;
                case STATE_MACHINE_NETWORK_INFORM_VIEW_CHANGE:

                    Console.WriteLine("=============== STATE_MACHINE_NETWORK_INFORM_VIEW_CHANGE ============");
                    Console.WriteLine("=============== STATE_MACHINE_NETWORK_INFORM_VIEW_CHANGE ============: " + STATE_MACHINE_NETWORK_prev); 


                    for (int i = 0; i < CurrentViewID.GetSizeOfView(); i++)
                    {
                        id_tmp = CurrentViewID.GetElementOfView(i);
                        if (id_tmp == MyID) //Avoid ping himself
                        {
                            continue;
                        }
                        try
                        {

                            idx = Server.AllServers.IndexOf(new EachServer(null, id_tmp));
                            IServerServices obj = (IServerServices)Activator.GetObject(typeof(IServerServices), Server.AllServers[idx].UID.AbsoluteUri + "MyRemoteObjectName");
                            Console.WriteLine("CHECK: {0}", Server.AllServers[id_tmp - 1].UID.AbsoluteUri);
                            obj.RX_ReplicaCommand(new CommandReplicas("VIEW_CHANGE", CurrentViewID, null, MyAddress, MyID));
                        }
                        catch (Exception e)
                        {
                            //flag = true;
                            //Console.WriteLine("IN STATE_MACHINE_NETWORK_START: EXCEPTION");
                            //++Failed;
                            //One is failed. Must restart ViewChange
                            //CurrentViewID.RemoveNode(id_tmp);
                            //STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_INFORM_VIEW_CHANGE;
                            

                            Console.WriteLine("DEAD: {0}", Server.AllServers[i].UID.AbsoluteUri);
                            break;
                            //Console.WriteLine(e);
                        }
                    }

                    /*if (flag == true)
                    {
                        CurrentViewID.RemoveNode(idx);
                        RootIs = MyID;
                        flag = false;
                    }*/

                    if (STATE_MACHINE_NETWORK_prev == STATE_MACHINE_NETWORK_CHECK_ROOT)
                    {
                        STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_UPDATE_ROOT;
                    }
                    else
                    {
                        STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_prev;
                    }
                    //CurrentViewID.RemoveNode(idx);
                    //RootIs = MyID;                  
                    break;
                default:
                    break;



            }
        }
        
    }
}
