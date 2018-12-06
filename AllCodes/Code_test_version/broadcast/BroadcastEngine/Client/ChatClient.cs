using System;
using System.Runtime.Remoting;
using System.Threading;

using KnownObjects;
using Belikov.GenuineChannels;
using Belikov.GenuineChannels.BroadcastEngine;
using Belikov.GenuineChannels.DotNetRemotingLayer;

namespace Client
{
	/// <summary>
	/// ChatClient demostrates simple client application.
	/// </summary>
	class ChatClient : MarshalByRefObject, IChatClient
	{
		/// <summary>
		/// Sigleton instance.
		/// </summary>
		public static ChatClient Instance = new ChatClient();

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Console.WriteLine("Sleep for 3 seconds.");
			Thread.Sleep(TimeSpan.FromSeconds(3));

			Console.WriteLine("Configuring Remoting environment...");
			System.Configuration.ConfigurationSettings.GetConfig("DNS");
			GlobalEventContainer.GenuineChannelsGlobalEvent += new GenuineChannelsGlobalEventHandler(GenuineChannelsEventHandler);
			RemotingConfiguration.Configure("Client.exe.config");

			Console.WriteLine(".NET Remoting has been configured from Client.exe.config file.");

			for(;;)
			{
				try
				{
					IChatRoom iChatRoom = (IChatRoom) Activator.GetObject(typeof(IChatRoom), "gtcp://127.0.0.1:8737/ChatRoom.rem");
					iChatRoom.AttachClient(ChatClient.Instance);

					for(;;)
					{
						Console.WriteLine("Enter a message to send or an empty string to exit.");

						string str = Console.ReadLine();
						if (str.Length <= 0)
							return ;

						iChatRoom.SendMessage(str);
						Console.WriteLine("Message \"{0}\" has been sent.", str);
					}
				}
				catch(Exception ex)
				{
					Console.WriteLine("Exception: {0}. Stack trace: {1}.", ex.Message, ex.StackTrace);
				}

				Console.WriteLine("Next attempt to connect to the server will be in 3 seconds.");
				Thread.Sleep(3000);
			}
		}

		public static void GenuineChannelsEventHandler(object sender, GlobalEventArgs e)
		{
			Console.WriteLine("Global event: {0}, Url: {1}, Exception: {2}", e.EventType, e.Url, e.SourceException);
			if (e.EventType == GlobalEventTypes.GTcpConnectionClosed && 
				e.SourceException is OperationException && 
				((OperationException) e.SourceException).OperationErrorMessage.ErrorIdentifier.IndexOf("ServerHasBeenRestarted") > -1 )
			{
				// server has been restarted so we have to register our listener again
				IChatRoom iChatRoom = (IChatRoom) Activator.GetObject(typeof(IChatRoom), "gtcp://127.0.0.1:8737/ChatRoom.rem");
				iChatRoom.AttachClient(ChatClient.Instance);
			}
		}

		/// <summary>
		/// Receives messages from the server.
		/// </summary>
		/// <param name="message">The message.</param>
        public object ReceiveMessage(string message)
		{
			Console.WriteLine("Message \"{0}\" has been received from the server.", message);
			return null;
		}

		/// <summary>
		/// This is to insure that when created as a Singleton, the first instance never dies,
		/// regardless of the expired time.
		/// </summary>
		/// <returns></returns>
		public override object InitializeLifetimeService()
		{
			return null;
		}
	}
}
