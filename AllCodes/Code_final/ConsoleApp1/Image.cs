using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    class TupleSpace
    {
        List<MyTuple> mySpace;

        public TupleSpace()
        {
            mySpace = new List<MyTuple>();
        }

        public void add(MyTuple mt)
        {
            mySpace.Add(mt);
        }



        public object read(MyTuple mt)
        {

            for (int i = 0; i < mySpace.Count; ++i)
            {
                if (mySpace[i].Equals(mt)) //Finds the first one. Returns
                {
                    return mySpace[i];
                }

            }
            return false;
        }

        public object take(MyTuple mt)
        {

            for (int i = 0; i < mySpace.Count; ++i)
            {
                if (mySpace[i].Equals(mt)) //Finds the first one. Returns
                {
                    object tmp = mySpace[i];
                    mySpace.Remove(mySpace[i]);
                    return tmp;

                }

            }
            return false;
        }

        public override string ToString()
        {
            string tmp = "";
            foreach (MyTuple i in mySpace)
            {
                tmp += i + "\n";
            }
            return tmp;
        }
    }
}
