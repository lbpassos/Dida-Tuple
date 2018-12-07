using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    /// <summary>
    /// Deals with commands from other replicas. ViewChange for instance
    /// </summary>
    class CommunicationLayerReplica
    {
        private static Queue CommandQueue; //received commands
        //private static List<Command> BackLogCommands; //commands received but there is no answer yet for them
       

        public CommunicationLayerReplica()
        {
            CommandQueue = Queue.Synchronized( new Queue() ); //thread-safe queue
            //BackLogCommands = new List<Command>();
            //new Thread(() => CheckCommandsInQueue_thread()).Start();
        }

        /*public void InitFieds(Queue q, List<Command> l)
        {
            CommandQueue = q;
            BackLogCommands = l;
        }*/


        public void InsertCommand(CommandReplicas c)
        {
            CommandQueue.Enqueue(c); //Command in Queue
        }

        public CommandReplicas RemoveFromCommandQueue()
        {
            return (CommandReplicas)CommandQueue.Dequeue(); //Command in Queue
        }

        public int GetQueueSize()
        {
            return CommandQueue.Count;
        }

 
    }


}
