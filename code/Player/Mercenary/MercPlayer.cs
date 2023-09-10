using Sandbox;
using SvM.Player.Controller;
using SvM.Player.Base;
using SvM.Utility;

namespace SvM.Player
{
	public partial class MercPlayer : BasePlayer
	{
		[Net, Predicted]
		public int CamDistance { get; set; }
		[Net, Predicted]
		public bool LeftAim { get; set; }
		[Net, Predicted]
		public Vector3 AimOffset { get; set; }
		[Net, Predicted]
		public float SideOffset { get; set; }
		public SpotLightEntity Flashlight { get; set; }
		[Net, Predicted]
		public bool FlashlightEnabled { get; set; }
		public override void Spawn()
		{
			SetModel( "citizen/svm.vmdl" );

			Controller = new MercController() { };

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
			EnableHideInFirstPerson = true;
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
		}

		public void ToggleFlashlight()
		{

			if ( !FlashlightEnabled )
			{
				Flashlight = new SpotLightEntity();

				Flashlight.Rotation = ViewAngles.ToRotation();
				Flashlight.Brightness = 1;
				Flashlight.Parent = this;
				Flashlight.Range = 720;
				Flashlight.Enabled = true;
				Flashlight.LightCookie = Texture.Find( "materials/effects/lightcookie.vtex" );
				Flashlight.InnerConeAngle = 10;
				Flashlight.OuterConeAngle = 30;
	
				FlashlightEnabled = true;
			}
			else
			{
				FlashlightEnabled = false;

				if (Flashlight.IsValid())
					Flashlight.Delete();
			}
		}

		public override void Simulate( IClient client )
		{
			if (Input.Pressed(InputButton.Flashlight))
			{
				ToggleFlashlight();
			}

			if ( Flashlight.IsValid() )
			{
				Flashlight.Rotation = ViewAngles.ToRotation();
				Flashlight.Position = EyePosition + EyeRotation.Forward * 10;
			}

			base.Simulate( client );
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
			Camera.Rotation = ViewAngles.ToRotation();
			Camera.Position = EyePosition;
			Camera.ZNear = 0.1f;
			Camera.ZFar = 10000.0f;
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
			Camera.FirstPersonViewer = this;
		}

		public override void FrameSimulate( IClient client )
		{
			SetupCamera();
			//CycleVision();
			Controller?.FrameSimulate( client, this );
		}

		[Event.PreRender]
		public void OnFrame()
		{
		
		}
	}
}
