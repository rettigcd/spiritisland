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

		public static T VerboseSingle<T>(this IEnumerable<T> items,string msg){
			var result = items.ToList();
			if( result.Count == 1 ) return result[0];

			string name = typeof(T).Name;
			throw new InvalidOperationException($"{msg} Expected 1 but found {result.Count} items of type {name}");
		}

		public static void AddCount<T>(this List<T> list, int count, T item){
			while(count-->0)
				list.Add(item);
		}

		static readonly Random rng = new Random();
		static public void Shuffle<T>( this IList<T> list ) {
			int n = list.Count;
			while(n > 1) {
				n--;
				int k = rng.Next( n + 1 );
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

	}

}
