using Sandbox;
using System;

namespace SvM.Utility
{
	public static class MouseInput
	{
		public static float GetConCommand( string command )
		{
			var conCommand = ConsoleSystem.GetValue( command );

			return conCommand != null ? float.Parse( conCommand ) : 0f;
		}

		private static void FilterMouse( ref Vector2 deltaIn, ref Vector2 deltaPrev )
		{
			bool mouseFilter = GetConCommand( "m_filter" ) == 1;

			// Apply filtering?
			if ( mouseFilter )
			{
				// Average over last two samples
				deltaIn += deltaPrev * 0.5f;
			}

			// Latch previous
			deltaPrev = deltaIn;
		}

		private static void ScaleMouse( ref Vector2 delta )
		{
			float dx = delta.x;
			float dy = delta.y;
			float mouseSens = GetConCommand( "sensitivity" );
			float customAccel = GetConCommand( "m_customaccel" );

			if ( customAccel is 1.0f or 2.0f )
			{
				float rawMouseMovementDist = MathF.Sqrt( dx * dx + dy * dy );
				float accelScale = GetConCommand( "m_customaccel_scale" );
				float accelSensMax = GetConCommand( "m_customaccel_max" );
				float accelSensExponent = GetConCommand( "m_customaccel_exponent" );
				float accelSens = (MathF.Pow( rawMouseMovementDist,
					accelSensExponent ) * accelScale) + mouseSens;

				if ( accelSensMax > 0.0001f && accelSens > accelSensMax )
					accelSens = accelSensMax;

				delta *= accelSens;

				// Further re-scale by yaw and pitch magnitude if user requests alternate mode 2/4
				// This means that they will need to up their value for m_customaccel_scale greatly (>40x) since m_pitch/yaw default
				// to 0.022
				if ( customAccel is 2.0f or 4.0f )
				{
					delta.x *= GetConCommand( "m_yaw" );
					delta.y *= GetConCommand( "m_pitch" );
				}
			}
			else if ( customAccel == 3.0f )
			{
				float rawMouseMoveDistSqrt = dx * dx + dy * dy;
				float fExp = MathF.Max( 0.0f, (GetConCommand( "m_customaccel_exponent" ) - 1.0f) / 2.0f );
				float accelSens = MathF.Pow( rawMouseMoveDistSqrt, fExp ) * mouseSens;

				delta *= accelSens;
			}
			else
			{
				delta *= mouseSens;
			}

			if ( float.IsNaN( delta.x ) || float.IsInfinity( delta.x ) )
				delta.x = 0.0f;

			if ( float.IsNaN( delta.x ) || float.IsInfinity( delta.x ) )
				delta.y = 0.0f;
		}

		private static void AdjustView( ref Angles viewAng, Vector2 delta )
		{
			viewAng -= new Angles( GetConCommand( "m_pitch" ) * (-delta.y), GetConCommand( "m_yaw" ) * delta.x, 0 );
			viewAng = viewAng.WithPitch( MathX.Clamp( viewAng.pitch, -85, 85 ) );
		}

		public static void MouseMove( ref Angles viewAng, ref Vector2 prevDelta, Vector2 delta )
		{
			FilterMouse( ref delta, ref prevDelta );
			ScaleMouse( ref delta );
			AdjustView( ref viewAng, delta );
		}
	}
}
