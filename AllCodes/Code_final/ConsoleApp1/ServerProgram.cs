using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Collections;


namespace Projeto_DAD
{
    class ServerProgram
    {

        private const int STATE_MACHINE_NETWORK_START = 0;
        private const int STATE_MACHINE_NETWORK_KEEP_ALIVE = 1;
        private const int STATE_MACHINE_NETWORK_IM_ROOT = 2;
        private const int STATE_MACHINE_NETWORK_END = 3;

        private static int STATE_MACHINE_NETWORK;

        private ArrayList serversAlive;
        private int Root_id;

        private Random rand;

        public ServerProgram()
        {

            STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_START;
            serversAlive = new ArrayList(Server.AllServers.Count);
            Root_id = 0;
            rand = new Random();

            //new Thread(() => PingLoop()).Start();
            new Thread(() => NetworkStatusLoop()).Start();

        }
        
        private void NetworkStatusLoop()
        {
            while (STATE_MACHINE_NETWORK != STATE_MACHINE_NETWORK_END)
            {
                NetworkStatusStateMachine();
                //Thread.Sleep(1000);
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
                            Console.WriteLine("CHECK: {0}", Server.AllServers[i].UID.AbsoluteUri);

                            if (obj.isRoot() == true)
                            {
                                Root_id = i; //ID of the current root node
                                STATE_MACHINE_NETWORK = STATE_MACHINE_NETWORK_KEEP_ALIVE;
                                flag = true;
                                Console.WriteLine("ROOT is {0}", Server.AllServers[i].UID.AbsoluteUri);
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
                            if ((Server.My_Identification.ID - 1) < (int)serversAlive[0])
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
                    Console.WriteLine("I'M THE ROOT SERVER");
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

            if (Server.AllServers.Count <= 1)
            {
                return;
            }

            for (int i = 0; i < Server.AllServers.Count; i++)
            {
                if (Server.AllServers[i].ID == Server.My_Identification.ID) //Avoid ping to himself
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
