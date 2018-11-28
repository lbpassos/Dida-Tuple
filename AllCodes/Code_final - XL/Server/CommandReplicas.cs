using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_DAD
{
    [Serializable]
    class CommandReplicas
    {
        private string cmd;
        private object[] payload; //View and Tuplespace(TSS)
        private Uri Sender_uri;


        public CommandReplicas(string command, View v, TupleSpace ts, Uri add)
        {
            cmd = command;
            payload = new object[] { v, ts };
            Sender_uri = add;
        }

        public string GetCommand()
        {
            return cmd;
        }

        public View GetProposedView()
        {
            return (View)payload[0];
        }

        public TupleSpace GetTSS()
        {
            return (TupleSpace)payload[1];
        }

        public Uri GetURI()
        {
            return Sender_uri;
        }
    }
}
