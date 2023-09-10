using Sandbox;

namespace SvM
{
	public partial class Deathmatch : GameMode
	{
		public Deathmatch(int spyLives, int mercLives) : base(spyLives, mercLives)
		{
			Title = "Deathmatch";
			SpyDescription = "Elminate the mercs";
			MercDescription = "Eliminate the spies";
		}

		public override void TryFinishGame()
		{
			ITeam winner = GetLosingTeam();

			if ( winner != null )
			{
				// End the game
				EndGame( winner );
			}
		}
	}
}
