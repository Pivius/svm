using System;
using System.Collections.Generic;
using System.Numerics;
using Sandbox;

using SvM.Player.Components;
using SvM.Player.Controller.Base;
using SvM.Utility;

namespace SvM.Player.Controller
{
	public partial class SpyController : BaseController
	{
		[Net, Predicted]
		public bool IsCovering { get; set; }
		[Net, Predicted]
		public bool MovingToCover { get; set; }
		[Net, Predicted]
		public Vector3 CoverPos { get; set; }
		[Net, Predicted]
		public Vector3 CoverDirection { get; set; }
		[Net, Predicted]
		public float CoverDistance { get; set; }
		[Net, Predicted]
		public bool AtEdge { get; set; }
		[Net, Predicted]
		public bool WasOnGround { get; set; }
		[Net, Predicted]
		public bool CanTurnToEdge { get; set; }
		[Net, Predicted]
		public Vector3 EdgePos { get; set; }
		public float Accelerate = 5;
		[Net, Predicted]
		public TimeSince Landed { get; set; }
		public new bool IsDucked
		{
			get => Player.DuckComp.Ducked;
		}

		public new SpyPlayer Player => (SpyPlayer)Pawn;

		public SpyController()
		{
			JumpPower = 275;
			SprintMult = 1.75f;
			DefaultSpeed = 250f;
			AccelComponent = new AccelComponent();
			CanRun = false;
			Radius = 10;
		}

		public override Vector3 GetPlayerMins()
		{
			return GetPlayerMins( IsDucked );
		}

		public override Vector3 GetPlayerMaxs()
		{
			return GetPlayerMaxs( IsDucked );
		}
		public override float GetWalkSpeed()
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
			if (IsDucked)
			{
				Player.DuckComp.UnDuck( 0.2f );
				return;
			}

			if ( OnGround )
			{
				DoJump();
			}
		}

		public override bool ShouldJump()
		{
			return Input.Pressed( InputButton.Jump ) && !IsCovering && !MovingToCover;
		}

		// Leaning against walls
		
		public virtual TraceResult GetCoverDir( Vector3 position, Vector3 direction, float distance )
		{
			int stepRays = 4;
			TraceResult tr = TraceBBox( position, position + direction * distance );
			List<Vector3> normals = new();
			
			if ( tr.StartedSolid )
			{
				TraceResult groundTrace = TraceBBox( position + Vector3.Up * StepSize * 3, position );

				if (groundTrace.StartedSolid && !groundTrace.Hit)
				{
					return default;
				}
				else
				{
					position = groundTrace.EndPosition;
					tr = TraceBBox( position, position + direction * distance );
				}
			}
			
			normals.Add( tr.Normal );
			
			if ( tr.Hit )
			{
				// Do stepRay amount of traces from feet to head
				for ( int j = 0; j < stepRays; j++ )
				{
					TraceResult stepTr = TraceBBoxLift( position, position + direction * distance, j * (GetViewOffset() / stepRays) );

					// Make add to normals
					normals.Add( stepTr.Normal );

					// If not hit then cannot take cover

					if ( !stepTr.Hit || stepTr.StartedSolid )
					{
						tr.Hit = false;
						normals = new List<Vector3> { Vector3.Zero };
						break;
					}
				}

				// Make tr.Normal the most common of the normals list
				tr.Normal = ListHelper.GetMostCommon<Vector3>(normals);
			}

			return tr;
		}

		public virtual TraceResult GetCover()
		{
			float coverDistance = 5;
			Rotation viewRotation = Rotation.From( ViewAngles.WithPitch( 0 ) );
			Vector3[] directions = { viewRotation.Forward, viewRotation.Right, viewRotation.Backward, viewRotation.Left };

			for ( int i = 0; i < directions.Length; i++ )
			{
				TraceResult cover = GetCoverDir( Position, directions[i], coverDistance );

				if (cover.Hit)
				{
					return cover;
				}
			}

			return default;
		}

		// Move to cover until we hit wall
		public virtual void MoveToCover(Vector3 coverPos)
		{
			// Move to cover
			if ( MovingToCover )
			{
				var direction = (coverPos - Position).Normal;
				var tr = TraceBBox( Position, Position + direction * 2	 );
	
				// Move to cover
				WishVelocity = direction * MaxSpeed;
				RotatePlayer( Rotation.From( direction.EulerAngles ) );
	
				// If we hit a wall, stop moving
				if ( tr.Hit || tr.Fraction < 0.3 )
				{
					MovingToCover = false;
					IsCovering = true;
				}

				if (tr.Distance > 100)
				{
					MovingToCover = false;
				}
			}
		}
		
