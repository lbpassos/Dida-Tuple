using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Messaging;

using KnownObjects;
using Belikov.GenuineChannels.BroadcastEngine;
using Belikov.GenuineChannels.DotNetRemotingLayer;

namespace Server
{
	/// <summary>
	/// Chat server implements server that configures Genuine Server TCP Channel and implements
	/// chat server behavior.
	/// </summary>
	class ChatServer : MarshalByRefObject, IChatRoom
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				// setup remoting
				System.Configuration.ConfigurationSettings.GetConfig("DNS");
				GlobalEventContainer.GenuineChannelsGlobalEvent += new GenuineChannelsGlobalEventHandler(GenuineChannelsEventHandler);
				RemotingConfiguration.Configure("Server.exe.config");

				// bind the server
				RemotingServices.Marshal(new ChatServer(), "ChatRoom.rem");

				_dispatcher.BroadcastCallFinishedHandler += new BroadcastCallFinishedHandler(ChatServer.BroadcastCallFinishedHandler);
				_dispatcher.CallIsAsync = true;
				_caller = (IChatClient) _dispatcher.TransparentProxy;

				Console.WriteLine("Server has been started. Press enter to exit.");
				Console.ReadLine();
			}
			catch(Exception ex)
			{
				Console.WriteLine("Exception: {0}. Stack trace: {1}.", ex.Message, ex.StackTrace);
			}
		}

		private static Dispatcher _dispatcher = new Dispatcher(typeof(IChatClient));
		private static IChatClient _caller;

		/// <summary>
		/// Attaches the client.
		/// </summary>
		/// <param name="iChatClient">Client to attach.</param>
		public void AttachClient(IChatClient iChatClient)
		{
			if (iChatClient == null)
				return ;

			_dispatcher.Add((MarshalByRefObject) iChatClient);
		}

		/// <summary>
		/// Sends message to all clients.
		/// </summary>
		/// <param name="message">Message to send.</param>
		/// <returns>Number of clients having received this message.</returns>
		public void SendMessage(string message)
		{
			Console.WriteLine("\"{0}\" message will be sent to all clients.", message);

			_caller.ReceiveMessage(message);
		}

		public static void GenuineChannelsEventHandler(object sender, GlobalEventArgs e)
		{
			Console.WriteLine("Global event: {0}, Url: {1}, Exception: {2}", e.EventType, e.Url, e.SourceException);
		}

		/// <summary>
		/// Is called by dispatcher when broadcast call is finished.
		/// Can be used to analyze call results.
		/// </summary>
		/// <param name="dispatcher">Source dispatcher.</param>
		/// <param name="message">Source message.</param>
		/// <param name="resultCollector">Result collector.</param>
		public static void BroadcastCallFinishedHandler(Dispatcher dispatcher, IMessage message, ResultCollector resultCollector)
		{

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
