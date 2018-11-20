using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Projeto_DAD
{
    class ServerService : MarshalByRefObject, IServerServices
    {

        private TupleSpace ts = new TupleSpace(); //Imagem of each server

        private static bool Root = false;
        private bool Repeat = false;

        public static void setRoot(bool value)
        {
            Root = value;
        }

        public void Ping()
        {
            return;
        }

        public bool isRoot()
        {
            return Root;
        }



        /* SEM CALLBACK
         */ 
        public void Add(MyTuple mt)
        {
            ts.add(mt);
            Console.WriteLine("ADICIONADO: " + mt);
            Console.WriteLine("TOTAL DEPOIS DO ADD: " + ts);
        }

        public object Read(MyTuple mt)
        {
            Console.WriteLine("ENCONTRADO: " + ts.read(mt));
            Console.WriteLine("TOTAL DEPOIS DO READ: " + ts);
            return ts.read(mt);
        }

        public object Take(MyTuple mt)
        {
            object o = ts.take(mt);
            Console.WriteLine("REMOVIDO: " + mt);
            Console.WriteLine("TOTAL DEPOIS DO TAKE: " + ts);
            return o;
        }

    }

}
