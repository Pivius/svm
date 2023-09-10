using Sandbox;
using SvM.Player.Base;
using SvM.Player.Controller.Base;

namespace SvM.Utility
{
	public partial class PredictedComponent : Entity
	{
		public BasePlayer Player 
		{ 
			get => Owner as BasePlayer;
			set => SetOwner( value );
		}

		public BaseController Controller { get; set; }

		public void SetOwner( Entity player )
		{
			if ( player != Owner || Controller != (player as BasePlayer).Controller ) 
			{
				Owner = player;
				Parent = player;
				Controller = (player as BasePlayer).Controller;
				Initialize();
			}
		}

		public override void Spawn()
		{
			EnableLagCompensation = true;
			Transmit = TransmitType.Owner;
		}

		public virtual void Initialize() { }

		public virtual void Simulate() { }
		public virtual void FrameSimulate() { }
		public override void BuildInput() { }
		public virtual void OnAnimEvent( string name, int intData, float floatData, Vector3 vectorData, string stringData ) { }

		public virtual void BuildInput( IClient client ) 
		{
			SetOwner( client.Pawn as Entity );
			BuildInput();
		}

		public override void Simulate( IClient client )
		{
			SetOwner( client.Pawn as Entity );
			Simulate();
		}

		public override void FrameSimulate( IClient client )
		{
			SetOwner( client.Pawn as Entity );
			FrameSimulate();
		}
	}
}
