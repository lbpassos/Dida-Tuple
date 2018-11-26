using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    [Serializable]
    class TupleSpace
    {
        List<MyTuple> mySpace;
       

        public TupleSpace()
        {
            mySpace = new List<MyTuple>();
        }
        //Constructor with image
        public TupleSpace(List<MyTuple> l)
        {
            mySpace = l;
        }


        public void Add(MyTuple mt)
        {
            mySpace.Add(mt);
        }

        public object Read(MyTuple mt)
        {

            for (int i = 0; i < mySpace.Count; ++i)
            {
                if (mySpace[i].Equals(mt)) //Finds the first one. Returns
                {
                    return mySpace[i];
                }

            }
            return null;
        }

        public object Take(MyTuple mt)
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
            return null;
        }

        //Get Current Image
        public List<MyTuple> GetImage()
        {
            return mySpace;
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
