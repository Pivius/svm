using Sandbox;
using SvM.Player;
using SvM.Player.Base;
using SvM.UI;
using System.Collections.Generic;

namespace SvM
{
	public partial class SvMGame : GameManager
	{
		public static SvMGame Instance => Current as SvMGame;
		[Net]
		public GameMode GameType { get; set; }
		[Net, Predicted]
		public TimeUntil Timer { get; set; }

		public SvMGame()
		{
			Spies = new SpyTeam( 1 );
			Mercs = new MercTeam( 1 );
			ReadyPlayers = new List<IClient>();
			Chat.ChatMessages = new List<ChatEntry>();

			if ( Game.IsServer )
			{
				Game.TickRate = 100;
			}
			else if ( Game.IsClient )
			{
				Game.RootPanel?.Delete( true );
				Game.RootPanel = new Hud();
			}
		}

		[ConCmd.Server( "rsb" )]
		public static void rsb()
		{
			// Respawn as base
			if ( ConsoleSystem.Caller == null )
				return;


			ConsoleSystem.Caller.Pawn?.Delete();

			ConsoleSystem.Caller.Pawn = new BasePlayer();
		}

		[ConCmd.Server( "rss" )]
		public static void rss()
		{
			// Respawn as spy
			if ( ConsoleSystem.Caller == null )
				return;

			ConsoleSystem.Caller.Pawn?.Delete();

			ConsoleSystem.Caller.Pawn = new SpyPlayer();
		}

		[ConCmd.Server( "rsm" )]
		public static void rsm()
		{
			// Respawn as merc
			if ( ConsoleSystem.Caller == null )
				return;


			ConsoleSystem.Caller.Pawn?.Delete();

			ConsoleSystem.Caller.Pawn = new MercPlayer();
		}

		public virtual void OnKilled( Entity pawn, Entity killer )
		{
			// TODO: Kill notification ui
			// Notify all players that the player has been killed and who killed them
			// If the killer is null, the player was killed by the world
			if ( pawn == null )
			{
				return;
			}

			BasePlayer player = pawn as BasePlayer;

			if ( killer is BasePlayer killerPlayer )
			{
				// TODO: Kill feed ui
				// TODO: Add to frags
			}
			else if ( killer == null )
			{

			}

			// Reset death timer and add to deaths
			player.DeathTimer = RespawnTime;
			player.Deaths++;
			GameType.TryFinishGame();


			// TODO: Respawn Timer UI
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();
		}
		public override void ClientJoined( IClient client )
		{
			base.ClientJoined( client );

			AutoAssignTeam( client );
		}
	}
}
