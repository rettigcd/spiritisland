using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public class InvalidPresenceLocation : Exception {

		public InvalidPresenceLocation(string invalidSpace,string[] allowed)
			:base($"Invalid:{invalidSpace} allowed:"+allowed.Join(","))
		{}
		
	}

	static public class IEnumerableExtensions {
		public static string Join(this IEnumerable<string> items, string glue ) => string.Join(glue,items);
		public static string Join(this IEnumerable<string> items) => string.Join(string.Empty,items);
	}

}
