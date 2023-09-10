using Sandbox;
using SvM.Player.Controller;
using SvM.Player.Controller.Base;
using SvM.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SvM.Player.Components
{
	public partial class LedgeComponent : PredictedComponent
	{
		[Net, Predicted]
		private float _hangHeight { get; set; }
		[Net, Predicted]
		private bool _canTurnToEdge { get; set; }
		[Net, Predicted]
		private int _shimmyDir { get; set; }
		[Net, Predicted]
		private AnimMoveComponent _shimmy { get; set; }
		[Net, Predicted]
		private AnimMoveComponent _vault { get; set; }
		[Net, Predicted]
		private AnimMoveComponent _hangClimb { get; set; }
		[Net, Predicted]
		private AnimMoveComponent _shortClimb { get; set; }
		[Net, Predicted]
		private AnimMoveComponent _hangClimbDouble { get; set; }
		[Net]
		private IList<float> _climbHeights { get; set; }
		[Net]
		private float _hangHandHeight { get; set; } = 67;
		[Net, Predicted]
		public bool CancelMovement { get; set; }
		[Net, Predicted]
		public bool IsHanging { get; set; }
		[Net, Predicted]
		public bool ShouldStand { get; set; }

		public new SpyController Controller => Player.Controller as SpyController;
		
		private readonly string _shimmyLAnim = "Hang_Left";
		private readonly string _shimmyRAnim = "Hang_Right";
		private readonly string _shimmyInnerLAnim = "Hang_Left_Inner";
		private readonly string _shimmyInnerRAnim = "Hang_Right_Inner";
		private readonly string _shimmyOuterLAnim = "Hang_Left_Outer";
		private readonly string _shimmyOuterRAnim = "Hang_Right_Outer";

		public override void Initialize()
		{
			_climbHeights = new List<float>() { 25, 30, 50, 80 };
			_shimmy = AnimMoveComponent.New( Player, _shimmyLAnim );;
			_shimmy.ShouldMove = false;
			_vault = AnimMoveComponent.New( Player, "Vault" );
			_hangClimb = AnimMoveComponent.New( Player, "Hang_Climb_Fast" );
			_shortClimb = AnimMoveComponent.New( Player, "Hang_Climb_Short" );
			_hangClimbDouble = AnimMoveComponent.New( Player, "Hang_Climb_Double" );
		}

		private bool TraceUp(Vector3 position, float height)
		{
			var radius = Controller.Radius;
			Vector3 size = Controller.GetPlayerMins( false );

			TraceResult trUp = Trace.Ray( position, position + Vector3.Up * height )
				.Size( size, (size * -1).WithZ(1) )
				.WithAnyTags( "solid", "playerclip", "passbullets" )
				.Ignore( Player )
				.Run();

			return !trUp.Hit;
		}

		private Vector3 DownTrace(Vector3 position, float down)
		{
			TraceResult downTrace = TraceHelper.TraceHullResult(
				position,
				position - Vector3.Up * down,
				Controller.GetPlayerMins(),
				Controller.GetPlayerMaxs( true ),
				Player
			);

			return downTrace.EndPosition;
		}

		private TraceResult SendToGround( float down )
		{
			TraceResult downTrace = TraceHelper.TraceHullResult(
				Controller.Position,
				Controller.Position - Vector3.Up * down,
				Controller.GetPlayerMins(),
				Controller.GetPlayerMaxs( true ),
				Player
			);

			Controller.Position = downTrace.EndPosition;

			return downTrace;
		}

		private bool TraceToPosition(Vector3 startPosition, Vector3 endPosition, float traceHeight, out Trace trForward, out TraceResult trResult)
		{
			var radius = Controller.Radius;
			Vector3 mins = Controller.GetPlayerMins(false);
			Vector3 maxs = (mins * -1).WithZ(traceHeight);
			var up = Vector3.Up;

			trForward = TraceHelper.TraceHull(startPosition + up, endPosition + up, mins, maxs, Player);
			trResult = trForward.Run();

			if (trResult.Hit && Vector3.GetAngle(up, trResult.Normal) < Controller.StandableAngle)
			{
				var direction = BaseController.ClipVelocity((endPosition - startPosition).Normal, trResult.Normal);
				trForward = TraceHelper.TraceHull(startPosition + up, startPosition + up + direction * endPosition.Distance(startPosition), mins * -1, maxs, Player);
				trResult = trForward.Run();
			}

			return trResult.Hit && !trResult.StartedSolid;
		}

		private Vector3? CheckClimb(float distance = 15, float edge = 32, float height = 0, bool failTrace = false, bool forwardEdgeTrace = false)
		{
			var traceHeight = 20;
			var up = Vector3.Up;
			float radius = Controller.Radius;
			Vector3 forward = Controller.Rotation.Angles().WithPitch( 0 ).Forward;
			Vector3 position = Controller.Position + up;
			Vector3 mins = Controller.GetPlayerMins( false );
			Vector3 maxs = (mins * -1).WithZ(traceHeight);

			// Trace to front wall
			Trace trForward;
			TraceResult traceResult;

			// TODO: Use list max
			if (height == 0)
			{
				foreach ( var climbHeight in _climbHeights )
				{
					if ( climbHeight > height )
					{
						height = climbHeight;
					}
				}
			}

			// If trace is a ramp then align direction with ramp
			bool traceHit = TraceToPosition(position, position + forward * distance, traceHeight, out trForward, out traceResult);

			// If not hit then we do a trace at position + next climb height and etc.
			if ( !traceHit && failTrace )
			{
				foreach ( var climbHeight in _climbHeights )
				{
					traceHit = TraceToPosition( position + Vector3.Up * climbHeight, position + Vector3.Up * climbHeight + forward * distance, traceHeight, out trForward, out traceResult );
					if ( traceHit )
					{
						break;
					}
				}
			}
			//DebugOverlay.TraceResult( traceResult, 1 );
			// Hit wall and check for if we can climb
			if ( traceHit )
			{
				// Move into the wall the diameter of the player
				// We need to trace to the wall to make sure its not starting in solid
				var edgeDist = edge;
				var endPosition = Controller.Position - (traceResult.Normal * edgeDist);
				var upHeight = up * height;

				if ( forwardEdgeTrace )
				{
					traceResult = trForward.FromTo( Controller.Position + upHeight, endPosition + upHeight )
						.Size( mins, maxs.WithZ( 1 ) )
						.Run();

					if (traceResult.Hit)
						endPosition = Controller.Position - (traceResult.Normal * traceResult.Distance);
				}

				traceResult = trForward.FromTo( endPosition + upHeight, endPosition )
					.Size(mins, maxs.WithZ(1))
					.Run();

				//DebugOverlay.TraceResult( traceResult, 1 );

				if ( traceResult.Hit && !traceResult.StartedSolid && traceResult.Normal.z > 0.9f )
				{
					return traceResult.EndPosition;
				}
			}

			return null;
		}

		private Vector3? CheckVault( Vector3 position, Vector3 direction, float thickness, float height = 0 )
		{
			var radius = Controller.Radius;
			direction = direction * (thickness + radius * 2);
			var startPosition = position + direction;
			Vector3 mins = Controller.GetPlayerMins( false );
			Vector3 maxs = Controller.GetPlayerMaxs( true );
			Trace trForward;
			TraceResult trResult;

			if (height == 0)
			{
				height = _climbHeights[1] * 2;
			}

			trForward = TraceHelper.TraceHull( startPosition, startPosition - (Vector3.Up * height), mins, maxs, Player );
			trResult = trForward.Run();

			float heightDelta = position.z - trResult.EndPosition.z;
			//DebugOverlay.TraceResult( trResult, 1 );
			if ( !trResult.StartedSolid && trResult.Fraction >= 0.5f)
			{
				// Trace back towards the player
				
				trResult = trForward.FromTo(trResult.EndPosition, trResult.EndPosition - direction).Run();
				if ( trResult.Hit )
				{
					return trResult.EndPosition + trResult.Normal * 2;
				}
			}

			return null;
		}

		private int GetClimbLevel( Vector3 endPosition )
		{
			var climbHeight = endPosition.z - Position.z;

			for ( int i = 0; i < _climbHeights.Count; i++ )
			{
				if ( climbHeight <= _climbHeights[i] )
				{
					return i + 1;
				}
			}

			return 0;
		}

		private bool CanClimbOnto( Vector3 climbPosition )
		{
			const int steps = 4;
			float radius = Controller.Radius;
			float distance = radius * 2;
			const float downDistance = 10;
			Vector3 forward = Controller.Rotation.Angles().WithPitch( 0 ).Forward;
			Vector3 position = Controller.Position + Vector3.Up;
			Vector3 mins = Controller.GetPlayerMins( false );

			Trace trForward;
			TraceResult trResult;
			TraceToPosition(position, climbPosition + Vector3.Up + (forward * radius), 1f, out trForward, out trResult);

			if ( trResult.Fraction < 1 )
			{
				return false;
			}

			for ( int i = 0; i < steps; i++ )
			{
				Vector3 vDistance = forward * i * (distance / steps);

				// Trace down to see if we can stand on the climb position
				trForward = trForward.FromTo(position + Vector3.Up + vDistance, climbPosition - (Vector3.Up * downDistance) + vDistance);
				trResult = trForward.Run();

				if ( !trResult.Hit || trResult.StartedSolid )
				{
					return false;
				}
			}

			return true;
		}

		private void TryHangLedge(Vector3 climbPos, Vector3 direction)
		{
			float handHeight = _hangHandHeight;
			Vector3 position = Controller.Position;

			// If the hands are at the right height and we are moving down then hang
			Log.Info( MathX.AlmostEqual( climbPos.z - position.z, handHeight, 1 ) );
	
			if ( MathX.AlmostEqual( climbPos.z - position.z, handHeight, 1 ) && Controller.Velocity.z < 0 && !IsHanging )
			{
				
				if ( TraceUp( position, handHeight ) )
				{
					IsHanging = true;
					Player.SetAnimParameter( "b_hanging", true );
					_hangHeight = climbPos.z;
					Controller.Rotation = direction.EulerAngles.WithPitch( 0 ).ToRotation();
				}
			}
		}

		private void TryShortClimb(Vector3 climbPos, Vector3 direction)
		{
			float forwardSpeed = 145^2;
			Vector3 velocity = Controller.Velocity;

			// If we are moving fast enough and not moving up faster than 100 units then try to short climb
			if ( velocity.WithZ(0).LengthSquared > forwardSpeed && velocity.z > 100 && velocity.z < 200 && !_shortClimb.IsPlaying )
			{
				if ( TraceUp( Controller.Position, _climbHeights[3] ) )
				{
					var paramDict = new Dictionary<string, bool>
					{
						{ "b_climb_short", true }
					};
					ShouldStand = !((SpyPlayer)Player).DuckComp.Ducked;
					((SpyPlayer)Player).DuckComp.Duck( 0.1f );
					_shortClimb.StartAnim( direction, climbPos, 1, boolParams: paramDict );
				}
			}
		}

		private void TryVault(Vector3 climbPos, Vector3 direction)
		{
			var upSpeed = 10;

			// If we are moving up slightly
			if (Controller.Velocity.z > upSpeed && !_vault.IsPlaying )
			{
				var downDistance = 40;
				var vaultThickness = 26;
				var handDelta = 20;
				Vector3? vaultPos = CheckVault( climbPos + Vector3.Up * handDelta, direction, vaultThickness );
				Vector3 position = Controller.Position;
				TraceResult traceLine = TraceHelper.TraceLineResult( position, position - (Vector3.Up * downDistance), Player );

				Log.Info( vaultPos );

				if ( vaultPos != null && traceLine.Hit )
				{
					var paramDict = new Dictionary<string, bool>
					{
						{ "b_vault", true }
					};
					DebugOverlay.TraceResult( traceLine, 3 );
					// TODO: Fix snapping to pos before anim starts
					//Controller.Position = Controller.Position.WithZ( ClimbPos.z - handHeight );
					_vault.StartAnim( direction, (Vector3)vaultPos, 1, boolParams: paramDict );
					Controller.Velocity = Vector3.Zero;
				}
			}
		}

		private void LedgeMove()
		{
			if ( !Controller.OnGround && !IsHanging )
			{
				Vector3? ledge = CheckClimb( edge: 8, failTrace: true, forwardEdgeTrace: true );

				if ( ledge != null && !MathX.AlmostEqual( _hangHeight, ((Vector3)ledge).z, 1 ) )
				{
					Vector3 climbPos = (Vector3)ledge;
					var climbLevel = GetClimbLevel( climbPos );
					var canClimb = CanClimbOnto( climbPos );
					var position = Controller.Position;
					var direction = (climbPos.WithZ( position.z ) - position).Normal;

					//Log.Info( climbLevel );

					if ( climbLevel == 4 )
					{
						// Hanging
						TryHangLedge( climbPos, direction );
					}
					else if ( climbLevel == 3 )
					{
						// Short climb
						TryShortClimb( climbPos, direction );
					}
					else if ( climbLevel == 2 )
					{
						TryVault( climbPos, direction );
					}
				}
				else
				{
					ledge = CheckClimb( edge: 4, height: _climbHeights[1], failTrace: true );

					if ( ledge != null )
					{
						Vector3 climbPos = (Vector3)ledge;
						var climbLevel = GetClimbLevel( climbPos );
						var canClimb = CanClimbOnto( climbPos );
						var position = Controller.Position;
						var direction = (climbPos.WithZ( position.z ) - position).Normal;

						if ( climbLevel == 2 )
						{
							TryVault( climbPos, direction );
						}
					}
				}
			}
		}

		private bool IsShimmyValid(int direction)
		{
			var position = Controller.Position;
			var velocity = Controller.Velocity;
			var shimmyDir = direction == 1 ? Controller.Rotation.Left : Controller.Rotation.Right;
			var shimmyVel = shimmyDir * 50;
			var shimmyDuration = _shimmy.Duration;
			var finalPos = position + (shimmyVel * Time.Delta * (shimmyDuration * 50) );

			Controller.Velocity = shimmyVel;
			Controller.Position = finalPos;
			Controller.TryPlayerMove();

			Vector3? ledge = CheckClimb(30, 8, failTrace: true, forwardEdgeTrace: true );

			Controller.Position = position;

			return ledge != null;
		}
		
		private void TryShimmyAround( Trace traceHull, bool shimmyLeft )
		{
			float shimmyDistance = 30;
			Vector3 position = Controller.Position;
			Rotation rotation = Controller.Rotation;
			Vector3 shimmyDir = (shimmyLeft ? rotation.Left : rotation.Right) ;
			Vector3 shimmyDist = shimmyDir * shimmyDistance;
			Vector3 startPos = position + shimmyDist + (rotation.Forward * (Controller.Radius * 2.5f));
			TraceResult cornerTrace = traceHull.FromTo(startPos, startPos - shimmyDist ).Run();

			if (cornerTrace.Hit && !cornerTrace.StartedSolid)
			{
				Controller.Position = cornerTrace.EndPosition;
				Controller.Rotation = (-cornerTrace.Normal).EulerAngles.ToRotation();

				Vector3? ledge = CheckClimb( shimmyDistance, 4, failTrace: true, forwardEdgeTrace: true );

				Controller.Position = position;
				Controller.Rotation = rotation;
				
				// Found a ledge to shimmy to
				if (ledge != null)
				{
					Vector3 climbPos = (Vector3)ledge;
					var paramDict = new Dictionary<string, int>
					{
						{ "hang_climb_states", shimmyLeft ? 9 : 8 },
					};

					_shimmy.Animation.SetAnimation( shimmyLeft ? _shimmyOuterLAnim : _shimmyOuterRAnim, 1f );
					_shimmy.ShouldMove = true;
					_shimmy.StartAnim( -cornerTrace.Normal, cornerTrace.EndPosition + shimmyDir * 2, 1, intParams: paramDict, autoRotate: false );
					_shimmyDir = -1;
				}	 
			}
		}

		private void TryShimmyTurn( Trace traceHull, bool shimmyLeft)
		{
			const float shimmyDistance = 30f;
			Vector3 position = Controller.Position;
			Rotation rotation = Controller.Rotation;
			Vector3 shimmyDir = (shimmyLeft ? rotation.Left : rotation.Right) * shimmyDistance;
			TraceResult cornerTrace = traceHull.FromTo(position, position + shimmyDir).Run();

			if (cornerTrace.Hit)
			{
				Controller.Position = cornerTrace.EndPosition;
				Controller.Rotation = (-cornerTrace.Normal).EulerAngles.ToRotation();

				Vector3? ledge = CheckClimb( shimmyDistance, 4, failTrace: true, forwardEdgeTrace: true );

				Controller.Position = position;
				Controller.Rotation = rotation;

				if (ledge != null)
				{
					Vector3 climbPos = (Vector3)ledge;
					var paramDict = new Dictionary<string, int>
					{
						{ "hang_climb_states", shimmyLeft ? 7 : 6 },
					};

					_shimmy.Animation.SetAnimation( shimmyLeft ? _shimmyInnerLAnim : _shimmyInnerRAnim, 1f );
					_shimmy.StartAnim( -cornerTrace.Normal, cornerTrace.EndPosition, 1, intParams: paramDict, autoRotate: false );
					_shimmy.ShouldMove = true;
					_shimmyDir = -1;
				}	 
			}
		}

		public void TryShimmy( int direction )
		{
			bool shimmyLeft = direction == 1;
			Rotation rotation = Controller.Rotation;
			Vector3 position = Controller.Position;
			Log.Info( IsShimmyValid( direction ) );
			if ( IsShimmyValid( direction ) )
			{
				var paramDict = new Dictionary<string, float>
				{
					{ "f_shimmy_move", shimmyLeft ? -50f : 50f },
				};

				_shimmy.Animation.SetAnimation( shimmyLeft ? _shimmyLAnim : _shimmyRAnim, 1f );
				_shimmy.StartAnim( Controller.Rotation.Forward, position, 1, floatParams: paramDict );
				_shimmyDir = direction;
				_shimmy.ShouldMove = false;
			}
			else
			{
				// Trace straight left/right to the player
				// If we hit a wall, shimmy inner
				float radius = Controller.Radius;
				Trace traceHull = TraceHelper.TraceHull(
					position, 
					position + (shimmyLeft ? rotation.Left : rotation.Right) * 25, 
					Controller.GetPlayerMins( false ),
					Controller.GetPlayerMaxs( false ),
					Player
				);
				TraceResult cornerTrace = traceHull.Run();

				if (cornerTrace.Hit)
				{
					TryShimmyTurn(traceHull, shimmyLeft);
				}
				else
				{
					TryShimmyAround(traceHull, shimmyLeft);
				}
			}
		}

		private void CheckDoubleClimb()
		{

		}

		public override void FrameSimulate()
		{
		}

		private void AnimSimulate()
		{
			_shimmy.Simulate( Client );
			_shortClimb.Simulate( Client );
			_hangClimb.Simulate( Client );
			_vault.Simulate( Client );
			_hangClimbDouble.Simulate( Client );
		}

		public override void Simulate()
		{
			// Reset hangheight
			if ( Controller.OnGround && !IsHanging )
			{
				_hangHeight = 0;
			}

			// Debug boxes
			foreach ( var height in _climbHeights )
			{
				//DebugOverlay.Box( Controller.Position + new Vector3( -16, -16, 0 ), Controller.Position + new Vector3( 16, 16, height ), Color.White, 0, true );
			}

			AnimSimulate();

			// Cancel shimmy animation
			if (_shimmy.JustEnded)
			{
				Player.SetAnimParameter( "f_shimmy_move", 0 );
			}

			// Is shimmying a corner
			if (_shimmy.IsPlaying && _shimmyDir < 0)
			{
				CancelMovement = true;
				return;
			}

			LedgeMove();

			if (_shortClimb.JustEnded)
			{
				var tr = SendToGround( 2 );

				Controller.GroundEntity = tr.Entity;
				IsHanging = false;

				if ( ShouldStand )
				{
					((SpyPlayer)Player).DuckComp.UnDuck( 0.2f );
				}
			}

			if ( _hangClimb.JustEnded || _hangClimbDouble.JustEnded )
			{
				var tr = SendToGround( 2 );

				Controller.GroundEntity = tr.Entity;
				IsHanging = false;

				if ( ShouldStand )
				{
					((SpyPlayer)Player).DuckComp.UnDuck( 0.2f );
				}
			}

			// Level 3 hanging stuff
			if ( IsHanging )
			{
				var position = Controller.Position;
				var rotation = Controller.Rotation;
				var traceHeight = position.WithZ( _hangHeight - 2 );
				var dist = 30;
				TraceResult ledgeTrace = TraceHelper.TraceLineResult(traceHeight, traceHeight + rotation.Forward.WithZ( 0 ) * dist, Player);

				// Reset velocity 
				Controller.Velocity = Vector3.Zero;

				// Cancel hanging
				if ( !ledgeTrace.Hit || Input.Pressed( InputButton.Back ) )
				{
					Player.SetAnimParameter( "b_hanging", false );
					IsHanging = false;
					Controller.Velocity = Vector3.Zero;
				}
				else if ( !_hangClimb.IsPlaying )
				{
					Controller.Rotation = (-ledgeTrace.Normal).EulerAngles.WithPitch( 0 ).ToRotation();
				}

				// Try shimmy move
				if (!_shimmy.IsPlaying && Input.Down(InputButton.Left))
				{
					TryShimmy(1);
				}
				else if (!_shimmy.IsPlaying && Input.Down(InputButton.Right))
				{
					TryShimmy(0);
				}
				
				// Shimmy moving
				if ( _shimmy.IsPlaying && _shimmyDir > -1 )
				{
					Vector3 shimmyDir = _shimmyDir == 1 ? Controller.Rotation.Left : Controller.Rotation.Right;
					bool isValid = IsShimmyValid( _shimmyDir );

					if ( isValid )
					{
						Controller.Velocity += shimmyDir * 50;
						Controller.TryPlayerMove();
					}
					else
					{
						var paramDict = new Dictionary<string, float>
						{
							{ "f_shimmy_move", 0f },
						};

						_shimmy.Animation.Stop( floatParams: paramDict );
					}
				}

				// Climb up
				if ( Input.Pressed( InputButton.Forward ) && (!_hangClimb.IsPlaying || !_hangClimbDouble.IsPlaying) && !_shimmy.IsPlaying )
				{
					Vector3? climbPos = CheckClimb( edge: Controller.Radius * 2, failTrace: true ) ;
	
					if ( climbPos != null && GetClimbLevel( (Vector3)climbPos ) >= 3 )
					{
						Vector3? canVault = CheckVault( (Vector3)climbPos, rotation.Forward, 10, 30 );

						if ( canVault != null)
						{
							var intParams = new Dictionary<string, int>
							{
								{ "hang_climb_states", 3 }
							};

							var boolParams = new Dictionary<string, bool>
							{
								{ "b_hanging", false }
							};

							ShouldStand = !((SpyPlayer)Player).DuckComp.Ducked;
							((SpyPlayer)Player).DuckComp.Duck();
							_hangClimb.Animation.SetAnimation( "Hang_Climb_Over", 1 );
							_hangClimb.StartAnim( rotation.Forward, DownTrace( (Vector3)canVault, 5 ), intParams: intParams, boolParams: boolParams );
						}
						else
						{
							var intParams = new Dictionary<string, int>
							{
								{ "hang_climb_states", 1 }
							};

							var boolParams = new Dictionary<string, bool>
							{
								{ "b_hanging", false }
							};

							ShouldStand = !((SpyPlayer)Player).DuckComp.Ducked;
							((SpyPlayer)Player).DuckComp.Duck();
							_hangClimb.Animation.SetAnimation( "Hang_Climb_Fast", 1 );
							_hangClimb.StartAnim( rotation.Forward, DownTrace( (Vector3)climbPos, 5 ), intParams: intParams, boolParams: boolParams );
						}
					}
					else
					{
						// Check for ledge between 16-32 units heights and has a fall after
						// Interpolate up then forward to position
						Vector3? ledgePos = CheckClimb(16, 6, _climbHeights.Max() + 40, failTrace: true);
						
						if (ledgePos != null)
						{
							Vector3? canVault = CheckVault( (Vector3)ledgePos , rotation.Forward, 10, 30 );

							if ( canVault != null)
							{
								Vector3 vaultPos = (Vector3)canVault;
								
								var intParams = new Dictionary<string, int>
								{
									{ "hang_climb_states", 2 }
								};

								var boolParams = new Dictionary<string, bool>
								{
									{ "b_hanging", false }
								};

								var heightDelta = Math.Abs(vaultPos.z - _hangHeight);

								ShouldStand = !((SpyPlayer)Player).DuckComp.Ducked;
								((SpyPlayer)Player).DuckComp.Duck();
								_hangClimbDouble.StartAnim( rotation.Forward, DownTrace(vaultPos, heightDelta), intParams: intParams, boolParams: boolParams );
							}
						}
					}
				}

				Controller.Velocity = Vector3.Zero;

				CancelMovement = true;
			}
			else
			{
				CancelMovement = false;
			}
		}
	}
}
