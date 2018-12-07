using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHash
{
    public class MyTuple
    {
        List<object> ObjTuple;

        public MyTuple(object[] obj)
        {
            ObjTuple = new List<object>();

            for (int i = 0; i < obj.Length; ++i)
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
            if (a == null || this.GetSize() != a.GetSize())
            {
                return false;
            }
            else
            {
                for (int i = 0; i < this.GetSize(); ++i)
                {
                    if (this.GetValue(i).Equals(a.GetValue(i)))
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

            for (int i = 0; i < ObjTuple.Count; ++i)
            {
                try
                {
                    tmp += ObjTuple[i].ToString();
                }
                catch (NullReferenceException e)
                {
                    tmp += "null";
                }

                if (i + 1 < ObjTuple.Count)
                {
                    tmp += ",";
                }
            }
            return tmp + ">";
        }


    }
}
