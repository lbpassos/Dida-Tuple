using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_types
{
    // Server service to other servers
    public interface IServerToServer
    {

        bool isRoot(); //check if node is root
        void Ping();
        

        
    }


}
