using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common_types;

namespace Server
{
    class ServerService : MarshalByRefObject, IServerServices
    {

        public static List<string> lista = new List<string>();

        private static bool Root = false;

        public static void setRoot(bool value)
        {
            Root = value;
        }


        //Implement Interface IServerToServer
        public void Ping()
        {
            return;
        }        
        public bool isRoot()
        {
            return Root;
        }
        

    }
}
