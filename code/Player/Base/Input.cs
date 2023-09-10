using Sandbox;
using SvM.Utility;

namespace SvM.Player.Base
{
	public partial class BasePlayer
	{
		[ClientInput]
		public Angles ViewAngles { get; set; }
		[ClientInput]
		public Vector3 InputDirection { get; set; }
		[ClientInput]
		private Vector2 PreviousDelta { get; set; }

		public virtual void ScaleSensitivity( ref Angles viewAng, Vector2 prevDelta, Vector2 mouseDelta )
		{
			MouseInput.MouseMove( ref viewAng, ref prevDelta, mouseDelta );
			PreviousDelta = prevDelta;
		}

		public override void BuildInput()
		{
			base.BuildInput();

			InputDirection = Input.AnalogMove;

			if ( Input.StopProcessing )
				return;

			var look = Input.AnalogLook;

			if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
			{
				look = look.WithYaw( look.yaw * -1f );
			}

			var viewAngles = ViewAngles;
			viewAngles += look;
			viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
			viewAngles.roll = 0f;

			Controller?.BuildInput();

			ScaleSensitivity( ref viewAngles,
				PreviousDelta,
				new Vector2( look.yaw * -10, look.pitch * 10 ) );

			ViewAngles = viewAngles.Normal;
		}
	}
}
