﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiritIsland {

	static public class IEnumerableExtensions {
		public static string Join(this IEnumerable<string> items) => string.Join(string.Empty,items);
		public static string Join(this IEnumerable<string> items, string glue ) => string.Join(glue,items);
		public static string Join_WithLast(this IEnumerable<string> items, string glue, string lastGlue ) {
			var itemArray = items.ToArray();
			int last = itemArray.Length-1;
			var buf = new StringBuilder();
			for(int i=0;i<itemArray.Length;++i) {
				if(i > 0)
					buf.Append( i==last ? lastGlue : glue);
				buf.Append( itemArray[i] );
			}
			return buf.ToString();
		}

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

		static public void Shuffle<T>( this Random randomizer, IList<T> list ) {
			int n = list.Count;
			while(n > 1) {
				n--;
				int k = randomizer.Next( n + 1 );
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		// shorter syntax:
		// space.Terrain.IsIn(Terrain.Wetland,Terrain.Sand)
		// vs.
		// new Terraion[]{Terrain.Wetland,Terrain.Sand}.Contains(space.Terrain);
		static public bool IsOneOf<T>( this T needle, params T[] haystack ) where T : Enum
			=> haystack.Contains( needle );

		static public bool IsOneOf( this TokenClass needle, params TokenClass[] haystack )
			=> haystack.Contains( needle );

		static public void SetItems<T>(this List<T> list, params T[] items ) { list.Clear(); list.AddRange(items);}
		static public void SetItems<T>(this HashSet<T> hashSet, params T[] items ) { hashSet.Clear(); foreach(var item in items) hashSet.Add(item);}

		/// <summary>
		/// [0] => top of stack, [^1] => bottom of stack
		/// </summary>
		static public void SetItems<T>(this Stack<T> stack, params T[] saved ) { 
			stack.Clear(); 				
			for(int i=saved.Length;i-->0;) 
//			for(int i=0;i<saved.Length;++i) 
				stack.Push(saved[i]);
		}

	}

}
