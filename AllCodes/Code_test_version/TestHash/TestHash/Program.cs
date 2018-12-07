using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHash
{
    class Program
    {
        private static List<Lock> LockedTuples = new List<Lock>(); //Locks in the Tuplespace

        static void Main(string[] args)
        {
            object[] tupleOBJ = new object[1];
            tupleOBJ[0] = new StringEmulator("a");
            MyTuple mt = new MyTuple(tupleOBJ);

            //LockedTuples.Add(mt);

            HashSet<MyTuple> TuplesLocked = new HashSet<MyTuple>();
            TuplesLocked.Add(mt);

            Console.WriteLine(TuplesLocked.Contains(mt));

            Console.ReadLine();

        }
    }
}
