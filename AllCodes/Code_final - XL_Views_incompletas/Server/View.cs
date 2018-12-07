using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Projeto_DAD
{
    [Serializable]
    public class View
    {
        public int NodeID;
        public int Sequence;
        public List<EachServer> Servers_in_View;

        public View(int nodeid, int sequence, List<EachServer> servers_in_View)
        {
            NodeID = nodeid;

            Sequence = 1;

            Servers_in_View = servers_in_View;
        }

        public int GetNodeID()
        {
            return NodeID;
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
            string tmp = "[" + Sequence + "," + NodeID + "]";
            return tmp;
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
                return (this.Sequence == a.Sequence && this.NodeID==a.NodeID && this.Servers_in_View.Equals(a.Servers_in_View));
            }
        }
        public override int GetHashCode()
        {
            return NodeID.GetHashCode() + Sequence.GetHashCode();
        }
    }
}
