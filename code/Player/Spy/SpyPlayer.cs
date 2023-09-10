using Sandbox;
using SvM.Player.Controller;
using SvM.Player.Base;
using SvM.RenderHooks;
using SvM.Utility;
using System;
using SvM.Ent;
using System.Linq;

namespace SvM.Player
{
	public partial class SpyPlayer : BasePlayer
	{
		[Net, Predicted]
		public int CamDistance { get; set; }
		[Net, Predicted]
		public bool LeftAim { get; set; }
		[Net, Predicted]
		public Vector3 AimOffset { get; set; }
		[Net, Predicted]
		public float SideOffset { get; set; }
		[Net, Predicted]
		public bool IsInteracting { get; set; }
		[Net, Predicted]
		public ObjectiveEntity UsingEnt { get; set; }

		public override void Spawn()
		{
			SetModel( "citizen/svm.vmdl" );

			Controller = new SpyController() { };
			
			EnableLagCompensation = true;
			CreateDefaultComponents();
			Tags.Add( "player" );
			Respawn();
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();
		}

		public override void Respawn()
		{
			base.Respawn();
			CamDistance = 50;
			LeftAim = true;
			SideOffset = 10;
			EnableHideInFirstPerson = false;
			IsFirstPerson = false;
		}
		
		public virtual void OffsetAim()
		{
			// Interpolate aim offset
			if ( LeftAim )
			{
				AimOffset = AimOffset.LerpTo( new Vector3( 0, SideOffset, 0 ), Time.Delta * 10 );
			}
			else
			{
				AimOffset = AimOffset.LerpTo( new Vector3( 0, -SideOffset, 0 ), Time.Delta * 10 );
			}
		}

		public override void SimulateAnimator()
		{
			base.SimulateAnimator();

			//SetAnimParameter( "scale_height", 1f );

			if ( Controller.HasEvent( "duck" ) )
			{
				SetAnimParameter( "duck", 2f );
			}

			if ( Controller.HasEvent( "stand" ) )
			{
				SetAnimParameter( "duck", 0f );
			}

			if ( Controller.HasEvent( "reset_movey" ) )
			{
				SetAnimParameter( "f_shimmy_move", 0f );
			}

			if ( Controller.HasEvent( "shimmy_left" ) )
			{
				SetAnimParameter( "f_shimmy_move", -50f );
			}

			if ( Controller.HasEvent( "shimmy_right" ) )
			{
				SetAnimParameter( "f_shimmy_move", 50f );
			}
		}

		public override void Simulate( IClient client )
		{
			if (Input.Pressed(InputButton.Use) && Game.IsServer)
			{
				var radius = 50;
				//DebugOverlay.Sphere( Position + Vector3.Up * 30, radius, Color.White, 2 );
				var ents = FindInSphere( Position + Vector3.Up * 30, radius );
				ObjectiveEntity closestEnt = null;
				float closestDist = 9999;

				//find the closest entity and only count it if the type is objectiveentity
				foreach (var ent in ents)
				{
					Log.Info( ent );
					if (ent is ObjectiveEntity)
					{
						if (closestEnt == null)
						{
							closestEnt = ent as ObjectiveEntity;
							closestDist = closestEnt.Position.Distance(Position);
						}
						else
						{
							if (closestDist > ent.Position.Distance(Position))
							{
								closestEnt = ent as ObjectiveEntity;
								closestDist = closestEnt.Position.Distance(Position);
							}
						}
					}
				}

				if ( closestEnt != null )
				{
					DebugOverlay.Sphere( closestEnt.Position, 10, Color.Blue, 2 );
					closestEnt.OnUse( this );
				}
				
			}

			if ( IsInteracting )
			{
				Log.Info( UsingEnt.TimeSince );
				if ( Input.Pressed(InputButton.Forward) || Input.Pressed( InputButton.Back ) || Input.Pressed( InputButton.Left ) || Input.Pressed( InputButton.Right ) )
				{
					UsingEnt.StopUse();
					StopInteracting();
				}

				return;
			}

			base.Simulate( client );
			OffsetAim();
		}

		public void StopInteracting()
		{
			IsInteracting = false;
			UsingEnt = null;
		}

		public void StartInteract(ObjectiveEntity ent)
		{
			IsInteracting = true;
			UsingEnt = ent;
		}

		public override void OnAnimEventGeneric( string name, int intData, float floatData, Vector3 vectorData, string stringData )
		{
			Controller.OnAnimEvent( name, intData, floatData, vectorData, stringData );
		}

		private bool InSolid()
		{
			return Controller.TraceBBox(Position, Position).StartedSolid;
		}

		private void SetupCamera()
		{
			var forward = ViewAngles.Forward;
			var forwardRot = Rotation.Forward;
			var transform = GetBoneTransform( 1 );
			var up = Vector3.Up * 5;
			var size = new Vector3( 4, 4, 2f );
			var position = EyePosition - forwardRot * 5 + up;
			var direction = position - Camera.Position;

			Trace traceHull = TraceHelper.TraceHull( position, position - forward * CamDistance, -size, size, this );
			TraceResult traceResult = traceHull.Run();
			//DebugOverlay.Box( traceResult.StartPosition - size, traceResult.StartPosition + size );
			for ( int i = 0; i < 100; i++ )
			{
				if ( traceResult.StartedSolid )
				{
					traceHull = traceHull.FromTo( traceResult.StartPosition - direction, transform.Position - forward * CamDistance + up );
					traceResult = traceHull.Run();
				}
				else
				{
					break;
				}
			}

			Camera.Rotation = ViewAngles.ToRotation();
			Camera.Position = Camera.Position.LerpTo( traceResult.EndPosition + forward * 1, 0.1f);
			Camera.ZNear = 0.1f;
			Camera.ZFar = 10000.0f;
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
			Camera.FirstPersonViewer = this;
		}

		public override void FrameSimulate( IClient client )
		{
			SetupCamera();
			CycleVision();

			if ( CurrentVision != null)
				CurrentVision.OnFrame();

			Controller?.FrameSimulate( client, this );
		}

		[Event.PreRender]
		public void OnFrame()
		{
		
		}
	}
}
