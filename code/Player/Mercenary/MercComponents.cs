using Sandbox;
using SvM.Player.Components;

namespace SvM.Player
{
	public partial class MercPlayer
	{
		[Net]
		private bool ComponentsCreated { get; set; } = false;

		public void CreateDefaultComponents()
		{
			DestroyComponents();
			ComponentsCreated = true;
		}

		public void DestroyComponents()
		{
			if ( ComponentsCreated )
			{

			}
		}
	}
}
