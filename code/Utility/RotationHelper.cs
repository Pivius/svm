namespace SvM.Utility
{
	public static class RotationHelper
	{
		public static Rotation LerpTo( this Rotation rotation, Rotation target, float frac )
		{
			Vector3 rotDir = rotation.Forward;
			Vector3 targetDir = target.Forward;
			Vector3 newDir = rotDir.LerpTo( targetDir, frac );

			return newDir.EulerAngles.ToRotation();
		}
	}
}
