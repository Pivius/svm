using Sandbox;
using SvM.RenderHooks;
using System.Linq;

namespace SvM.Player.VisionModes
{
	public class NightVision : BaseVision
	{
		public float OriginalBrightness = 0;
		public Color OriginalSky;
		public PointLightEntity NightVisionLight;
		public override void OnEnable() 
		{
			Camera.Main.FindOrCreateHook<NoiseHook>();
			Camera.Main.FindOrCreateHook<NightVisionHook>();


			foreach ( var entity in Entity.All.OfType<EnvironmentLightEntity>() )
			{
				OriginalBrightness = entity.Brightness;
				entity.Brightness = entity.Brightness;
				//OriginalSky = entity.SkyColor;
				//entity.SkyColor = Color.White;
			}
		}
		public override void OnDisable() 
		{
			Camera.Main.RemoveAllHooks<NoiseHook>();
			Camera.Main.RemoveAllHooks<NightVisionHook>();

			foreach ( var entity in Entity.All.OfType<EnvironmentLightEntity>() )
			{
				entity.Brightness = 0;
				//entity.SkyColor = OriginalSky;
			}
		}
		public override void OnFrame() 
		{
			if ( !Enabled )
				return;

		}
	}
}
