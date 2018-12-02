using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Timeout
{
    class Timeout
    {
        private Stopwatch st;
        private int id;
        private int Timeout_ms;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i">ServerID</param>
        /// <param name="t">Timeout constant</param>
        public Timeout(int i, int t)
        {
            st = Stopwatch.StartNew();
            id = i;
            Timeout_ms = t;
        }

        public bool IsTimeOut()
        {
            if (st.ElapsedMilliseconds > Timeout_ms)
            {
                return true;
            }
            return false;
        }

        public void ResetTimeOut()
        {
            st.Reset();
        }

        public override bool Equals(object obj)
        {
            Timeout t = obj as Timeout;
            if (t == null)
            {
                return false;
            }
            return this.id == t.id;
        }
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return " s: " + st.ElapsedMilliseconds;
        }

        public static void Main(string[] args)
        {
            Timeout um = new Timeout(1, 1000);
            Thread.Sleep(500);
            Console.WriteLine(um);
            Thread.Sleep(500);
            Console.WriteLine(um);
            Thread.Sleep(500);
            Console.WriteLine(um);
            Thread.Sleep(500);
            Console.WriteLine(um);

            Console.ReadLine();
        }

    }
}
