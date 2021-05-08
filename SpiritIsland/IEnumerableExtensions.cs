using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	static public class IEnumerableExtensions {
		public static string Join(this IEnumerable<string> items, string glue ) => string.Join(glue,items);
		public static string Join(this IEnumerable<string> items) => string.Join(string.Empty,items);

		public static T VerboseSingle<T>(this IEnumerable<T> items, Func<T,bool> predicate){
			var result = items.Where(predicate).ToList();
			if( result.Count == 1 ) return result[0];

			string name = typeof(T).Name;
			throw new InvalidOperationException($"Expected 1 but found {result.Count} items of type {name}");
		}

		public static T[] Include<T>(this IEnumerable<T> list1, params T[] list2) 
			=> list1.Union(list2).ToArray();

	}

}
