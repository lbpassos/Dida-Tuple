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


        private static int STATE_MACHINE_NETWORK;
        private static int STATE_MACHINE_NETWORK_prev = -1;

        public const int STATE_MACHINE_NETWORK_START = 0;
        public const int STATE_MACHINE_NETWORK_REQUEST_VIEW = 1;
        public const int STATE_MACHINE_NETWORK_CHECK_VIEW = 2;
        public const int STATE_MACHINE_NETWORK_CHANGE_VIEW= 3;
        public const int STATE_MACHINE_NETWORK_UPDATE_VIEW = 4;
        public const int STATE_MACHINE_NETWORK_UPDATE_ROOT = 5;
        public const int STATE_MACHINE_NETWORK_WAIT = 6;
        public const int STATE_MACHINE_NETWORK_RESTART = 7;
        public const int STATE_MACHINE_NETWORK_RESTART2 = 8;
        public const int STATE_MACHINE_NETWORK_RESTART3 = 9;

        private static IServerServices ss;
        private static List<EachServer> serversAlive = new List<EachServer>();
        public static List<EachServer> AllServers = new List<EachServer>();    //All servers present in the pool
        private static bool first_request = true;
        public static List<View> Proposed_Views = new List<View>();
        private static bool getFromRoot = false;
        public static int view_sequence = 0;
        private static bool Im_Root = false;

        public static bool wait = true;

        private static View CurrentViewID;

        public static EachServer RootServer;
        public static View root_view;

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

        public static int GetMyId()
        {
            return MyID;
        }


        //=========================================================================================
        //                                      MAIN
        //=========================================================================================
        public ServerProgram(Uri uri, int id, List<EachServer> allServers, EachServer rootServer)
        {
            MyAddress = uri;
            MyID = id;
            AllServers = allServers;
            RootServer = rootServer;

            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START;

            NetworkStatusLoop();

        }

        
        private static void NetworkStatusLoop()
        {
            while (true)
            {
                View_Update_Program();
            }
        }


        //===================================================================
        //                       MAIN VIEW UPDATE PROGRAM
        //===================================================================


        private static void View_Update_Program()
        {
            switch (STATE_MACHINE_NETWORK)
            {
                case STATE_MACHINE_NETWORK_START:
                    if (RootServer != null && MyID != RootServer.id)
                    {
                        ss = (IServerServices)Activator.GetObject(typeof(IServerServices), RootServer.uid.ToString() + "MyRemoteObjectName");
                        if (ss != null)
                        {
                            serversAlive.Add(RootServer);
                            first_request = true;
                            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_REQUEST_VIEW;
                        }
                    }
                    else
                    {
                        Ping_All_Servers();
                        CurrentViewID = new View
                        (
                            nodeid: MyID,
                            sequence: 0,
                            servers_in_View: serversAlive
                        );
                        Im_Root = true;
                        RootServer = new EachServer(MyAddress, MyID);
                        Console.WriteLine("IM ROOT");
                    }
                    STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_RESTART;

                    break;

                case STATE_MACHINE_NETWORK_REQUEST_VIEW:
                    if (first_request == true && Im_Root == false)
                    {
                        ss = (IServerServices)Activator.GetObject(typeof(IServerServices), RootServer.uid.ToString() + "MyRemoteObjectName");
                        ss.RX_ReplicaCommand(new CommandReplicas("REQUEST_VIEW_AND_IMAGE", null, null, MyAddress, MyID));
                        first_request = false;
                        STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_WAIT;
                    }
                    else
                    {
                        try
                        {
                            Console.WriteLine(MyID + "ooooooooooooooooooooooooooooo" + CurrentViewID.Servers_in_View.Count);
                            foreach (EachServer server_alive in CurrentViewID.Servers_in_View)
                            {
                                //if (server_alive.id != MyID)
                                //{
                                    ss = (IServerServices)Activator.GetObject(typeof(IServerServices), server_alive.uid.ToString() + "MyRemoteObjectName");
                                    ss.RX_ReplicaCommand(new CommandReplicas("REQUEST_VIEW", null, null, MyAddress, MyID));
                                //}
                            }
                            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_WAIT;
                        }
                        catch
                        {
                            if (Im_Root == true)
                            {
                                Ping_All_Servers();
                                foreach (EachServer server_alive in serversAlive)
                                {
                                    if (server_alive.id != MyID)
                                    {
                                        view_sequence++;
                                        View tmp = new View(MyID, view_sequence, serversAlive);
                                        ss = (IServerServices)Activator.GetObject(typeof(IServerServices), server_alive.uid.ToString() + "MyRemoteObjectName");
                                        ss.RX_ReplicaCommand(new CommandReplicas("UPDATE_VIEW", tmp, null, MyAddress, MyID));
                                    }
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                    //Console.WriteLine("adeus");
                    STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_RESTART2;
                    break;

                case STATE_MACHINE_NETWORK_CHECK_VIEW:
                    foreach(View proposed_view in Proposed_Views)
                    {
                        if (!GetCurrentViewID().Equals(proposed_view))
                        {
                            getFromRoot = true;
                            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_CHANGE_VIEW;
                            break;
                        }
                    }
                    STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_REQUEST_VIEW;
                    break;

                case STATE_MACHINE_NETWORK_CHANGE_VIEW:
                    if (getFromRoot == true && root_view != null)
                    {
                        SetCurrentViewID(root_view);
                        root_view = null;
                        STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_RESTART;
                    }
                    else
                        STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_UPDATE_ROOT;
                    break;
                case STATE_MACHINE_NETWORK_UPDATE_VIEW:
                    Console.WriteLine("Entrei no update view");
                    Ping_All_Servers();
                    view_sequence++;
                    View new_view = new View(MyID, view_sequence, serversAlive);
                    root_view = new_view;
                    SetCurrentViewID(new_view);
                    Console.WriteLine("UPDATE VIEW______=> " + new_view.ToString());
                    foreach (EachServer server_alive in serversAlive)
                    {
                        if (server_alive.id != MyID)
                        {
                            ss = (IServerServices)Activator.GetObject(typeof(IServerServices), server_alive.uid.ToString() + "MyRemoteObjectName");
                            ss.RX_ReplicaCommand(new CommandReplicas("UPDATE_VIEW", new_view, null, MyAddress, MyID));
                        }
                    }
                    STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_WAIT;
                    break;
                case STATE_MACHINE_NETWORK_UPDATE_ROOT:
                    if (serversAlive.Count == 0 || ((Server.My_Identification.ID - 1) < serversAlive[0].id)) //check if anyone is ROOT
                    {
                        //IM ROOT
                        RootServer = new EachServer(MyAddress, MyID);
                        Ping_All_Servers();
                        View new_view2 = new View(MyID, view_sequence++,serversAlive);
                        root_view = new_view2;
                        SetCurrentViewID(new_view2);
                        Console.WriteLine("IM THE NEW ROOT-----VIEW => " + new_view2.ToString());
                        foreach(EachServer server_alive in serversAlive)
                        {
                            if (server_alive.id != MyID)
                            {
                                ss = (IServerServices)Activator.GetObject(typeof(IServerServices), server_alive.uid.ToString() + "MyRemoteObjectName");
                                ss.RX_ReplicaCommand(new CommandReplicas("UPDATE_VIEW", new_view2 , null, MyAddress, MyID));
                            }
                        }
                    }
                    //else
                    //{
                    //    //Console.WriteLine("MY_IDENTIFICATION: {0}, {1}", Server.My_Identification.ID, (int)serversAlive[0]);
                    //    if ()
                    //    {
                    //        //IM ROOT
                    //        RootServer = new EachServer(MyAddress, MyID);
                    //        View new_view = new View(MyID, view_sequence++);
                    //        root_view = new_view;
                    //        SetCurrentViewID(new_view);
                    //        Console.WriteLine("IM THE NEW ROOT-----VIEW => " + new_view.ToString());
                    //        foreach (EachServer server_alive in serversAlive)
                    //        {
                    //            if (server_alive.id != MyID)
                    //                ss.RX_ReplicaCommand(new CommandReplicas("UPDATE_VIEW", new_view, null, MyAddress, MyID));
                    //        }
                    //    }

                    //}


                    break;
                case STATE_MACHINE_NETWORK_WAIT:
                    while (wait == true) Thread.Sleep(50);
                    wait = true;
                    STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_RESTART;
                    break;

                case STATE_MACHINE_NETWORK_RESTART:
                    Thread.Sleep(3000);
                    STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_REQUEST_VIEW;
                    break;
                case STATE_MACHINE_NETWORK_RESTART2:
                    Thread.Sleep(3000);
                    break;
                case STATE_MACHINE_NETWORK_RESTART3:
                    Thread.Sleep(5000);
                    STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_UPDATE_ROOT;
                    break;
            }
        }

        
        public static void Ping_All_Servers()
        {
            serversAlive.Clear();
            foreach(EachServer s in AllServers)
            {
                if (s.id != MyID)
                {
                    IServerServices sd = (IServerServices)Activator.GetObject(typeof(IServerServices), s.uid.ToString() + "MyRemoteObjectName");
                    try
                    {
                        sd.Ping();
                        if (!serversAlive.Contains(s))
                            serversAlive.Add(s);
                        Console.WriteLine("The server {0} is alive", s.uid.ToString());
                    }
                    catch
                    {
                        Console.WriteLine("The server {0} is dead", s.uid.ToString());
                    }
                }
            }

        }

        ////===================================================================
        ////                       Main program
        ////===================================================================
        /*    private static void NetworkStatusStateMachine()
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
                        Thread.Sleep(60000);

                        /*serversAlive.Clear();

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
                        }

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
            */
    }
}
