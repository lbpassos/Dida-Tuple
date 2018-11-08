using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_types
{
    // Server service to other servers
    public interface IServerServices
    {
        bool isRoot(); //check if node is root
        void Ping();
    }


    //Client service to servers
    public interface IClientServices
    {
      
    }

}
