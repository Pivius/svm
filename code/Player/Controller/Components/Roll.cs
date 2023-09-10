using Sandbox;
using SvM.Player.Controller.Base;
using SvM.Utility;
using System.Collections.Generic;

namespace SvM.Player.Components
{
	public partial class RollComponent : PredictedComponent
	{
		private readonly string _standRollAnim = "Fast_Roll";
		private readonly string _crouchRollAnim = "Crouch_Roll";
		private readonly float StandRollSpeed = 300;
		public readonly float CrouchRollSpeed = 200;
		[Net, Predicted]
		public bool CancelMovement { get; set; }
		[Net, Predicted]
		private AnimMoveComponent _standRoll { get; set; }
		[Net, Predicted]
		private AnimMoveComponent _crouchRoll { get; set; }

		public override void Initialize()
		{
			_standRoll = AnimMoveComponent.New( Player, _standRollAnim );
			_crouchRoll = AnimMoveComponent.New( Player, _crouchRollAnim );

			_standRoll.ShouldMove = false;
			_crouchRoll.ShouldMove = false;
			_standRoll.Animation.Speed = 1.5f;
			_crouchRoll.Animation.Speed = 1.5f;
		}

		public bool IsRolling()
		{
			return _standRoll.IsPlaying || _crouchRoll.IsPlaying;
		}

		public bool WillRollIntoWall()
		{
			var up = Vector3.Up;
			var velocity = Controller.Velocity;
			var rotation = Controller.Rotation;
			var position = Controller.Position + up * 5;
			var isDucked = ((SpyPlayer)Player).DuckComp.Ducked;
			var rollSpeed = isDucked ? CrouchRollSpeed : StandRollSpeed;
			var rollVelocity = (rotation.Forward.WithZ( 0 ) * rollSpeed).WithZ( velocity.z );
			var rollAnim = isDucked ? _crouchRoll : _standRoll;
			var endPosition = position + (rollVelocity * Time.Delta * rollAnim.Duration * 25);
			var mins = Controller.GetPlayerMins();
			var maxs = Controller.GetPlayerMaxs(true);
			var trace = TraceHelper.TraceHull( position, endPosition, mins, maxs, Player);
			var trResult = trace.Run();

			if ( trResult.Hit && Vector3.GetAngle( up, trResult.Normal ) < Controller.StandableAngle )
			{
				var direction = BaseController.ClipVelocity( (endPosition - position).Normal, trResult.Normal );

				trace = trace.FromTo( position + up, position + up + direction * endPosition.Distance( position ) );
				trResult = trace.Run();
			}

			return trResult.Hit;
		}

		private void TryRoll()
		{
			var velocity = Controller.Velocity;
			var rotation = Controller.Rotation;

			if ( Input.Pressed( InputButton.Run ) && velocity.Dot( rotation.Forward ) > 0 && velocity.LengthSquared > 0 && !IsRolling() && Controller.OnGround && !WillRollIntoWall() )
			{
				// Crouch roll or stand roll
				if ( ((SpyPlayer)Player).DuckComp.Ducked )
				{
					var paramDict = new Dictionary<string, bool>
					{
						{ "crouch_roll", true },
					};

					_crouchRoll.StartAnim( rotation.Forward, Controller.Position, 1, boolParams: paramDict, autoRotate: false );
				}
				else
				{
					var paramDict = new Dictionary<string, bool>
					{
						{ "fast_roll", true },
					};
					((SpyPlayer)Player).DuckComp.Duck( 0.2f );
					_standRoll.StartAnim( rotation.Forward, Controller.Position, 1, boolParams: paramDict, autoRotate: false );
				}
			}
		}

		public override void Simulate()
		{
			var velocity = Controller.Velocity;
			var rotation = Controller.Rotation;

			_standRoll.Simulate( Client );
			_crouchRoll.Simulate( Client );

			if (_standRoll.Animation.JustEnded )
			{

				((SpyPlayer)Player).DuckComp.UnDuck( 0.2f );
			}

			TryRoll();

			if ( IsRolling() )
			{
				if ( _standRoll.IsPlaying )
				{
					Controller.Velocity = (rotation.Forward.WithZ( 0 ) * StandRollSpeed).WithZ( velocity.z );
					((SpyPlayer)Player).DuckComp.Simulate();
				}
				else if ( _crouchRoll.IsPlaying )
				{
					Controller.Velocity = (rotation.Forward.WithZ( 0 ) * CrouchRollSpeed).WithZ( velocity.z );
				}

				Controller.Velocity -= new Vector3( 0, 0, Controller.Gravity * Time.Delta );
				Controller.StepMove();
				CancelMovement = true;
			}
			else
			{
				CancelMovement = false;
			}
		}
	}
}
