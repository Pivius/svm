using Sandbox;
using SvM.Player.Base;
using SvM.Player.Components;
using System;

namespace SvM.Player.Controller.Base
{
	public partial class BaseController : PawnController
	{
		[Net]
		public float SprintMult { get; set; } = 1.5f;
		[Net]
		public float WalkMult { get; set; } = 0.4f;
		[Net]
		public float DuckMult { get; set; } = 0.6f;
		[Net]
		public bool CanRun { get; set; } = true;
		[Net]
		public bool CanWalk { get; set; } = true;
		[Net]
		public float MaxSpeed { get; set; } = 10000f;
		[Net]
		public float JumpPower { get; set; } = 220;
		[Net]
		public float DefaultSpeed { get; set; } = 400f;
		[Net]
		public float StepSize { get; set; } = 16f;
		[Net]
		public float StandableAngle { get; set; } = 45f;

		public bool IsTouchingLadder { get; set; } = false;
		public Vector3 LadderNormal { get; set; }
		public Vector3 OldVelocity { get; set; }
		public bool OnGround
		{
			get => GroundEntity != null;
		}

		[Net, Predicted]
		public bool IsDucked { get; set; } = false;

		// View Properties
		public float ViewOffset { get; set; } = 64.0f;
		[Net]
		public float StandViewOffset { get; set; } = 64f;
		[Net]
		public float DuckViewOffset { get; set; } = 28f;
		[Net]
		public float DeadViewOffset { get; set; } = 14f;

		// Hull Properties
		protected Vector3 mins;
		protected Vector3 maxs;
		[Net]
		public float StandHeight { get; set; } = 72f;
		[Net]
		public float DuckHeight { get; set; } = 32f;
		[Net]
		public float Radius { get; set; } = 16f;
		[Net]
		public AccelComponent AccelComponent { get; set; }
		[Net]
		public int Gravity { get; set; } = 500;
		[Net]
		public int Friction { get; set; } = 8;
		[Net]
		public float StopSpeed { get; set; } = 10f;
		[Net, Predicted]
		public Vector3 MoveTo { get; set; }
		
		public BasePlayer Player => Pawn as BasePlayer;
		
		public BaseController()
		{
			AccelComponent = new AccelComponent();
		}

		///
		/// Player BBox and View Methods
		///

		public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
		{
			if ( liftFeet > 0 )
			{
				start += Vector3.Up * liftFeet;
				maxs = maxs.WithZ( maxs.z - liftFeet );
			}

			var tr = Trace.Ray( start, end )
						.Size( mins, maxs )
						.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
						.Ignore( Pawn )
						.Run();

			return tr;
		}

		public TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
		{
			return TraceBBox( start, end, GetPlayerMins(), GetPlayerMaxs(), liftFeet );
		}

