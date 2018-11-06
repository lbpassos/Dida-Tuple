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

        public void Ping()
        {
            return;
        }



    }
}
