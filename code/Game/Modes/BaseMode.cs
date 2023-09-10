using Sandbox;

namespace SvM
{
	public partial class GameMode 
	{
		public string Title = "Base";
		public string SpyDescription = "Game description for spies";
		public string MercDescription = "Game description for mercs";
		public SpyTeam Spies;
		public MercTeam Mercs;
		public bool IsSpyTask = true;
		public int Tasks = 1;
		public int CurrentTasks = 0;

		public GameMode(int spyLives, int mercLives) 
		{
			Spies = new SpyTeam(spyLives);
			Mercs = new MercTeam(mercLives);
		}

		// Get the team with no lives if it exists
		public virtual ITeam GetLosingTeam()
		{
			if (Spies.Lives <= 0)
			{
				return Spies;
			}
			else if (Mercs.Lives <= 0)
			{
				return Mercs;
			}

			return null;
		}

		public virtual void TryFinishGame()
		{
			ITeam winner = GetLosingTeam();

			if ( CurrentTasks >= Tasks )
			{
				winner = IsSpyTask ? Spies : Mercs;
			}

			if ( winner != null )
			{
				// End the game
				EndGame( winner );
			}
		}

		public virtual ITeam EndGame(ITeam winner)
		{
			if (winner == Spies)
			{
				Log.Info("Spies win!");
			}
			else if (winner == Mercs)
			{
				Log.Info("Mercs win!");
			}
			else
			{
				Log.Info("Draw!");
			}

			return winner;
		}
	}
}