		public TraceResult TraceBBoxLift( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
		{
			if ( liftFeet > 0 )
			{
				start += Vector3.Up * liftFeet;
				end += Vector3.Up * liftFeet;
				maxs = maxs.WithZ( maxs.z - liftFeet );
			}

			var tr = Trace.Ray( start, end )
						.Size( mins, maxs )
						.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
						.Ignore( Pawn )
						.Run();

			return tr;
		}

		public TraceResult TraceBBoxLift( Vector3 start, Vector3 end, float liftFeet = 0.0f )
		{
			return TraceBBoxLift( start, end, GetPlayerMins(), GetPlayerMaxs(), liftFeet );
		}

		public virtual BBox GetHull() => new( mins, maxs );

		public virtual Vector3 GetPlayerMins( bool isDucked )
		{
			return (Radius * Vector3.Right + Radius * Vector3.Backward);
		}

		public virtual Vector3 GetPlayerMaxs( bool isDucked )
		{
			return (Vector3.Up * (isDucked ? DuckHeight : StandHeight) + Radius * Vector3.Left + Radius * Vector3.Forward);
		}

		public virtual Vector3 GetPlayerMins()
		{
			return GetPlayerMins( IsDucked );
		}

		public virtual Vector3 GetPlayerMaxs()
		{
			return GetPlayerMaxs( IsDucked );
		}

		public virtual float GetPlayerViewOffset( bool isDucked )
		{
			return isDucked ? DuckViewOffset : StandViewOffset;
		}

		public virtual float GetViewOffset() => ViewOffset;

		public virtual void SetBBox( Vector3 mins, Vector3 maxs )
		{
			this.mins = mins;
			this.maxs = maxs;
		}

		public virtual void UpdateBBox()
		{
			var mins = GetPlayerMins();
			var maxs = GetPlayerMaxs();

			if ( this.mins != mins || this.maxs != maxs )
			{
				SetBBox( mins, maxs );
			}
		}

		///
		/// Utils
		///

		public virtual void RotatePlayer( Rotation? wishLook = null, float speed = 0.1f )
		{
			// Get the direction we are moving
			Angles inputDirection = InputDirection.EulerAngles + ViewAngles.yaw;
			// Get the rotation of the direction we are moving
			Rotation inputRotation = Rotation.From( new Angles( 0, wishLook == null ? inputDirection.yaw : ((Rotation)wishLook).Yaw(), 0 ) );

			// Interpolate rotation to where we are moving
			if ( InputDirection != Vector3.Zero || wishLook != null )
			{
				Rotation = Rotation.Lerp( Rotation, inputRotation, speed );
			}
		}

		public virtual void OnAnimEvent( string name, int intData, float floatData, Vector3 vectorData, string stringData ) {}
		
		public virtual void UpdateStamina() {}

		public virtual void CheckJumpButton()
		{
			if ( !OnGround )
				return;


			ClearGroundEntity();
			Velocity = Velocity.WithZ( JumpPower );
			AddEvent( "jump" );
		}

		public virtual bool ShouldJump()
		{
			return Input.Pressed( InputButton.Jump );
		}


		///
		/// Movement Utils
		///

		public Vector3 ApplyFriction( Vector3 velocity, float friction, float stopSpeed )
		{
			float speed = velocity.Length;
			float control = MathF.Max( speed, stopSpeed );
			float drop = control * friction * Time.Delta;
			float newSpeed = MathF.Max( speed - drop, 0 );

			if ( newSpeed != speed )
			{
				newSpeed /= speed;
				velocity *= newSpeed;
			}

			return velocity;
		}

		public virtual float GetWalkSpeed()
		{
			bool isWalking = Input.Down( InputButton.Walk ) && CanWalk;
			bool isRunning = Input.Down( InputButton.Run ) && CanRun;
			float multiplier = 1;
			
			if ( isWalking )
				multiplier *= WalkMult;
			else if ( isRunning )
				multiplier *= SprintMult;

			multiplier = IsDucked ? multiplier * DuckMult : multiplier;

			return DefaultSpeed * multiplier;
		}

		public virtual Vector3 WishVel( float strafeSpeed )
		{
			Vector3 forward = ViewAngles.WithPitch( 0 ).Forward;
			Vector3 analogMove = InputDirection;
			float forwardSpeed = analogMove.x * strafeSpeed;
			float sideSpeed = analogMove.y * strafeSpeed;
			Vector3 forwardWish = new Vector3( forward.x, forward.y, 0 ).Normal * forwardSpeed;
			Vector3 sideWish = new Vector3( -forward.y, forward.x, 0 ).Normal * sideSpeed;

			return forwardWish + sideWish;
		}

		public static Vector3 ClipVelocity( Vector3 velocity, Vector3 normal )
		{
			return velocity - (normal * velocity.Dot( normal ));
		}

		public virtual void StepMove()
		{
			MoveHelper mover = new( Position, Velocity );
			mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
			mover.MaxStandableAngle = StandableAngle;

			mover.TryMoveWithStep( Time.Delta, StepSize );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public virtual void Move()
		{
			MoveHelper mover = new( Position, Velocity );
			mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
			mover.MaxStandableAngle = StandableAngle;

			mover.TryMove( Time.Delta );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public virtual void TryPlayerMove()
		{
			MoveHelper mover = new( Position, Velocity );
			mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
			mover.MaxStandableAngle = StandableAngle;

			mover.TryMove( Time.Delta );

			var trace = mover.TraceFromTo( Position, Position );
			var angle = trace.Normal.Angle( Vector3.Up );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public virtual void AirMove()
		{
			Velocity += BaseVelocity;
			TryPlayerMove();
			Velocity -= BaseVelocity;
		}

		public virtual void LadderMove()
		{
			var velocity = WishVelocity;
			float normalDot = velocity.Dot( LadderNormal );
			var cross = LadderNormal * normalDot;
			Velocity = (velocity - cross) + (-normalDot * LadderNormal.Cross( Vector3.Up.Cross( LadderNormal ).Normal ));

			Move();
		}

		public virtual void ApplyAccelerate()
		{
			var wishSpeed = WishVelocity.Length;

			WishVelocity = WishVelocity.WithZ( 0 );
			WishVelocity = WishVelocity.Normal * wishSpeed;
			Velocity = AccelComponent.GetControllerVelocity( this ).WithZ( 0 );
			Velocity += BaseVelocity;
		}

		public virtual void WalkMove()
		{
			ApplyAccelerate();

			try
			{
				if ( Velocity.Length < 1.0f )
				{
					Velocity = Vector3.Zero;
					return;
				}

				var dest = (Position + Velocity * Time.Delta).WithZ( Position.z );
				var pm = TraceBBox( Position, dest );

				if ( pm.Fraction == 1 )
				{
					Position = pm.EndPosition;
					StayOnGround();

					return;
				}

				StepMove();
			}
			finally
			{
				Velocity -= BaseVelocity;
			}

			StayOnGround();
		}

		/// 
		/// Ground Utils
		/// 
		public virtual void UpdateGroundEntity( TraceResult tr )
		{
			GroundNormal = tr.Normal;

			Vector3 oldGroundVelocity = default;
			if ( GroundEntity != null ) oldGroundVelocity = GroundEntity.Velocity;

			bool wasOffGround = GroundEntity == null;

			GroundEntity = tr.Entity;

			if ( GroundEntity != null )
			{
				BaseVelocity = GroundEntity.Velocity;
			}
		}

		public virtual void ClearGroundEntity()
		{
			if ( GroundEntity == null ) return;

			GroundEntity = null;
			GroundNormal = Vector3.Up;
		}

		public virtual void StayOnGround()
		{
			var start = Position + Vector3.Up * 2;
			var end = Position + Vector3.Down * StepSize;
			var trace = TraceBBox( Position, start );

			start = trace.EndPosition;
			trace = TraceBBox( start, end );

			if ( trace.Fraction <= 0 ) return;
			if ( trace.Fraction >= 1 ) return;
			if ( trace.StartedSolid ) return;
			if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > StandableAngle ) return;

			Position = trace.EndPosition;
		}

		public virtual void CategorizePosition( bool bStayOnGround )
		{
			var point = Position - Vector3.Up * 2;
			var vBumpOrigin = Position;

			//
			//  Shooting up really fast.  Definitely not on ground trimed until ladder shit
			//
			float MaxNonJumpVelocity = 140.0f;
			bool bMovingUpRapidly = Velocity.z > MaxNonJumpVelocity;
			_ = Velocity.z > 0;

			bool bMoveToEndPos = false;

			if ( GroundEntity != null ) // and not underwater
			{
				bMoveToEndPos = true;
				point.z -= StepSize;
			}
			else if ( bStayOnGround )
			{
				bMoveToEndPos = true;
				point.z -= StepSize;
			}

			if ( bMovingUpRapidly ) // or ladder and moving up
			{
				ClearGroundEntity();
				return;
			}

			var pm = TraceBBox( vBumpOrigin, point, 4.0f );
			var angle = Vector3.GetAngle( Vector3.Up, pm.Normal );

			if ( pm.Entity == null ||
				angle > StandableAngle )
			{
				ClearGroundEntity();
				bMoveToEndPos = false;

			}
			else
			{
				UpdateGroundEntity( pm );
			}

			if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
			{
				Position = pm.EndPosition;
			}
		}

		///
		/// Simulates
		///

		public override void FrameSimulate()
		{
			base.FrameSimulate();
			
			EyeLocalPosition = Vector3.Up * GetViewOffset();
			EyeRotation = ViewAngles.ToRotation();
		}

		public virtual bool StartMove()
		{

			EyeLocalPosition = Vector3.Up * GetViewOffset();
			EyeRotation = ViewAngles.ToRotation();
			Rotation = Rotation.From( ViewAngles.WithPitch( 0 ) );
			WishVelocity = WishVel( MaxSpeed );

			UpdateBBox();

			return false;
		}

		public virtual bool SetupMove()
		{

			// Start Gravity
			if ( !IsTouchingLadder )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f * Time.Delta );
				Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;
				BaseVelocity = BaseVelocity.WithZ( 0 );
			}

			// Try jumping up ledge

			if ( ShouldJump() )
			{
				CheckJumpButton();
			}

			if ( OnGround )
			{
				Velocity = Velocity.WithZ( 0 );
				Velocity = ApplyFriction( Velocity, Friction, StopSpeed );
			}

			if ( !IsTouchingLadder )
				WishVelocity = WishVelocity.WithZ( 0 );

			bool bStayOnGround = false;
		
			if ( IsTouchingLadder )
			{
				LadderMove();
			}
			else if ( OnGround )
			{
				bStayOnGround = true;
				WalkMove();
			}
			else
			{
				AirMove();

				if ( MathF.Abs( Velocity.x ) == 1 && MathF.Abs( Velocity.y ) == 1 )
					Velocity = Velocity.WithX( 0 ).WithY( 0 );
			}

			if ( !IsTouchingLadder )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f * Time.Delta );
			}

			if ( OnGround )
			{
				Velocity = Velocity.WithZ( 0 );
			}

			CategorizePosition( bStayOnGround );

			return false;
		}
		public virtual void EndMove()
		{
			UpdateStamina();
			OldVelocity = Velocity;
		}

		public override void Simulate()
		{
			if ( StartMove() )
				return;

			if ( SetupMove() )
				return;

			EndMove();
		}
	}
}
