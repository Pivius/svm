using Sandbox;
using SvM.Player.Base;
using SvM.RenderHooks;
using System.Linq;

namespace SvM.Player.VisionModes
{
	public class ThermalVision : BaseVision
	{
		public override void OnEnable() 
		{
			//Camera.Main.ExcludeTags.Add( "player" );
			Camera.Main.FindOrCreateHook<DepthHook>();
			Camera.Main.FindOrCreateHook<ThermalHook>();
			Camera.Main.FindOrCreateHook<NoiseHook>();
			Camera.Main.FindOrCreateHook<BlurHook>();
			
		}
		public override void OnDisable() 
		{
			//Camera.Main.ExcludeTags.Remove( "player" );
			Camera.Main.RemoveAllHooks<DepthHook>();
			Camera.Main.RemoveAllHooks<ThermalHook>();
			Camera.Main.RemoveAllHooks<NoiseHook>();
			Camera.Main.RemoveAllHooks<BlurHook>();
		}
		public override void OnFrame() 
		{ 
			if ( !Enabled )
				return;


		}
	}
}
