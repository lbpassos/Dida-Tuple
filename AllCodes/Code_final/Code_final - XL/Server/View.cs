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
        private int NodeId;
        private ArrayList NodesInView;
        private int Sequence;

        public View(int Nid)
        {
            NodeId = Nid;
            NodesInView = new ArrayList();
            //NodesInView.Add(Nid); //starts with the only node
            
            NodesInView.Add(NodeId); //Add(NodeId);
            Sequence = 1;

        }

        public int GetNodeID()
        {
            return NodeId;
        }
        /*public void ClearView()
        {
            NodesInView.Clear();
            NodesInView.Add(NodeId); //starts with the only node
        }*/
        public void AddNodeInView(int n)
        {
            NodesInView.Add(n);
            
        }
        public void RemoveNode(int id)
        {
            NodesInView.Remove(id);
        }
        public int GetSizeOfView()
        {
            return NodesInView.Count;
        }

        public int GetElementOfView(int pos)
        {
            return (int)NodesInView[pos];
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
            string tmp = "<" + Sequence + "," + NodeId + ">" + "[ ";
            foreach(int s in NodesInView)
            {
                tmp += s + " ";
            }
            tmp += "]";
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
                return (this.Sequence == a.Sequence);
            }
        }
        public override int GetHashCode()
        {
            return NodeId.GetHashCode() + Sequence.GetHashCode();
        }
    }
}
