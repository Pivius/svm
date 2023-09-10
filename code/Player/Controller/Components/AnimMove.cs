using Sandbox;
using SvM.Utility;
using System.Collections.Generic;
using SvM.Player.Base;

namespace SvM.Player.Components
{
	public partial class AnimMoveComponent : PredictedComponent
	{
		// Properties
		[Net, Predicted]
		private bool _autoRotate { get; set; } = true;
		[Net, Predicted]
		public Vector3 StartPos { get; set; }
		[Net, Predicted]
		public Vector3 EndPos { get; set; }
		[Net, Predicted]
		public Rotation StartRot { get; set; }
		[Net, Predicted]
		public Rotation EndRot { get; set; }
		[Net, Predicted]
		public bool CancelMovement { get; set; }
		[Net, Predicted]
		public AnimComponent Animation { get; set; }
		[Net, Predicted]
		public bool ShouldMove { get; set; } = true;
		[Net, Predicted]
		public bool ShouldInterp { get; set; } = true;

		// Reference properties
		public bool IsPlaying { get => Animation.IsPlaying; }
		public bool JustEnded { get => Animation.JustEnded; }
		public float Duration { get => Animation.Duration; }
		public float Time { get => Animation.TimeStarted; }
		public float Fraction { get => Animation.GetFraction; }

		// Methods
		public override void Initialize() {}

		public virtual void SetAnimation(string animation)
		{
			Animation = new AnimComponent()
			{
				Player = Player,
				Animation = animation
			};
		}

		public static AnimMoveComponent New(BasePlayer player, string animation)
		{
			var component = new AnimMoveComponent() { Player = player };

			component.SetAnimation( animation );

			return component;
		}

		public virtual void StartAnim(
			Vector3 direction,
			Vector3 wishPos,
			float rotateSpeed = 1,
			Dictionary<string, int> intParams = null, 
			Dictionary<string, float> floatParams = null, 
			Dictionary<string, bool> boolParams = null,
			bool autoRotate = true
		){
			if (!Animation.IsPlaying)
			{
				Animation.Start( intParams, floatParams, boolParams );
				SetPositions( Controller.Position, wishPos );

				if ( autoRotate )
				{
					_autoRotate = true;
					Controller.RotatePlayer( direction.EulerAngles.WithPitch( 0 ).ToRotation(), rotateSpeed );
				}
				else
				{
					_autoRotate = false;
					SetRotation( Controller.Rotation, direction.EulerAngles.ToRotation() );
				}
			}
		}

		public virtual void SetPositions(Vector3 startPos, Vector3 endPos)
		{
			StartPos = startPos;
			EndPos = endPos;
		}

		public virtual void SetRotation( Rotation startRot, Rotation endRot )
		{
			StartRot = startRot;
			EndRot = endRot;
		}

		public override void Simulate()
		{
			if ( Animation.JustEnded && ShouldMove )
			{
				Controller.Position = EndPos;

				if ( !_autoRotate )
					Controller.Rotation = EndRot;
			}
			
			Animation.Simulate( Client );

			// Interpolate position
			if ( Animation.IsPlaying && ShouldMove && ShouldInterp )
			{
				Controller.Position = StartPos.LerpTo( EndPos, Fraction );

				if ( !_autoRotate )
					Controller.Rotation = Rotation.Lerp( StartRot, EndRot, Fraction );
			}
		}
	}
}
