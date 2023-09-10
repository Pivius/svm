using Sandbox;
using SvM.Player.Controller.Base;

namespace SvM.Player.Components
{
	public partial class AccelComponent : BaseNetworkable
	{
		[Net]
		public int Accelerate { get; set; } = 5;

		public static float CapWishSpeed( float wishSpeed, float maxSpeed )
		{
			return MathX.Clamp( wishSpeed, 0, maxSpeed );
		}

		public static float GetVelDiff( Vector3 velocity, float length, Vector3 strafeDir )
		{
			return length - velocity.Dot( strafeDir );
		}

		public static Vector3 GetAccelSpeed( Vector3 strafeDir, float accelerate, float length, float velDiff )
		{
			return strafeDir * MathX.Clamp( length * accelerate * Time.Delta, 0, velDiff );
		}

		public static (Vector3, float) GetFinalVelocity( Vector3 velocity, Vector3 strafeVel, float accelerate, float maxSpeed )
		{
			Vector3 strafeDir = strafeVel.Normal;
			float strafeVelLength = CapWishSpeed( strafeVel.Length, maxSpeed );
			float velDiff = GetVelDiff( velocity, strafeVelLength, strafeDir );
			Vector3 accelSpeed = GetAccelSpeed( strafeDir, accelerate, strafeVelLength, velDiff );

			return (velocity + accelSpeed, velDiff);
		}

		public virtual Vector3 GetControllerVelocity( BaseController controller )
		{
			return GetFinalVelocity
			(
				controller.Velocity,
				controller.WishVelocity,
				Accelerate,
				controller.GetWalkSpeed()
			).Item1;
		}
	}
}
