using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public enum Element{ 
		None,	// none / default / error
		Air,	// purple
		Animal,	// red
		Earth,  // gray 
		Fire,	// orange
		Moon,	// white
		Plant,	// green
		Sun,	// yellow
		Water,	// blue

		Any		// used by Bringer
	};

	public static class ElementList {
		public static Element[] Parse( string elementFormat ) {
			var items = new List<Element>();
			foreach(var singleElementType in elementFormat.Split( ',' )) {
				var (count,el) = GetElementCounts(singleElementType);
				while(count-- > 0)
					items.Add( el );
			}

			return items.ToArray();
		}

		static (int,Element) GetElementCounts(string single ) {
			string[] parts = single.Trim().Split( ' ' );
			return parts.Length == 1 
				? (1, ParseEl(parts[0]) ) 
				: (int.Parse( parts[0] ), ParseEl(parts[1]));
		}

		static Element ParseEl( string text ) {
//			try {
				return (Element)Enum.Parse( typeof( Element ), text, true );
			//} catch( Exception ex) {
			//	throw new FormatException($"Unable to parse '{text}' into Element", ex);
			//}

		}
		
		static public bool Contains(this CountDictionary<Element> dict, string subsetElementString) {
			if(string.IsNullOrEmpty(subsetElementString)) throw new ArgumentException("criteria elements cannot be null or empty.");
			var subset = new CountDictionary<Element>( Parse(subsetElementString) );
			return dict.Contains(subset);
		}

	}

}