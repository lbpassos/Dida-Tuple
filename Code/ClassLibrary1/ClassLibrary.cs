using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    public interface IServerServices
    {
        void Add(Tuple<int, string> tuple); //Adds a tuple to the tuple space

        Tuple<int, string> Read(Predicate<Tuple<int, string>> tuple);  //Reads a tuple from the tuple space which matches the schema.

        Tuple<int, string> Take(Predicate<Tuple<int, string>> tuple); // Removes a tuple from the tuple space which matches the schema.

        void Wait(int milliseconds); //Delays the execution of the next command for x milliseconds.

        void Begin_Repeat(int repetitions); //Repeats x number of times the commands following this command and before the next end-repeat.It is not possible to have another begin-repeat command before this loop is closed by a end-repeat command.

        void End_repeat(); //Closes a repeat loop.

        //To Finish?
    }

    public interface IClientServices
    {
        //TODO
    }

    public interface IPuppetMasterServices
    {
        //TODO
    }

    public interface IPCSServices
    {
        //TODO
    }

    public class TupleObject
    {
        private int a;
        private string b;

        public TupleObject(int a, string b)
        {
            this.a = a;
            this.b = b;
        }
    }

    public class Tuple
    {
        private string[] stringList;
        private TupleObject[] objectsList;

        public Tuple(string[] list)
        {
            this.stringList = list;
        }

        public Tuple(TupleObject[] list)
        {
            this.objectsList = list;
        }
    }

}
