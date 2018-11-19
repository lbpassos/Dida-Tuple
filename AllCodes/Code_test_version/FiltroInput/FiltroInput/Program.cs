using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace FiltroInput
{
    class Program
    {
        public static void Main()
        {
            
            List<string> myList = new List<string>();


            Regex rx = new Regex(@"(\w+\(((,?((\d)+),?)|(,?""\w+"",?))+\))|(""\w+"")|(\w+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Define a test string.        
            string text = "\"a\",DADTestA(1,\"a\"),DADTestB(1,\"c\",2),DADTestC(1,\"b\",\"d\")";
            //string text = "\"2\"";
            //string text = "\"a\",\"b\",\"c\",\"d\",\"e\"";
            //string text = "DADTestA,DADTestB,DADTestC";

            // Find matches.
            MatchCollection matches = rx.Matches(text);

            //Indices
            int[] idx = new int[matches.Count];

            // Report on each match.
            int i = 0;
            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;

                idx[i++] = groups[0].Index;


            }


            Console.WriteLine("Encontrei {0}", idx.Length);

            if (idx.Length > 0)
            {
                for (i = 0; i < idx.Length; ++i)
                {
                    //Console.WriteLine("idx[{0}]:", idx[i]);
                    //Console.WriteLine("Encontrei {0}", idx.Length);

                    if (i + 1 >= idx.Length)
                    {
                        myList.Add(text.Substring(idx[i]));
                    }
                    else
                    {
                        myList.Add(text.Substring(idx[i], (idx[i + 1] - 1)- idx[i] ));
                    }
                }
            }

            foreach (string str in myList)
            {
                Console.WriteLine(str);
            }
            Console.ReadLine();
        }
    }
}
