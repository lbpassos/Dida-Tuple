using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    public class View
    {
        private int NodeId;
        private int Sequence;

        public View(int Nid)
        {
            NodeId = Nid;
            Sequence = 1;
        }

        public void IncSequence()
        {
            ++Sequence;
        }
        public int GetSequence()
        {
            return Sequence;
        }
        public void SetSequence(int val)
        {
            Sequence = val;
        }
        public override string ToString()
        {
            return "<" + Sequence + "," + NodeId + ">";
        }
        public override bool Equals(object obj)
        {
            View a = obj as View;
            if (a == null)
            {
                return false;
            }
            else
            {
                return (this.Sequence == a.Sequence);
            }
        }
        public override int GetHashCode()
        {
            return NodeId.GetHashCode() + Sequence.GetHashCode();
        }
    }
}
