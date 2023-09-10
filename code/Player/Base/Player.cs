using Sandbox;
using SvM.Player.Controller.Base;

namespace SvM.Player.Base
{
	public partial class BasePlayer : AnimatedEntity
	{
		[Net, Predicted]
		public TimeUntil DeathTimer { get; set; }
		[Net, Predicted]
		public BaseController Controller { get; set; }
		[Net]
		public int Deaths { get; set; }
		public Vector3 EyeLocalPosition { get; set; }
		public Rotation EyeLocalRotation { get; set; }
		public Rotation EyeRotation
		{
			get => Transform.RotationToWorld( EyeLocalRotation );
			set => EyeLocalRotation = Transform.RotationToLocal( value );
		}

		public Vector3 EyePosition
		{
			get => Transform.PointToWorld( EyeLocalPosition );
			set => EyeLocalPosition = Transform.PointToLocal( value );
		}

		[Net, Predicted]
		public bool IsFirstPerson { get; set; }

		public override void Spawn()
		{
			SetModel( "models/svm.vmdl" );
			
			Controller = new BaseController() {};

			EnableLagCompensation = true;
			Tags.Add( "player" );

			Respawn();
		}

		public virtual void Respawn()
		{
			SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius( 72, 16 ) );
			LifeState = LifeState.Alive;
			Health = 100;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			IsFirstPerson = true;
			SvMGame.Instance?.MoveToSpawnpoint( this );
		}

		public virtual void SimulateAnimator()
		{
			var helper = new CitizenAnimationHelper( this );
			helper.WithVelocity( Velocity );
			helper.WithLookAt( EyePosition + EyeRotation.Forward * 500f );

			if ( Controller.HasEvent( "jump" ) )
			{
				helper.TriggerJump();
			}

			helper.IsGrounded = Controller.GroundEntity.IsValid();
		}

		public override void Simulate( IClient client )
		{
			if ( LifeState == LifeState.Dead )
			{
				if ( DeathTimer > 5.0f )
				{
					Respawn();
				}

				return;
			}

			SimulateAnimator();
			Controller?.Simulate( client, this );
		}

		public override void FrameSimulate( IClient client )
		{
			Camera.Rotation = ViewAngles.ToRotation();
			Camera.Position = EyePosition;
			Camera.ZNear = 1;
			Camera.ZFar = 10000.0f;
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
			Camera.FirstPersonViewer = this;

			Controller?.FrameSimulate( client, this );
		}

		public virtual void OnKilled( Entity killer )
		{
			SvMGame.Instance?.OnKilled( this, killer );
			LifeState = LifeState.Dead;
			Velocity = Vector3.Zero;
		}
	}
}
