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

        public static TupleSpace ts = new TupleSpace();
        private static CommunicationLayer commLayer = new CommunicationLayer();
        private static List<Command> CommandsAlreadyReceived = new List<Command>();

        private static bool MustFreeze = false;
        //private static bool Root = false;
        private static int DelayMessagesTime;



        /* XL */
        //public static CommunicationLayerReplica CommLayer_forReplica = new CommunicationLayerReplica(); //Replica channel


        public void Ping()
        {
            return;
        }

        // ====================================================================================================
        // Services for other Replicas TSS
        // ====================================================================================================
        public void RX_ReplicaCommand(object cmd) //Receive commands from other replicas
        {
            CommandReplicas a = cmd as CommandReplicas;
            if (a == null)
            {
                return;
            }
            Thread.Sleep(DelayMessagesTime);//Delay Insertion of messages
            //CommLayer_forReplica.InsertCommand(a);

        }

        public static void CheckCommandsInQueue_thread()
        {
            while (true)
            {
                while (MustFreeze == true) ; //FREEZE ****************************
                Thread.Sleep(50);//Min time to check commands
                if (commLayer.GetQueueSize() > 0) //if there is commands
                {
                    Command cmd = commLayer.RemoveFromCommandQueue();
                    MyTuple payload = (MyTuple)cmd.GetPayload();
                    Object tmp;

                    switch (cmd.GetCommand())
                    {
                        case "read":
                            Console.WriteLine("START ========= REGISTER");

                            tmp = ts.Read(payload);
                            MyTuple a = tmp as MyTuple;

                            if (CommandsAlreadyReceived.Contains(cmd) == false) //Test If command is received by the first time
                            {
                                //First time received
                                for (int i=0; i< CommandsAlreadyReceived.Count; ++i)
                                {
                                    if (CommandsAlreadyReceived[i].GetUriFromSender().Equals(cmd.GetUriFromSender()) == true) //Remove the last command sent by the client
                                    {
                                        CommandsAlreadyReceived.RemoveAt(i);
                                        break;
                                    }
                                }
                                CommandsAlreadyReceived.Add(cmd); //add

                                if (a == null) //object does not exist in the tuple space so we put in backlog
                                {
                                    commLayer.InsertInBackLog(cmd);
                                    Console.WriteLine("(ServerService) Comando no Backlog: " + cmd.GetCommand() + " " + cmd.GetPayload().ToString());
                                }
                                else
                                {
                                    
                                    GiveBackResult(cmd.GetUriFromSender(), new Command("read", a, ServerProgram.GetMyAddress(), cmd.GetSequenceNumber()) );
                                }
                            }
                            //Ignore command
                            break;
                        case "add":

                            Console.WriteLine("============ADD: " + cmd.GetUriFromSender());

                            ////
                            if (CommandsAlreadyReceived.Contains(cmd) == false) //Test If command is received by the first time
                            {
                                //First time received
                                for (int i = 0; i < CommandsAlreadyReceived.Count; ++i)
                                {
                                    if (CommandsAlreadyReceived[i].GetUriFromSender().Equals(cmd.GetUriFromSender()) == true) //Remove the last command sent by the client
                                    {
                                        CommandsAlreadyReceived.RemoveAt(i);
                                        break;
                                    }
                                }
                                CommandsAlreadyReceived.Add(cmd); //add

                                ts.Add(payload); //Insert in the tuple space

                                Console.WriteLine("Imagem: ");
                                Console.WriteLine(ts.ToString());


                                GiveBackResult(cmd.GetUriFromSender(), new Command("add", null, ServerProgram.GetMyAddress(), cmd.GetSequenceNumber()));

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
                                            //GiveBackResult(Command_tmp.GetUriFromSender(), a1);
                                            GiveBackResult(cmd.GetUriFromSender(), new Command("read", null, ServerProgram.GetMyAddress(), cmd.GetSequenceNumber()));
                                        }
                                    }
                                    /*else
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
                                    }*/
                                }
                            }
                            ///





                            
                            break;
                        
                        
                        





                        
                    }
                }
            }
        }

        public bool IsRoot()
        {
            return ServerProgram.AmIRoot();
        }





        /// <summary>
        /// Process Command received from the client
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="payload"></param>
        /*public static void DealWithRequestReplicaCommand_thread(CommandReplicas cmd)
        {
            Object tmp;
            View v = null;
            switch (cmd.GetCommand())
            {
                case "INVITATION":
                    v = cmd.GetProposedView();
                    if (v.GetSequence() <= ServerProgram.GetCurrentViewID().GetSequence())//Reject message if Sequence is less or equal than CurrentView
                    {
                        GiveBackResultToReplica(cmd.GetURI(), new CommandReplicas("REFUSE", ServerProgram.GetCurrentViewID(), null, null));
                    }
                    else
                    {
                        GiveBackResultToReplica(cmd.GetURI(), new CommandReplicas("ACCEPTANCE", ServerProgram.GetCurrentViewID(), ts, ServerProgram.GetMyAddress()));
                    }
                    break;
                case "ACCEPTANCE":
                    v = cmd.GetProposedView();
                    if (v.GetSequence() > ServerProgram.GetCurrentViewID().GetSequence())//Accepts mesaage if Sequence > currentViewID
                    {
                        ServerProgram.SetCurrentViewID(v);//Update my view
                        ts = cmd.GetTSS(); //Update TSS
                        GiveBackResultToReplica(cmd.GetURI(), new CommandReplicas("COMMIT", ServerProgram.GetCurrentViewID(), ts, ServerProgram.GetMyAddress()));
                    }
                    break;
                case "COMMIT":
                    ServerProgram.SetCurrentViewID(v);//Update my view
                    ts = cmd.GetTSS(); //Update TSS
                    break;
                case "REFUSE":
                    break;
            }
        }*/


        public void SinkFromReplicas(object cmd) //Receive answers
        {

            CommandReplicas a = cmd as CommandReplicas;
            if (a == null)
            {
                return;
            }

            while (MustFreeze == true) ; //Freeze
            Thread.Sleep(DelayMessagesTime);//Delay Insertion of messages
            //CommLayer_forReplica.InsertCommand(a);

            /*if (mt != null)
            {
                Console.WriteLine("(ClientServices) RECEBI: " + mt.ToString());

            }
            else
            {
                Console.WriteLine("(ClientServices) NADA A RECEBER");
            }
            ClientProgram.AnswerIsReceived();*/

        }

        private static void GiveBackResultToReplica(Uri uri, CommandReplicas cm)
        {

            IServerServices obj = (IServerServices)Activator.GetObject(typeof(IServerServices), uri.AbsoluteUri + "MyRemoteObjectName");
            obj.SinkFromReplicas(cm);

        }
        // ====================================================================================================
        // END Services for other Replicas TSS
        // ====================================================================================================

        /* END XL */

        public bool RX_Command(Command cmd)//Receive Commands do cliente
        {
            Thread.Sleep(DelayMessagesTime);//Delay Insertion of messages

            commLayer.InsertCommand(cmd);
            
            return true;
        }
        private static void GiveBackResult(Uri uri, Command mt)
        {
           
            IClientServices obj = (IClientServices)Activator.GetObject(typeof(IClientServices), uri.AbsoluteUri + "MyRemoteClient");
            obj.sink(mt);
        }




        public Object[] getImage() { return null; } //Request on Init
        public void TakeCommand(Command cmd) { }//Get Commands from ROOT

        public void freeze() { }
        public void unfreeze() { }



    }


}
