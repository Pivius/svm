using Sandbox;

namespace SvM.Utility
{
	public static class TraceHelper
	{
		public static Trace TraceLine( Vector3 startPos, Vector3 endPos, Entity ignore )
		{
			return Trace.Ray( startPos, endPos )
				.WithAnyTags( "solid", "playerclip", "passbullets" )
				.Ignore( ignore );
		}

		public static TraceResult TraceLineResult( Vector3 startPos, Vector3 endPos, Entity ignore )
		{
			return TraceLine( startPos, endPos, ignore ).Run();
		}

		public static Trace TraceHull( Vector3 startPos, Vector3 endPos, Vector3 mins, Vector3 maxs, Entity ignore )
		{
			return TraceLine( startPos, endPos, ignore ).Size( mins, maxs );
		}
		public static TraceResult TraceHullResult( Vector3 startPos, Vector3 endPos, Vector3 mins, Vector3 maxs, Entity ignore )
		{
			return TraceHull( startPos, endPos, mins, maxs, ignore ).Run();
		}
	}
}
