using Sandbox;
using System.Collections.Generic;

namespace SvM
{
	public class ChatEntry
	{
		public string Name { get; set; }
		public string Message { get; set; }
		public float Time { get; set; }
	}

	public static partial class Chat
	{
		public static IList<ChatEntry> ChatMessages;

		public static void AddChatEntry( string name, string message )
		{
			ChatEntry entry = new ChatEntry();

			entry.Name = name;
			entry.Message = message;
			entry.Time = Time.Now;

			ChatMessages.Add( entry );
		}

		[ConCmd.Server]
		public static void NetChatEntry( string name, string message )
		{
			AddChatEntry( name, message );
			SendMessageToClient( name, message );
		}

		[ClientRpc]
		public static void SendMessageToClient( string name, string message )
		{
			AddChatEntry( name, message );
			Event.Run( "svm.chatentry" );
		}
	}
}
