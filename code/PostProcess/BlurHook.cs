using Sandbox;
using SvM.Player;

namespace SvM.RenderHooks
{
	//[SceneCamera.AutomaticRenderHook]
	internal class BlurHook : RenderHook
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
			Graphics.Blit( Material.Load( "materials/gaussian-blur.vmat" ), attributes );

		}
	}
}
