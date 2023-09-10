using System;
using System.Collections.Generic;
using System.Numerics;
using Sandbox;

using SvM.Player.Components;
using SvM.Player.Controller.Base;
using SvM.Utility;

namespace SvM.Player.Controller
{
	public partial class MercController : BaseController
	{
		public float Accelerate = 5;
		public new MercPlayer Player => (MercPlayer)Pawn;

		public MercController()
		{
			JumpPower = 160;
			SprintMult = 1.75f;
			DefaultSpeed = 250f;
			AccelComponent = new AccelComponent();
			CanRun = false;
			Radius = 10;
		}

		public override void FrameSimulate()
		{
			base.FrameSimulate();

			EyeLocalPosition = Vector3.Up * GetViewOffset();
			EyeRotation = ViewAngles.ToRotation();
		}

		public virtual void AccelerateAir(float accelerate, float maxSpeed )
		{
			var forward = Rotation.Forward.WithZ( 0 );
			var dot = forward.Dot( WishVelocity );

			if ( dot < 0)
				forward = -forward;

			Velocity = Velocity.AddClamped( forward * accelerate, maxSpeed );
		}

		public virtual void DoJump()
		{
			ClearGroundEntity();
			Velocity = Velocity.WithZ( JumpPower );
			AddEvent( "jump" );
		}

		public override void CheckJumpButton()
		{
			if ( OnGround )
			{
				DoJump();
			}
		}

		public override bool ShouldJump()
		{
			return Input.Pressed( InputButton.Jump );
		}

		public override void OnAnimEvent( string name, int intData, float floatData, Vector3 vectorData, string stringData )
		{
		}

		public override void AirMove()
		{
			base.AirMove();

			if ( Input.Down( InputButton.Forward ) || Input.Down( InputButton.Back ) )
				AccelerateAir( Accelerate, 50 );

		}
		public override bool StartMove()
		{
			EyeLocalPosition = Vector3.Up * GetViewOffset();
			EyeRotation = ViewAngles.ToRotation();
			WishVelocity = WishVel( MaxSpeed );
			Rotation = Rotation.From( ViewAngles.WithPitch( 0 ) );
			UpdateBBox();

			if ( Input.Pressed( InputButton.Duck ) )
			{
				if ( !IsDucked )
				{
					AddEvent( "duck" );
				}
				else
				{
					AddEvent( "stand" );
				}

				IsDucked = !IsDucked;
			}

			if (IsDucked)
			{

			}
			return false;
		}

		public override bool SetupMove()
		{
			if (base.SetupMove())
				return true;

			return false;
		}

		public override void EndMove()
		{
		}
	}
}
