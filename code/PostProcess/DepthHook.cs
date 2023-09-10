using Sandbox;
using SvM.Player;
using SvM.Player.Base;
using System.Linq;

namespace SvM.RenderHooks
{

	internal class DepthHook : RenderHook
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

			Graphics.GrabDepthTexture( "DepthTexture", attributes );
			Graphics.Blit( Material.Load( "materials/depth-material.vmat" ), attributes );
		}
	}
}
