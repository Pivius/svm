using Sandbox;

namespace SvM
{
	public partial class Normal : GameMode
	{
		public Normal(int objectives, int spyLives, int mercLives) : base(spyLives, mercLives)
		{
			Title = "Normal";
			SpyDescription = "Finish the objectives";
			MercDescription = "Eliminate the spies before they can finish their objectives";
			Tasks = objectives;
			IsSpyTask = true;
		}
	}
}
