using Sandbox;
using SvM.Player;
using SvM.Player.Base;
using System.Linq;

namespace SvM.RenderHooks
{

	internal class ThermalHook : RenderHook
	{
		public override void OnStage( SceneCamera target, Stage renderStage )
		{
			if ( renderStage == Stage.AfterTransparent)
			{
				RenderEffect( target );
			}
		}

		public static void RenderEffect( SceneCamera target )
		{
			var mat = Material.Load( "materials/thermal.vmat" );

			foreach ( var player in Entity.All.OfType<BasePlayer>() )
			{
				Graphics.Render( player.SceneObject, material: mat );
			}
		}
	}
}
