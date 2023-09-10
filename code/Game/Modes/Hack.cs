using Sandbox;

namespace SvM
{
	public partial class Hack : GameMode
	{
		public Hack(int objectives, int spyLives, int mercLives) : base(spyLives, mercLives)
		{
			Title = "Hack";
			SpyDescription = "Hack " + objectives +" of the targets";
			MercDescription = "Eliminate the spies before they hack the target";
			Tasks = objectives;
			IsSpyTask = true;
		}
	}
}
