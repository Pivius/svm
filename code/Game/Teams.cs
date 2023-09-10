using Sandbox;
using SvM.Player.Base;
using System.Collections.Generic;

namespace SvM
{
	public interface ITeam
	{
		public string Name { get; set; }
		public IList<IClient> Players { get; set; }
		public int Lives { get; }

		public void AddPlayer(IClient client);
	}

	public partial class BaseTeam : BaseNetworkable, ITeam
	{
		[Net]
		private int _lives { get; set; }
		[Net]
		public string Name { get; set; } = "Base";
		[Net]
		public IList<IClient> Players { get; set; }
		public int Lives { 
			get {
				int lives = _lives * Players.Count;

				foreach (IClient client in Players)
				{
					if (client.Pawn is BasePlayer player)
					{
						lives -= player.Deaths;
					}
				}

				return lives;
			}
		}

		public BaseTeam(int lives)
		{
			Players = new List<IClient>();
			_lives = lives;
		}

		public void AddPlayer(IClient client)
		{
			Players.Add(client);
		}

		public void RemovePlayer(IClient client)
		{
			Players.Remove(client);
		}
	}

	public partial class SpyTeam : BaseTeam
	{
		public SpyTeam(int lives) : base(lives)
		{
			Players = new List<IClient>();
			Name = "Spy";
		}
	}

	public partial class MercTeam : BaseTeam
	{
		public MercTeam(int lives) : base(lives)
		{
			Players = new List<IClient>();
			Name = "Mercenary";
		}
	}
}
