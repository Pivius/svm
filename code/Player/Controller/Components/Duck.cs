using Sandbox;
using SvM.Player.Controller.Base;
using SvM.Utility;
using System.Runtime.InteropServices;

namespace SvM.Player.Components
{
	public partial class DuckComponent : PredictedComponent
	{
		[Net, Predicted]
		public bool IsDucking { get; set; }
		[Net, Predicted]
		public bool Ducked { get; set; }
		[Net, Predicted]
		public bool ShouldDuck { get; set; }
		[Net, Predicted]
		public float DuckSpeed { get; set; } = 0.2f;
		[Net, Predicted]
		public TimeSince DuckTime { get; set; }
		[Net, Predicted]
		public float AnimDuckAmount { get; set; } = 2;

		public void LerpEye()
		{
			if ( IsDucking )
			{
				if ( ShouldDuck )
					Controller.EyeLocalPosition = Vector3.Up * ( MathX.LerpTo( Controller.StandViewOffset, Controller.DuckViewOffset, DuckTime / DuckSpeed ) );
				else
					Controller.EyeLocalPosition = Vector3.Up * ( MathX.LerpTo( Controller.DuckViewOffset, Controller.StandViewOffset, DuckTime / DuckSpeed ) );
			}
			else
			{
				if ( Ducked )
					Controller.EyeLocalPosition = Vector3.Up * (MathX.LerpTo( Controller.StandViewOffset, Controller.DuckViewOffset, 1 ));
				else
					Controller.EyeLocalPosition = Vector3.Up * (MathX.LerpTo( Controller.DuckViewOffset, Controller.StandViewOffset, 1 ));
			}
		}

		public bool CanUnDuck()
		{
			if ( DuckTime < DuckSpeed )
			{
				return false;
			}

			var tr = TraceHelper.TraceHullResult( Controller.Position, Controller.Position, Controller.GetPlayerMins( false ), Controller.GetPlayerMaxs( false ), Player );

			if ( tr.StartedSolid )
			{
				return false;
			}

			return true;
		}

		public void Duck()
		{
			if ( !Ducked )
			{
				ShouldDuck = true;
				IsDucking = true;
				DuckTime = 0;
				Player.SetAnimParameter( "duck", 2 );
			}
		}

		public void Duck(float speed)
		{
			if ( !Ducked )
			{
				DuckSpeed = speed;
				ShouldDuck = true;
				IsDucking = true;
				DuckTime = 0;
				Player.SetAnimParameter( "duck", 2 );
			}
		}

		public void UnDuck()
		{
			if ( Ducked && CanUnDuck() )
			{
				ShouldDuck = false;
				IsDucking = true;
				DuckTime = 0;
				Player.SetAnimParameter( "duck", 0 );
			}
		}

		public void UnDuck( float speed )
		{
			Log.Info( CanUnDuck() );
			if ( Ducked && CanUnDuck() )
			{
				
				DuckSpeed = speed;
				ShouldDuck = false;
				IsDucking = true;
				DuckTime = 0;
				Player.SetAnimParameter( "duck", 0 );
			}
		}

		public override void FrameSimulate()
		{
			LerpEye();

		}

		public override void Simulate()
		{

			if ( DuckTime >= DuckSpeed )
			{
				Ducked = ShouldDuck;
				IsDucking = false;
				//Controller.EyeLocalPosition = Vector3.Up * ( Ducked ? Controller.DuckViewOffset : Controller.StandViewOffset );
			}

			if (IsDucking)
			{
				return;
			}

			if ( Input.Pressed(InputButton.Duck) && Controller.OnGround )
			{
				if ( ( Ducked && CanUnDuck() ) || !Ducked )
				{
					ShouldDuck = !Ducked;
					IsDucking = true;
					DuckTime = 0;
					Player.SetAnimParameter( "duck", ShouldDuck ? AnimDuckAmount : 0 );
				}
			}
		}
	}
}
