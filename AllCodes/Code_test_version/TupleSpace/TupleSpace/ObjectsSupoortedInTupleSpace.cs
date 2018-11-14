using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TupleSpace
{
    

    [Serializable]
    public class DADTestA
    {
        public int i1;
        public string s1;

        public DADTestA(int pi1, string ps1)
        {
            i1 = pi1;
            s1 = ps1;
        }
        public override bool Equals(object o)
        {

            StringEmulator sm = o as StringEmulator; //Test WILDCARD
            if (sm != null)
            {
                if( sm.GetString().Equals("*"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            DADTestA a = o as DADTestA;
            if (a == null)
            {
                return false;
            }
            else
            {
                return ((this.i1 == a.i1) && (this.s1.Equals(a.s1)));
            }
        }
        public override int GetHashCode()
        {
            return i1.GetHashCode() + s1.GetHashCode();
        }

        public override string ToString()
        {
            return "DADTestA(" + i1 + "," + "\"" + s1 + "\")";
        }

        
    }

    [Serializable]
    public class DADTestB
    {
        public int i1;
        public string s1;
        public int i2;

        public DADTestB(int pi1, string ps1, int pi2)
        {
            i1 = pi1;
            s1 = ps1;
            i2 = pi2;
        }

        public override bool Equals(object o)
        {

            StringEmulator sm = o as StringEmulator; //Test WILDCARD
            if (sm != null)
            {
                if (sm.GetString().Equals("*"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            DADTestB b = o as DADTestB;
            if (b == null)
            {
                return false;
            }
            else
            {
                return ((this.i1 == b.i1) && (this.s1.Equals(b.s1)) && (this.i2 == b.i2));
            }
        }
        public override int GetHashCode()
        {
            return i1.GetHashCode() + s1.GetHashCode() + i2.GetHashCode();
        }

        public override string ToString()
        {
            return "DADTestB(" + i1 + "," + "\"" + s1 + "\"" +"," + "\"" + i2 + ")";
        }

    }

    [Serializable]
    public class DADTestC
    {
        public int i1;
        public string s1;
        public string s2;

        public DADTestC(int pi1, string ps1, string ps2)
        {
            i1 = pi1;
            s1 = ps1;
            s2 = ps2;
        }

        public override bool Equals(object o)
        {
            StringEmulator sm = o as StringEmulator; //Test WILDCARD
            if (sm != null)
            {
                if (sm.GetString().Equals("*"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            DADTestC c = o as DADTestC;
            if (c == null)
            {
                return false;
            }
            else
            {
                return ((this.i1 == c.i1) && (this.s1.Equals(c.s1)) && (this.s2.Equals(c.s2)));
            }
        }

        public override int GetHashCode()
        {
            return i1.GetHashCode() + s1.GetHashCode() + s2.GetHashCode();
        }

        public override string ToString()
        {
            return "DADTestC(" + i1 + "," + "\"" + s1 + "\"" + "," + "\"" + s2 + "\"" + ")";
        }

    }





    /// <summary>
    /// Simular String com ""
    /// </summary>
    [Serializable]
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
                if ( this.s1.Equals(a.s1)==true)
                {
                    return true;
                }
                else
                {
                    string pattern = "";

                    //Console.WriteLine("DEBUG: {0}", a.GetString());
                    if ( a.GetString()[0]=='*' ) //In the beginning
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
                    if ( a.GetString()[a.GetSize() - 1]=='*' ) //In the end
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
