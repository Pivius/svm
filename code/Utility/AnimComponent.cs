using Sandbox;
using System.Collections.Generic;

namespace SvM.Utility
{
	public partial class AnimComponent : PredictedComponent
	{
		[Net]
		private float _speed { get; set; }
		[Net]
		public string Animation { get; set; }
		[Net]
		public string SpeedAnim { get; set; }
		[Net, Predicted]
		public TimeSince TimeStarted { get; set; }
		[Net, Predicted]
		public bool IsPlaying { get; set; }
		[Net, Predicted]
		public bool JustEnded { get; set; }
		public float GetFraction
		{
			get => TimeStarted / Duration;
		}
		public float Speed
		{
			get => _speed;
			set => SetSpeed( value );
		}
		public float Duration 
		{ 
			get => Player.GetSequenceDuration( Animation ) / Speed;
		}

		public override void Initialize()
		{
			SetSpeed();
		}

		public virtual void SetAnimation( string animation, string speedAnim = null )
		{
			Animation = animation;
			SpeedAnim = speedAnim;

			if (speedAnim != null)
				SetSpeed();
		}

		public virtual void SetAnimation( string animation, float speed )
		{
			Animation = animation;
			Speed = speed;
		}

		private void SetSpeed()
		{
			if ( SpeedAnim != null )
			{
				_speed = Player.GetAnimParameterFloat( SpeedAnim );
			}
			else
			{
				_speed = 1f;
			}
		}

		private void SetSpeed( float speed )
		{
			if ( SpeedAnim != null ) 
			{
				Player.SetAnimParameter( SpeedAnim, speed );
			}

			_speed = speed;
		}

		private void UpdateParams( 
			Dictionary<string, int> intParams, 
			Dictionary<string, float> floatParams,
			Dictionary<string, bool> boolParams
		)
		{
			if (intParams != null )
			{
				foreach ( var param in intParams )
				{
					Player.SetAnimParameter( param.Key, param.Value );
				}
			}

			if ( floatParams != null )
			{
				foreach ( var param in floatParams )
				{
					Player.SetAnimParameter( param.Key, param.Value );
				}
			}

			if ( boolParams != null )
			{
				foreach ( var param in boolParams )
				{
					Player.SetAnimParameter( param.Key, param.Value );
				}
			}
		}

		public void Start( 
			Dictionary<string, int> intParams = null, 
			Dictionary<string, float> floatParams = null, 
			Dictionary<string, bool> boolParams = null 
		){
			UpdateParams(intParams, floatParams, boolParams);

			IsPlaying = true;
			TimeStarted = 0;
		}

		public void Stop(
			Dictionary<string, int> intParams = null,
			Dictionary<string, float> floatParams = null,
			Dictionary<string, bool> boolParams = null
		)
		{
			UpdateParams( intParams, floatParams, boolParams );

			IsPlaying = false;
			JustEnded = true;
			TimeStarted = Duration;
		}

		public override void Simulate()
		{
			JustEnded = false;

			if (TimeStarted > Duration && IsPlaying)
			{
				IsPlaying = false;
				JustEnded = true;
			}
		}
	}
}
