using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestHash
{
    public class StringEmulator
    {
        public string s1;


        public StringEmulator(string ps1)
        {

            s1 = ps1;

        }

        public int GetSize()
        {
            return s1.Length;
        }

        public string GetString()
        {
            return s1;
        }
        public override bool Equals(object o)
        {
            StringEmulator a = o as StringEmulator;
            if (a == null)
            {
                return false;
            }
            else
            {
                if (this.s1.Equals(a.s1) == true)
                {
                    return true;
                }
                else
                {
                    string pattern = "";

                    //Console.WriteLine("DEBUG: {0}", a.GetString());
                    if (a.GetString()[0] == '*') //In the beginning
                    {
                        if (a.GetSize() == 1) //all
                        {
                            return true;
                        }
                        else
                        {
                            pattern += a.GetString().Substring(1) + "$";
                            Regex rx = new Regex(@pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                            MatchCollection matches = rx.Matches(s1);
                            if (matches.Count > 0)
                            {
                                return true;
                            }

                        }
                    }
                    if (a.GetString()[a.GetSize() - 1] == '*') //In the end
                    {
                        pattern += "^" + a.GetString().Substring(0, a.GetSize() - 2);
                        Regex rx = new Regex(@pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        MatchCollection matches = rx.Matches(s1);
                        if (matches.Count > 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }
        public override int GetHashCode()
        {
            return s1.GetHashCode();
        }

        public override string ToString()
        {
            return "\"" + s1 + "\"";
        }

    }
}
