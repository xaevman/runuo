using System;
using RunUO;
using RunUO.Gumps;
using RunUO.Network;

namespace RunUO.Chat
{
	public class ChatSystem
	{
		public static void Initialize()
		{
			EventSink.ChatRequest += new ChatRequestEventHandler( EventSink_ChatRequest );
		}

		private static void EventSink_ChatRequest( ChatRequestEventArgs e )
		{
			e.Mobile.SendMessage( "Chat is not currently supported." );
		}
	}
}