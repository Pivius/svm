using Sandbox;
using SvM.Player.Components;

namespace SvM.Player
{
	public partial class SpyPlayer
	{
		[Net, Predicted]
		public LedgeComponent LedgeComp { get; set; }
		[Net, Predicted]
		public RollComponent RollComp { get; set; }
		[Net, Predicted]
		public DuckComponent DuckComp { get; set; }
		[Net, Predicted]
		public PipeComponent PipeComp { get; set; }
		[Net]
		private bool ComponentsCreated { get; set; } = false;

		public void CreateDefaultComponents()
		{
			DestroyComponents();
			LedgeComp = new()
			{
				Player = this,
			};

			RollComp = new()
			{
				Player = this,
			};

			DuckComp = new()
			{
				Player = this,
			};

			PipeComp = new()
			{
				Player = this,
			};

			ComponentsCreated = true;
		}

		public void DestroyComponents()
		{
			if ( ComponentsCreated )
			{
				LedgeComp.Delete();
				RollComp.Delete();
				DuckComp.Delete();
				PipeComp.Delete();
			}
		}
	}
}
