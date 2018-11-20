using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace Projeto_DAD
{
    class CallbackModule
    {
        
        
        /*private ServerTalk _ServerTalk = null;      // this object lives on the server
        private CallbackSink _CallbackSink = null;  // this object lives here on the client
        private bool message_received = false;
        private string message;

        public CallbackModule(int port)
        {
            // creates a client object that 'lives' here on the client.
            _CallbackSink = new CallbackSink();

            // hook into the event exposed on the Sink object so we can transfer a server 
            // message through to this class.
            _CallbackSink.OnHostToClient += new delCommsInfo(CallbackSink_OnHostToClient);

            // Register a client channel so the server can communicate back - it needs a channel
            // opened for the callback to the CallbackSink object that is anchored on the client!
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);


            // now create a transparent proxy to the server component
            //_ServerTalk = (ServerTalk)Activator.GetObject(typeof(ServerTalk), "tcp://localhost:9000/MyRemoteObject");

        }

        public void CreateTransparentProxy(string root)
        {
            _ServerTalk = (ServerTalk)Activator.GetObject(typeof(ServerTalk), root+"/MyRemoteObject");
        }

        public void SendMessage(string msg)
        {

            // Register ourselves to the server with a callback to the client sink.
            _ServerTalk.RegisterHostToClient(msg, new delCommsInfo(_CallbackSink.HandleToClient));

        }



        void CallbackSink_OnHostToClient(CommsInfo info)
        {
            message_received = true;
            message = info.Message;
        }




        public bool IsMessageReceived()
        {
            return message_received;
        }




        public string GetMessage()
        {
            message_received = false;
            return message;
        }*/
    }
}
