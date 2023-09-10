using Sandbox;
using System.Collections.Generic;

namespace SvM.Utility
{
	public static class ListHelper
	{
		// Get Most common variable in list
		public static T GetMostCommon<T>( this List<T> list )
		{
			T mostCommon = default;
			int mostCommonCount = 0;

			foreach ( T item in list )
			{
				int count = list.FindAll( x => x.Equals( item ) ).Count;

				if ( count > mostCommonCount )
				{
					mostCommon = item;
					mostCommonCount = count;
				}
			}

			return mostCommon;
		}
	}
}
