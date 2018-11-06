using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common_types;

namespace Server
{
    class ServerService : MarshalByRefObject, IServerToServer
    {

        /*ServerProgram server;

        public ServerService()
        {
            server = new ServerProgram(Server.uri, Server.id);
        }
        */

        public void Ping()
        {
            return;
        }



    }
}
