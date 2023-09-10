using Sandbox;
using SvM.Ent;
using SvM.Player.Controller;
using SvM.Player.Controller.Base;
using SvM.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Intrinsics.Arm;

namespace SvM.Player.Components
{
	public partial class PipeComponent : PredictedComponent
	{
		[Net, Predicted]
		private AnimMoveComponent _climb { get; set; }

		[Net, Predicted]
		public bool IsClimbing { get; set; }
		[Net, Predicted]
		public bool ClimbDirUp { get; set; }
		[Net, Predicted]
		public bool CancelMovement { get; set; }
		[Net, Predicted]
		public TimeSince LastMount { get; set; }
		[Net]
		public float Cooldown { get; set; } = 1f;
		[Net, Predicted]
		public bool CanClimb { get; set; } = false;
		[Net, Predicted]
		public PipeEntity Pipe { get; set; }
		[Net, Predicted]
		public bool IsLeftIdle { get; set; }
		[Net]
		public float ClimbDist { get; set; } = 30f;

		public new SpyController Controller => Player.Controller as SpyController;

		public override void Initialize()
		{
			_climb = AnimMoveComponent.New( Player, "Pipe_Up_Left" ); ;
			//_climb.ShouldMove = false;
		}

		public void TryClimbPipe()
		{
			var player = Player;

			var trace = Controller.TraceBBox( Controller.Position, Controller.Position + Controller.ViewAngles.Forward, 2 );

			DebugOverlay.TraceResult( trace );
			if ( trace.Hit && trace.Entity is PipeEntity pipe )
			{
				if ( !IsClimbing )
				{
					if ( Input.Down( InputButton.Forward ) )
					{
						Player.SetAnimParameter( "b_pipe_up", true );
						Player.SetAnimParameter( "b_pipe", true );
						Player.SetAnimParameter( "b_pipe_left", false );
						Player.SetAnimParameter( "b_pipe_move", false );
						IsLeftIdle = false;
						IsClimbing = true;
						ClimbDirUp = pipe.Direction == PipeEntity.Flags.Vertical;
						CancelMovement = true;
						Pipe = pipe;
					}
				}
			}
		}

		public void CancelClimb()
		{
			Player.SetAnimParameter( "b_pipe", false );
			Player.SetAnimParameter( "b_pipe_up", false );
			IsClimbing = false;
			CancelMovement = false;
			LastMount = 0;
			Controller.Position = Controller.Position + Controller.Rotation.Backward.WithZ( 0 ) * 10;
		}

		public override void Simulate()
		{
			if ( !IsClimbing && Cooldown <= LastMount )
			{
				TryClimbPipe();
			}
			else
			{
				_climb.Simulate( Client );

				if ( !CanClimb )
				{
					var pipePos = Pipe.Position.WithZ( Controller.Position.z );
					var backDist = 7;
					var toRot = Rotation.From( (pipePos - Controller.Position).Normal.EulerAngles );
					var toPos = pipePos + Controller.Rotation.Backward.WithZ( 0 ) * backDist;

					if ( Controller.Rotation == toRot )
					{
						Controller.Position = Controller.Position.LerpTo( pipePos + Controller.Rotation.Backward.WithZ( 0 ) * backDist, 0.5f );
					}
					else
					{
						Controller.RotatePlayer( toRot, 0.5f );
					}

					if ( Controller.Position.AlmostEqual( toPos, 1 ) )
					{
						CanClimb = true;
					}
				}
				else
				{
					var isLeftIdle = Player.GetAnimParameterBool( "b_pipe_left" );
					var backDist = 7;
					var position = Controller.Position;
					var pipePos = Pipe.Position.WithZ( position.z );
					var toRot = Rotation.From( (Pipe.Position.WithZ( 0 ) - position.WithZ(0)).Normal.EulerAngles );
					
					Controller.RotatePlayer( toRot, 0.5f );
					

					if (Input.Down(InputButton.Left))
					{
						var leftTrace = TraceHelper.TraceLineResult( Controller.Position, Controller.Position + Vector3.Left * Controller.Radius, Player );

						if ( !leftTrace.Hit)
							Controller.RotatePlayer( Controller.Rotation.RotateAroundAxis(Vector3.Up, -90), 0.01f );
					}
					else if (Input.Down( InputButton.Right))
					{
						var rightTrace = TraceHelper.TraceLineResult( Controller.Position, Controller.Position + Vector3.Right * Controller.Radius, Player );

						if ( !rightTrace.Hit )
							Controller.RotatePlayer( Controller.Rotation.RotateAroundAxis( Vector3.Up, 90 ), 0.01f );
					}

					Controller.Position = position.LerpTo( pipePos + Controller.Rotation.Backward.WithZ( 0 ) * backDist, 0.5f );

					position = Controller.Position;

					if (Input.Down(InputButton.Forward))
					{
						Player.SetAnimParameter( "b_pipe_up", true );
						Player.SetAnimParameter( "b_pipe_move", true );
						_climb.StartAnim( Controller.Rotation.Forward, position.WithZ( position.z + ClimbDist ), 1, autoRotate: false );
					}
					else
					{
						Player.SetAnimParameter( "b_pipe_move", false );
					}

					var downTrace = TraceHelper.TraceLineResult( Controller.Position, Controller.Position - Vector3.Up * 10, Player );
					DebugOverlay.TraceResult( downTrace );
					if ( Input.Down( InputButton.Back ) && !downTrace.Hit )
					{
						Player.SetAnimParameter( "b_pipe_up", false );
						Player.SetAnimParameter( "b_pipe_move", true );
						_climb.StartAnim( Controller.Rotation.Forward, position.WithZ( position.z - ClimbDist ), 1, autoRotate: false );
					}

					if ( Input.Released( InputButton.Back ) || Input.Released( InputButton.Forward ) )
					{
						Player.SetAnimParameter( "b_pipe_move", false );
					}
				}

				

				if ( Input.Pressed( InputButton.Jump )  )
				{
					Controller.Velocity = Vector3.Zero;
					CancelClimb();
				}
			}
		}
	}
}
