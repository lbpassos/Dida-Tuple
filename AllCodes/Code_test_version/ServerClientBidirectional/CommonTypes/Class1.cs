using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using System.Collections;

namespace ServerClientBidirectional
{
    
    public delegate void delCommsInfo(CommsInfo info);

    
    
    
    
    // This class is created on the server and allows for client to register their existance and
    // a call-back that the server can use to communicate back.
    public class ServerTalk : MarshalByRefObject
    {
        
        private static Queue<ClientWrap> _list = new Queue<ClientWrap>();
        private static Queue<ClientWrap> _listOfPendents = new Queue<ClientWrap>(); //Pendentes TODO

        public void RegisterHostToClient(string UserID, delCommsInfo htc)
        {
            //_list.Add(new ClientWrap(UserID, htc));
            _list.Enqueue(new ClientWrap(UserID, htc));

           
        }

        
        public static bool HasCommands()
        {
            if (_list.Count > 0)
            {
                return true;
            }
            return false;
        }

        public static void SendToClient() // Ideia comprovada. falta implementar o final
        {
            ClientWrap client = _list.Dequeue();

            string message;
            switch ( client.UserID)
            {
                case "add":
                    message = "SOMA";
                    break;
                case "read":
                    message = "LER";
                    break;
                case "take":
                    message = "REMOVER";
                    break;
                default:
                    return;
            }
            client.HostToClient(new CommsInfo(message));
        }
    }






    [Serializable()]
    public class CommsInfo
    {
        private string _Message = "";

        public CommsInfo(string Message)
        {
            _Message = Message;
        }

        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }
    }

    




    
    /// <summary>
    /// This CallbackSink object will be 'anchored' on the client and is used as the target for a callback
    /// given to the server.
    /// </summary>
    public class CallbackSink : MarshalByRefObject
    {
        public event delCommsInfo OnHostToClient;

        public CallbackSink()
        { }

        [OneWay]
        public void HandleToClient(CommsInfo info)
        {
            if (OnHostToClient != null)
                OnHostToClient(info);
        }
    }


    // small private class to wrap the User and the callback together.
    public class ClientWrap
    {
        private string _UserID = "";
        private delCommsInfo _HostToClient = null;

        public ClientWrap(string UserID, delCommsInfo HostToClient)
        {
            _UserID = UserID;
            _HostToClient = HostToClient;
        }

        public string UserID
        {
            get { return _UserID; }
        }

        public delCommsInfo HostToClient
        {
            get { return _HostToClient; }
        }
    }
}
