using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TupleSpace
{
    




    /// <summary>
    /// Represents Each Tuple
    /// </summary>
    class MyTuple
    {
        List<object> ObjTuple;
        
        public MyTuple(object[] obj)
        {
            ObjTuple = new List<object>();

            for (int i = 0; i<obj.Length; ++i)
            {
                ObjTuple.Add(obj[i]);
            }
        }

        
        public int GetSize()
        {
            return ObjTuple.Count;
        }

        public object GetValue(int pos)
        {
            return ObjTuple[pos];
        }

        
        
        /// <summary>
        /// Completely Equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            MyTuple a = obj as MyTuple;
            if (a == null || this.GetSize()!=a.GetSize() )
            {
                return false;
            }
            else
            {
                for(int i=0; i<this.GetSize(); ++i)
                {
                    if( this.GetValue(i).Equals(a.GetValue(i)) )
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            string tmp = "<";

            for (int i=0; i<ObjTuple.Count; ++i)
            {
                tmp += ObjTuple[i].ToString();
                if(i+1 < ObjTuple.Count)
                {
                    tmp += ",";
                }
            }         
            return tmp + ">";
        }

        
    }


    /// <summary>
    /// Space of Tuples
    /// </summary>
    class TupleSpace
    {
        List<MyTuple> mySpace;

        public TupleSpace()
        {
            mySpace = new List<MyTuple>();
        }

        public void add(MyTuple mt)
        {
            mySpace.Add( mt );
        }

        

        public object read(MyTuple mt)
        {
   
            for (int i=0; i<mySpace.Count; ++i)
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
            foreach(MyTuple i in mySpace)
            {
                tmp += i + "\n";
            }
            return tmp;
        }

        



    }

    class Program
    {
        static void Main(string[] args)
        {
            TupleSpace ts = new TupleSpace();


            

            object[] o1 = { new StringEmulator("a"), new DADTestA(1, "b") };
            MyTuple um = new MyTuple( o1 );
            //Console.WriteLine("Tuplo 1:" + um);

            ts.add( um );

            object[] o2 = { new DADTestA(1, "a"), new DADTestB(1, "c", 2), new DADTestC(1, "b", "d") };
            MyTuple dois = new MyTuple(o2);
            //Console.WriteLine("Tuplo 2:" + dois);

            ts.add(dois);
            //Console.WriteLine(ts);

            object[] o3 = { new StringEmulator("a") };
            MyTuple tres = new MyTuple(o3);
            ts.add(tres);
            Console.WriteLine("Espaço de tuplos:");
            Console.WriteLine(ts);

            Console.WriteLine( "READ {0}: {1}", dois, ts.read(dois));
            Console.WriteLine("READ {0}: {1}", um, ts.read(um));

            //object[] c = { new StringEmulator("a") };
            object[] o4 = { new StringEmulator("*") };
            MyTuple quatro = new MyTuple(o4);
            Console.WriteLine("READ {0}: {1}", quatro, ts.read(quatro));

            object[] o5 = { new StringEmulator("*"), new DADTestB(1, "c", 2), new DADTestC(1, "b", "d") };
            MyTuple cinco = new MyTuple(o5);
            Console.WriteLine("READ {0}: {1}", cinco, ts.read(cinco));

            ts.take(tres);
            Console.WriteLine("Espaço de tuplos:");
            Console.WriteLine(ts);

            Console.ReadLine();


        }
    }
}
