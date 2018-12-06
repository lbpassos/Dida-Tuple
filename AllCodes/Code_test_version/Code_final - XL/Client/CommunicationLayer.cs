using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    class CommunicationLayer
    {
        private static Queue CommandQueue; //received commands
        private static List<Command> BackLogCommands; //commands received but there is no answer yet for them
       

        public CommunicationLayer()
        {
            CommandQueue = Queue.Synchronized( new Queue() ); //thread-safe queue
            BackLogCommands = new List<Command>();
            //new Thread(() => CheckCommandsInQueue_thread()).Start();
        }

        public void InitFieds(Queue q, List<Command> l)
        {
            CommandQueue = q;
            BackLogCommands = l;
        }


        public void InsertCommand(Command c)
        {
            CommandQueue.Enqueue(c); //Command in Queue
        }

        public Command RemoveFromCommandQueue()
        {
            return (Command)CommandQueue.Dequeue(); //Command in Queue
        }

        public int GetQueueSize()
        {
            return CommandQueue.Count;
        }

        public void InsertInBackLog(Command c)
        {
            BackLogCommands.Add(c);
        }

        public int GetBackLogSize()
        {
            return BackLogCommands.Count;
        }

        public Command GetBackLogCommand(int pos)
        {
            return BackLogCommands[pos];
        }

        public void RemoveFromBackLog(int pos)
        {
            BackLogCommands.RemoveAt(pos);
        }

        public Queue GetQueue()
        {
            return CommandQueue;
        }

        public List<Command> GetBackLog()
        {
            return BackLogCommands;
        }
 
    }


}
