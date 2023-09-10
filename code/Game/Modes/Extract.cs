using Sandbox;

namespace SvM
{
	public partial class Extract : GameMode
	{
		public Extract(int objectives, int spyLives, int mercLives) : base(spyLives, mercLives)
		{
			Title = "Extract";
			SpyDescription = "Extract with " + objectives + " of the targets";
			MercDescription = "Eliminate the spies before they can steal and extract the objectives";
			Tasks = objectives;
			IsSpyTask = true;
		}
	}
}
