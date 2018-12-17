using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    class Lock
    {
        private Uri OwnerUri;
        private HashSet<MyTuple> TuplesLocked;

        public Lock(Uri owner)
        {
            OwnerUri = owner;
            TuplesLocked = new HashSet<MyTuple>();
        }

        public bool Insert(MyTuple mt)
        {
            return TuplesLocked.Add(mt);
        }

        public bool IsIn(MyTuple mt) //check if tuple is in lock
        {
            Console.WriteLine("==========================================TUPLO " + TuplesLocked.Contains(mt));
            return TuplesLocked.Contains(mt);
        }

        public Uri GetOwner()
        {
            return OwnerUri;
        }

        public HashSet<MyTuple> GetSet()
        {
            return TuplesLocked;
        }

        public override bool Equals(object obj)
        {
            Lock a = obj as Lock;
            if (a == null)
            {
                return false;
            }
            return OwnerUri.Equals(a.GetOwner());
        }

        public override int GetHashCode()
        {
            return OwnerUri.GetHashCode();
        }
    }
}
