using Sandbox;
using SvM.Player;
using System.Collections.Generic;
using System.Linq;

namespace SvM
{
	public partial class SvMGame
	{
		public static SpyTeam Spies { get; private set; }
		public static MercTeam Mercs { get; private set; }
		public static IList<IClient> ReadyPlayers { get; private set; }
		public static bool IsGameRunning { get; private set; }

		public static IClient FindClientById( int id )
		{
			return Game.Clients.FirstOrDefault( c => c.Id == id );

		}

		public static void ReadyPlayer( IClient client, bool? toggle = null )
		{
			if ( client == null )
				return;

			bool isReady = IsReady( client );

			if ( toggle == null )
			{
				if ( isReady )
				{
					ReadyPlayers.Remove( client );
				}
				else
				{
					ReadyPlayers.Add( client );
				}
			}
			else
			{
				if ( (bool)toggle && isReady )
				{
					ReadyPlayers.Remove( client );
				}
				else if ( (bool)toggle && !isReady )
				{
					ReadyPlayers.Add( client );
				}
			}
		}

		public static bool IsAllReady()
		{
			return ReadyPlayers.Count == Game.Clients.Count;
		}

		public static bool IsReady( IClient client )
		{
			return ReadyPlayers.Contains( client );
		}

		public static BaseTeam GetTeam( IClient client )
		{
			if ( Spies.Players.Contains( client ) )
			{
				return Spies;
			}
			else if ( Mercs.Players.Contains( client ) )
			{
				return Mercs;
			}

			return null;
		}

		public static bool IsInTeam( IClient client )
		{
			if ( Spies.Players.Contains( client ) || Mercs.Players.Contains( client ) )
			{
				return true;
			}

			return false;
		}

		public static void SetTeam( IClient client, BaseTeam team )
		{
			if ( client == null || IsReady( client ) )
				return;

			BaseTeam oldTeam = GetTeam( client );

			if ( oldTeam != null )
			{
				oldTeam.RemovePlayer( client );
			}

			if ( team == Spies )
			{
				Spies.AddPlayer( client );
			}
			else if ( team == Mercs )
			{
				Mercs.AddPlayer( client );
			}
		}

		public static void SwitchTeam( IClient client )
		{
			if ( client == null || IsReady( client ) )
				return;

			BaseTeam oldTeam = GetTeam( client );
			BaseTeam team = oldTeam == Spies ? Mercs : Spies;

			if ( oldTeam != null )
			{
				oldTeam.RemovePlayer( client );
			}

			if ( team == Spies )
			{
				Spies.AddPlayer( client );
			}
			else if ( team == Mercs )
			{
				Mercs.AddPlayer( client );
			}
		}

		public static void AutoAssignTeam( IClient client )
		{
			if ( client == null )
				return;

			if ( Spies.Players.Count > Mercs.Players.Count )
			{
				Mercs.AddPlayer( client );
				SetTeamClient( client.Id, false );
			}
			else
			{
				Spies.AddPlayer( client );
				SetTeamClient( client.Id, true );
			}
		}

		public static void TryStartGame()
		{
			if ( !IsAllReady() )
				return;


			IsGameRunning = true;

			foreach ( var client in Spies.Players )
			{
				client.Pawn?.Delete();

				client.Pawn = new SpyPlayer();
			}

			foreach ( var client in Mercs.Players )
			{
				client.Pawn?.Delete();

				client.Pawn = new MercPlayer();
			}
		}

		[ConCmd.Server]
		public static void NetSwitchTeam( int clientId )
		{
			var client = FindClientById( clientId );

			SwitchTeam( client );
			SetTeamClient( clientId, GetTeam( client ) == Spies );

		}

		[ClientRpc]
		public static void SetTeamClient( int clientId, bool isSpy )
		{
			var client = FindClientById( clientId );

			SwitchTeam( client );
			SetTeam( client, isSpy ? Spies : Mercs );
			Event.Run( "svm.teamSwitch" );
		}

		[ConCmd.Server]
		public static void NetReadyUp( int clientId )
		{
			var client = FindClientById( clientId );
			Log.Info( "ReadyUp" );
			ReadyPlayer( client );
			Log.Info( IsReady( client ) );
			ReadyClient( clientId, IsReady( client ) );
		}

		[ClientRpc]
		public static void ReadyClient( int clientId, bool toggle )
		{
			var client = FindClientById( clientId );

			ReadyPlayer( client, toggle );
			Event.Run( "svm.ready" );
		}

		[ConCmd.Server]
		public static void NetStartGame()
		{
			TryStartGame();
			ClientStartGame();
		}

		[ClientRpc]
		public static void ClientStartGame()
		{
			IsGameRunning = true;
			Event.Run( "svm.start" );
		}
	}
}
