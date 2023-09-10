using Sandbox;
using SvM.Player;
using SvM.Player.Base;

namespace SvM.RenderHooks
{

	internal class NoiseHook : RenderHook
	{
		public override void OnStage( SceneCamera target, Stage renderStage )
		{
			if ( renderStage == Stage.AfterTransparent )
			{
				RenderEffect( target );
			}
		}

		public static void RenderEffect( SceneCamera target )
		{
			var attributes = new RenderAttributes();

			Graphics.GrabFrameTexture( "ColorBuffer", attributes );
			Graphics.GrabDepthTexture( "DepthTexture", attributes );
			attributes.Set( "NoiseTexture", Texture.Load( "textures/noise/perlin.vtex" ) );
			Graphics.Blit( Material.Load( "materials/perlin-noise.vmat" ), attributes );
		}
	}
}
