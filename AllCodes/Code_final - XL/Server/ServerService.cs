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
       // private static CommunicationLayer commLayer = new CommunicationLayer();
        private static bool MustFreeze = false;
        //private static bool Root = false;
        private static int DelayMessagesTime;


        /* XL */
        private static CommunicationLayerReplica CommLayer_forReplica = new CommunicationLayerReplica(); //Replica channel

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
            CommLayer_forReplica.InsertCommand(a);

        }

        public static void CheckCommandsInQueueFromReplica_thread() //Check commands send by the replicas
        {
            while (true)
            {
                while (MustFreeze == true) ; //FREEZE ****************************
                Thread.Sleep(50);//Min time to check commands
                if (CommLayer_forReplica.GetQueueSize() > 0) //if there is commands
                {
                    CommandReplicas cmd = CommLayer_forReplica.RemoveFromCommandQueue();

                    new Thread(() => DealWithRequestReplicaCommand_thread(cmd)).Start(); //Launch thread to attend command

                }
            }
        }

        /// <summary>
        /// Process Command received from the client
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="payload"></param>
        public static void DealWithRequestReplicaCommand_thread(CommandReplicas cmd)
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
        }


        public void SinkFromReplicas(object cmd) //Receive answers
        {

            CommandReplicas a = cmd as CommandReplicas;
            if (a == null)
            {
                return;
            }

            while (MustFreeze == true) ; //Freeze
            ServerProgram.InsertCommand(a);

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

            IServerServices obj = (IServerServices)Activator.GetObject(typeof(IServerServices), uri.AbsoluteUri + "MyRemoteClient");
            obj.SinkFromReplicas(cm);

        }
        // ====================================================================================================
        // END Services for other Replicas TSS
        // ====================================================================================================

        /* END XL */

        public void RX_Command(Command cmd) { } //Receive Commands do cliente
        public Object[] getImage() { return null; } //Request on Init
        public void TakeCommand(Command cmd) { }//Get Commands from ROOT

        public void freeze() { }
        public void unfreeze() { }



    }


}
