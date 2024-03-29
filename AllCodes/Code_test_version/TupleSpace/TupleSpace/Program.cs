﻿using System;
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
                try
                {
                    tmp += ObjTuple[i].ToString();
                }
                catch(NullReferenceException e)
                {
                    tmp += "null";
                }
                
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

        /// <summary>
        /// s is without <>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static object GetObjectFromString(string s)
        {
            if( s[0]=='\"') //Its a string
            {
                s = s.Replace("\"", "");
                return new StringEmulator(s);
            }
            else
            {
                int idx = s.IndexOf('(');
                string obj_stringName = s.Substring(0, idx);
                string obj_stringValues = s.Substring(idx + 1, (s.Length-1)-(idx + 1) );
                string[] results = obj_stringValues.Split(',');

                switch (obj_stringName )
                {
                    case "DADTestA":
                        return new DADTestA(Int32.Parse(results[0]), results[1]);
                    case "DADTestB":
                        return new DADTestB(Int32.Parse(results[0]), results[1], Int32.Parse(results[2]));
                    case "DADTestC":
                        return new DADTestC(Int32.Parse(results[0]), results[1], results[2]);
                    default:
                        throw new Exception("Invalid Command: GetObjectFromString");
                }
            }
        }

        static void Main(string[] args)
        {
            TupleSpace ts = new TupleSpace();


            string um = "\"a\"";
            string dois = "DADTestA(1,\"b\")";

            object[] o1 = new object[2];
            o1[0] = GetObjectFromString(um);
            o1[1] = GetObjectFromString(dois);
            MyTuple tuplo_um = new MyTuple(o1);
            Console.WriteLine("Tuplo 1:" + tuplo_um);

            um = "\"a\"";
            
            o1 = new object[1];
            o1[0] = GetObjectFromString(um);
            tuplo_um = new MyTuple(o1);
            Console.WriteLine("Tuplo 2:" + tuplo_um);


            //object[] o1 = { "\"a\"", new DADTestA(1, "b") }; //Cria Tuplo <"a",DADTestA(1, "b")>
            //MyTuple um = new MyTuple( o1 );
            //Console.WriteLine("Tuplo 1:" + um);

            //object[] o1 = { "\"a\"", new DADTestA(1, "b") }; //Cria Tuplo <"a",DADTestA(1, "b")>
            //MyTuple um = new MyTuple(o1);

            //ts.add( um );                                                   //Insere no espaço de tuplos
            //Console.WriteLine("Tuplo:" + ts);

            //object[] o2 = { "\"a\"", "DADTestA" }; //Procura por tuplos <"a",DADTestA>
            //object[] o2 = { "\"a\"", new DADTestA(1, "b") }; //procura por  Tuplo <"a",DADTestA(1, "b")>
            //object[] o2 = { "\"a\"", null }; // procura por todos os tuplos com <"a",null>
            //MyTuple dois = new MyTuple(o2);
            //Console.WriteLine("READ {0}: {1}", dois, ts.read(dois));    //Apanha Tuplo  <"a">

            /*object[] o2 = { new DADTestA(1, "a"), new DADTestB(1, "c", 2), new DADTestC(1, "b", "d") }; //Cria Tuplo <DADTestA(1, "a"),DADTestB(1, "c", 2),DADTestC(1, "b", "d")>
            MyTuple dois = new MyTuple(o2);
            //Console.WriteLine("Tuplo 2:" + dois);

            ts.add(dois);                                                   //Insere no espaço de tuplos
            //Console.WriteLine(ts);

            object[] o3 = { new StringEmulator("a") };                      //Cria Tuplo <"a">
            MyTuple tres = new MyTuple(o3);
            ts.add(tres);                                                   //Insere no espaço de tuplos
            Console.WriteLine("Espaço de tuplos:");
            Console.WriteLine(ts);

            Console.WriteLine( "READ {0}: {1}", dois, ts.read(dois));       //Ler tuplo <DADTestA(1, "a"),DADTestB(1, "c", 2),DADTestC(1, "b", "d")>
            Console.WriteLine("READ {0}: {1}", um, ts.read(um));            //Ler tuplo <"a",DADTestA(1, "b")>

            //object[] c = { new StringEmulator("a") };
            object[] o4 = { new StringEmulator("*") };                      //Ler por WILDCARD tuplo <"*">
            MyTuple quatro = new MyTuple(o4);
            Console.WriteLine("READ {0}: {1}", quatro, ts.read(quatro));    //Apanha Tuplo  <"a">

            object[] o5 = { new StringEmulator("*"), new DADTestB(1, "c", 2), new DADTestC(1, "b", "d") };  //Ler por WILDCARD tuplo  <"*",DADTestB(1, "c", 2),DADTestC(1, "b", "d")>
            MyTuple cinco = new MyTuple(o5);
            Console.WriteLine("READ {0}: {1}", cinco, ts.read(cinco));                                      //Apanha <DADTestA(1, "a"),DADTestB(1, "c", 2),DADTestC(1, "b", "d")>

            ts.take(tres);
            Console.WriteLine();
            Console.WriteLine("Espaço de tuplos:");
            Console.WriteLine(ts);

            object[] o6 = { new StringEmulator("aaaa") };                      //Cria Tuplo <"aaaa">
            MyTuple seis = new MyTuple(o6);
            ts.add(seis);                                                   //Insere no espaço de tuplos

            object[] o7 = { new StringEmulator("*a") };                      //Ler por WILDCARD tuplo <"*a">
            MyTuple sete = new MyTuple(o7);
            Console.WriteLine("READ {0}: {1}", sete, ts.read(sete));    //Apanha Tuplo  <"aaaa">

    */
            Console.ReadLine();


        }
    }
}
