using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Projeto_DAD
{
    class ServerService : MarshalByRefObject, IServerServices
    {

        private static TupleSpace ts = new TupleSpace();
        private static CommunicationLayer commLayer = new CommunicationLayer();
        private static bool MustFreeze = false;
        private static bool Root = false;

        private static int DelayMessagesTime;

        public static void setRoot(bool value)
        {
            Root = value;
        }

        public void Ping()
        {
            return;
        }

        public bool isRoot()
        {
            return Root;
        }

        public Object[] getImage()
        {
            Object[] o = new Object[3];
            o[0] = ts.GetImage();
            o[1] = commLayer.GetQueue();
            o[2] = commLayer.GetBackLog();
            return o;
        }

        public void RX_Command(Command cmd) // Get Commands from clients
        {
            Thread.Sleep(DelayMessagesTime);//Delay Insertion of messages

            commLayer.InsertCommand(cmd);
            if (Root == true) //Only the root server send updates
            {
                ServerProgram.UpdateAll(cmd);
            }
        }

        public void TakeCommand(Command cmd)
        {//Get Commands from ROOT
            Thread.Sleep(DelayMessagesTime);//Delay Insertion of messages
            commLayer.InsertCommand(cmd);
            Console.WriteLine("(ServerService) Comando no Queue: " + cmd.GetCommand() + " " + cmd.GetPayload().ToString());
        }

        public static void SetTupleSpace(Object img)
        {
            ts = (TupleSpace)img;
        }
        public static string GetTupleSpaceRepresentation()
        {
            return ts.ToString();
        }
        public static void SetCommunicationLayer(Queue q, List<Command> l)
        {
            commLayer.InitFieds(q, l);
        }


        public static void CheckCommandsInQueue_thread()
        {
            while(true) 
            {
                while (MustFreeze == true) ; //FREEZE ****************************
                Thread.Sleep(50);//Min time to check commands
                if (commLayer.GetQueueSize() > 0) //if there is commands
                {
                    Command cmd = commLayer.RemoveFromCommandQueue();
                    MyTuple payload = (MyTuple)cmd.GetPayload();

                    new Thread(() => DealWithRequest_thread(cmd, payload)).Start();

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

        public static void SetDelayMessageTime(int delay_ms)
        {
            DelayMessagesTime = delay_ms;
        }

        public static void DealWithRequest_thread(Command cmd, MyTuple payload)
        {
            Object tmp;

            switch (cmd.GetCommand())
            {
                case "read":
                    tmp = ts.Read(payload);
                    MyTuple a = tmp as MyTuple;

                    Console.WriteLine("Imagem: ");
                    Console.WriteLine(ServerService.GetTupleSpaceRepresentation());

                    if (a == null) //object does not exist in the tuple space so we put in backlog
                    {
                        commLayer.InsertInBackLog(cmd);
                        Console.WriteLine("(ServerService) Comando no Backlog: " + cmd.GetCommand() + " " + cmd.GetPayload().ToString());
                    }
                    else
                    {
                        GiveBackResult(cmd.GetUriFromSender(), a);
                    }
                    break;
                case "add":
                    ts.Add(payload);

                    Console.WriteLine("Imagem: ");
                    Console.WriteLine(ServerService.GetTupleSpaceRepresentation());

                    GiveBackResult(cmd.GetUriFromSender(), null);
                    MyTuple a1;
                    //serach in the backlog
                    for (int i = 0; i < commLayer.GetBackLogSize(); ++i)
                    {
                        Command Command_tmp = commLayer.GetBackLogCommand(i);
                        if (Command_tmp.GetCommand().Equals("read"))
                        {
                            tmp = ts.Read((MyTuple)Command_tmp.GetPayload());
                            a1 = tmp as MyTuple;
                            if (a1 != null)
                            {
                                commLayer.RemoveFromBackLog(i);
                                i = -1;
                                Console.WriteLine("(ServerService) Comando Atendido e Removido do Backlog: " + cmd.GetCommand() + " " + cmd.GetPayload().ToString());
                                GiveBackResult(Command_tmp.GetUriFromSender(), a1);
                            }
                        }
                        else
                        {
                            if (Command_tmp.GetCommand().Equals("take"))
                            {
                                tmp = ts.Take((MyTuple)Command_tmp.GetPayload());
                                a1 = tmp as MyTuple;
                                if (a1 != null) //test if is a MyTuple
                                {
                                    commLayer.RemoveFromBackLog(i);
                                    i = -1;
                                    GiveBackResult(Command_tmp.GetUriFromSender(), a1);
                                }

                            }
                        }
                    }
                    break;
                case "take":
                    tmp = ts.Take(payload);
                    MyTuple a2 = tmp as MyTuple;
                    if (a2 == null) //object does not exist in the tuple space so we put in backlog
                    {
                        commLayer.InsertInBackLog(cmd);
                        Console.WriteLine("(ServerService) Comando no Backlog: " + cmd.GetCommand() + " " + cmd.GetPayload().ToString());
                    }
                    else
                    {
                        Console.WriteLine("Imagem: ");
                        Console.WriteLine(ServerService.GetTupleSpaceRepresentation());

                        GiveBackResult(cmd.GetUriFromSender(), a2);
                    }
                    break;
            }
        }


        private static void GiveBackResult(Uri uri, MyTuple mt)
        {
            if (Root == true) //Only sends back to client if is ROOT SERVER
            {
                IClientServices obj = (IClientServices)Activator.GetObject(typeof(IClientServices), uri.AbsoluteUri + "MyRemoteClient");
                obj.sink(mt);
            }
        }
    }


}
