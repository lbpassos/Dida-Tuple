using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting.Messaging;


namespace Server
{

    delegate void UpdateHandler(string typeOfCommand, string val); //Delegate to remote call

    class ServerProgram
    {

        private const int STATE_MACHINE_NETWORK_START = 0;
        private const int STATE_MACHINE_NETWORK_KEEP_ALIVE = 1;
        private const int STATE_MACHINE_NETWORK_IM_ROOT = 2;
        private const int STATE_MACHINE_NETWORK_END = 3;

        private const int STATE_MACHINE_REPLICATION_INIT = 0;
        private const int STATE_MACHINE_REPLICATION_READ = 1;
        private const int STATE_MACHINE_REPLICATION_ADD = 2;
        private const int STATE_MACHINE_REPLICATION_TAKE = 3;

        private static int STATE_MACHINE_REPLICATION;
        private static int STATE_MACHINE_NETWORK;

        private static readonly object Lock = new object();

        private ArrayList serversAlive;
        private static ArrayList serversWhoAnswered; //servers who manage to update their image sucessfully 
        private int Root_id;

        private Random rand;

        //private Image image;

        public ServerProgram()
        {

            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START;
            STATE_MACHINE_REPLICATION = STATE_MACHINE_REPLICATION_INIT;

            serversAlive = new ArrayList( Server.AllServers.Count );
            serversWhoAnswered = new ArrayList(Server.AllServers.Count);

            Root_id = 0;
            rand = new Random();
            

            //new Thread(() => PingLoop()).Start();
            new Thread(() => NetworkStatusLoop()).Start();
            new Thread(() => StateMachineReplicationLoop()).Start();

        }


        public static int checkSMRState()
        {
            return STATE_MACHINE_REPLICATION;
        }

        public static bool changeSMRState(int state)
        {
            if( STATE_MACHINE_REPLICATION==STATE_MACHINE_REPLICATION_INIT)
            {
                STATE_MACHINE_REPLICATION = state;
                return true;
            }
            return false;
            
        }



        private static void OnCallEnded(IAsyncResult ar)
        {
            UpdateHandler handler = ((AsyncResult)ar).AsyncDelegate as UpdateHandler;
            int index = (int)ar.AsyncState;

            handler.EndInvoke(ar);

            lock (Lock)
            {
                serversWhoAnswered.Add(index);
            }
            
        }


        private void NetworkStatusLoop()
        {
            while (STATE_MACHINE_NETWORK != STATE_MACHINE_NETWORK_END)
            {
                NetworkStatusStateMachine();
                Thread.Sleep(1000);
            }
        }

        private void StateMachineReplicationLoop()
        {
            while (true)
            {
                StateMachineReplication();
                Thread.Sleep(1000);
            }
        }

        private void StateMachineReplication()
        {
            switch (STATE_MACHINE_REPLICATION)
            {
                case STATE_MACHINE_REPLICATION_INIT:

                    break;
                case STATE_MACHINE_REPLICATION_READ:
                    break;
                case STATE_MACHINE_REPLICATION_ADD:
                    break;
                case STATE_MACHINE_REPLICATION_TAKE:
                    break;
            }

        }



        private void NetworkStatusStateMachine()
        {
            bool flag = false;

            switch (STATE_MACHINE_NETWORK)
            {
                case STATE_MACHINE_NETWORK_START:
                    //Console.WriteLine("IN STATE_MACHINE_NETWORK_START: BEGIN");
                    serversAlive.Clear();

                    for (int i = 0; i < Server.AllServers.Count; i++)
                    {
                        if (Server.AllServers[i].ID == Server.My_Identification.ID) //Avoid ping himself
                        {
                            continue;
                        }

                        try
                        {

                            ServerService obj = (ServerService)Activator.GetObject(typeof(ServerService), Server.AllServers[i].UID.AbsoluteUri + "MyRemoteObjectName");
                            //Console.WriteLine("CHECK: {0}", Server.AllServers[i].UID.AbsoluteUri);

                            if ( obj.isRoot()==true )
                            {
                                Root_id = i; //ID of the current root node

                                Console.WriteLine("My old Image is: {0}\n", ServerService.getImageRepresentation()); //OLD

                                ServerService.setImage( obj.getImage() ); //get the image of the root
                                

                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_KEEP_ALIVE;
                                flag = true;
                                Console.WriteLine("ROOT is {0}", Server.AllServers[i].UID.AbsoluteUri);
                                Console.WriteLine("My new Image is: {0}\n", ServerService.getImageRepresentation()); //NEW
                                break;
                            }
                            else
                            {
                                serversAlive.Add(i); //Server ID
                                Console.WriteLine("ALIVE: {0}", Server.AllServers[i].UID.AbsoluteUri);

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
                            ServerService.setRoot(true);
                            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_IM_ROOT;
                        }
                        else
                        {
                            //Console.WriteLine("MY_IDENTIFICATION: {0}, {1}", Server.My_Identification.ID, (int)serversAlive[0]);
                            if ( (Server.My_Identification.ID-1) < (int)serversAlive[0] ) 
                            {
                                //ROOT
                                ServerService.setRoot(true);
                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_IM_ROOT;
                            }
                                
                        }
                    }
                    break;
                case STATE_MACHINE_NETWORK_KEEP_ALIVE:
                    //Console.WriteLine("IN STATE_MACHINE_NETWORK_KEEP_ALIVE");
                    //ping root 
                    try
                    {
                        ServerService obj = (ServerService)Activator.GetObject(typeof(ServerService), Server.AllServers[Root_id].UID.AbsoluteUri + "MyRemoteObjectName");
                        obj.Ping();
                        //Console.WriteLine("ALIVE: {0}", Server.AllServers[i].UID.AbsoluteUri);
                        Thread.Sleep(rand.Next(1000)); //keepalive

                    }
                    catch (Exception e)
                    {
                        STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START;

                        //Console.WriteLine("IN STATE_MACHINE_NETWORK_KEEP_ALIVE: EXCEPTION");

                        //Console.WriteLine("GOING TO STATE_MACHINE_NETWORK_START");
                        //Console.WriteLine("DEAD: {0}", Server.AllServers[i].UID.AbsoluteUri);
                        //Console.WriteLine(e);
                    }
                    break;
                case STATE_MACHINE_NETWORK_IM_ROOT:
                    Console.WriteLine("ROOT SERVER");

                    ServerService.add("OLA"); //I'm root. I add OLA

                    STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_END;
                    break;
                case STATE_MACHINE_NETWORK_END:
                    break;

            }
        }



        private void PingLoop()
        {
            while (true)
            {
                PingAllServers();
                Thread.Sleep(1000);
            }
        }

        private void PingAllServers()
        {
            
            if(Server.AllServers.Count <= 1)
            {
                return;
            }

            for (int i = 0; i < Server.AllServers.Count; i++)
            {
                if( Server.AllServers[i].ID == Server.My_Identification.ID) //Avoid ping to himself
                {
                    continue;
                }

                try
                {
                    
                    ServerService obj = (ServerService)Activator.GetObject(typeof(ServerService), Server.AllServers[i].UID.AbsoluteUri + "MyRemoteObjectName");
                    obj.Ping();
                    Console.WriteLine("ALIVE: {0}", Server.AllServers[i].UID.AbsoluteUri);

                }
                catch (Exception e)
                {
                    Console.WriteLine("DEAD: {0}", Server.AllServers[i].UID.AbsoluteUri);
                    //Console.WriteLine(e);
                }
            }
            
        }
    }
}