		public virtual void MoveCover()
		{
			TraceResult coverTrace = GetCoverDir( Position, CoverDirection, 5 );
			Rotation moveDirection = coverTrace.Normal.EulerAngles.ToRotation().RotateAroundAxis( Vector3.Up, 90 );
			Vector3 hitEdge = IsAtEdge( CoverDirection, moveDirection );
			

			if ( coverTrace.Hit )
			{
				var playerRotation = coverTrace.Direction.EulerAngles.ToRotation().RotateAroundAxis( Vector3.Up, 180 );

				WishVelocity = WishVelocity.ProjectOnNormal( moveDirection.Forward );
				RotatePlayer( playerRotation );
				Velocity += CoverDirection * 100;
				CoverDirection = -coverTrace.Normal.EulerAngles.WithPitch(0).Forward;
		
				if ( hitEdge.LengthSquared > 0 )
				{
					var towardPlayer = (Position - hitEdge).Normal;
					TraceResult cornerCheck = GetCoverDir( hitEdge, towardPlayer, GetPlayerMaxs().x * 3 );
					float angleThreshold = 100;
					float angle = Math.Abs( CoverDirection.Angle( cornerCheck.Normal ) );
					if ( angle > angleThreshold )
					{
						WishVelocity = WishVelocity.ProjectOnNormal( moveDirection.Forward );
						Velocity += CoverDirection * 100;
					}
					else
					{

						// If wishvelocity is towards the edge
						if ( WishVelocity.Dot( Position - hitEdge ) < 0 )
						{
							WishVelocity = Vector3.Zero;
							Velocity = ApplyFriction( Velocity, 10, 100 );
						}
					}
				}
			}
		}

		// Detect this is the edge of the cover
		public virtual Vector3 IsAtEdge(Vector3 coverDirection, Rotation moveDirection)
		{
			for ( int i = 0; i < 2; i++ )
			{
				float playerHullX = GetPlayerMaxs().x;
				Vector3 direction = moveDirection.RotateAroundAxis( Vector3.Up, 180 * i ).Forward * playerHullX * 3;
				var start = Position + direction - CoverDirection * 5;

				TraceResult tr = GetCoverDir( start, CoverDirection, playerHullX * 1 );

				if ( !tr.Hit && !tr.StartedSolid )
				{
					return tr.EndPosition;
				}
			}

			return Vector3.Zero;
		}

		public override void OnAnimEvent( string name, int intData, float floatData, Vector3 vectorData, string stringData )
		{
		}

		public override void AirMove()
		{
			base.AirMove();

			if ( Input.Down( InputButton.Forward ) || Input.Down( InputButton.Back ) )
				AccelerateAir( Accelerate, 100 );

		}

		public override void FrameSimulate()
		{
			base.FrameSimulate();

			//EyeLocalPosition = Vector3.Up * GetViewOffset();
			Player.DuckComp.FrameSimulate( Client );
			EyeRotation = ViewAngles.ToRotation();
		}

		public override bool StartMove()
		{
			//EyeLocalPosition = Vector3.Up * GetViewOffset();
			EyeRotation = ViewAngles.ToRotation();
			WishVelocity = WishVel( MaxSpeed );

			UpdateBBox();

			Player.PipeComp.Simulate( Client );

			if ( Player.PipeComp.CancelMovement )
			{
				return true;
			}

			Player.RollComp.Simulate( Client );

			if ( Player.RollComp.CancelMovement )
			{
				return true;
			}

			Player.DuckComp.Simulate( Client );

			if (IsDucked)
			{

			}
	
			if ( Input.Pressed(InputButton.Grenade) && GroundEntity != null )
			{
				if ( !IsCovering && !MovingToCover )
				{
					// Get cover
					TraceResult coverTrace = GetCover();
					var distance = coverTrace.Distance;

					if ( distance > 0 )
					{
						MovingToCover = true;
						CoverPos = coverTrace.EndPosition;
						CoverDirection = -coverTrace.Normal;
						CoverDistance = distance;
					}
				}
				else
				{
					IsCovering = false;
					MovingToCover = false;
				}
			}
			
			if ( MovingToCover )
			{
				MoveToCover(CoverPos);
			}
			else if ( IsCovering ) 
			{
				MoveCover();
			}
			else if (GroundEntity != null)
			{
				RotatePlayer();
			}


			Player.LedgeComp.Simulate( Client );

			if ( Player.LedgeComp.CancelMovement )
			{
				return true;
			}

			return false;
		}

		public override bool SetupMove()
		{
			if (base.SetupMove())
				return true;


			// Interpolate walkspeed with Landed


			if ( WasOnGround && !OnGround && Input.Down( InputButton.Walk ) && !IsDucked )
			{
				TraceResult edgeTrace = Trace.Ray(Position - Vector3.Up * 2, Position - Vector3.Up * 2 - Velocity.Normal * 30)
					.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
					.Ignore( Pawn )
					.Run();
				DebugOverlay.TraceResult( edgeTrace, 3 );
				if ( edgeTrace.Hit )
				{
					EdgePos = edgeTrace.EndPosition;
					CanTurnToEdge = true;
				}
			}
			else if ( OnGround && !Input.Down( InputButton.Walk ) )
			{
				CanTurnToEdge = false;
			}

			if ( CanTurnToEdge && !OnGround )
			{
				RotatePlayer( (Position - EdgePos).EulerAngles.ToRotation().RotateAroundAxis( Vector3.Up, 180 ), 0.2f );
				AccelerateAir( Accelerate, 100 );
			}

			return false;
		}

		public override void EndMove()
		{
			WasOnGround = OnGround;
		}
	}
}
