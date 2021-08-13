using System;
using System.Collections.Generic;

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
				string[] parts = singleElementType.Trim().Split( ' ' );
				int count = int.Parse( parts[0] );
				Element el = (Element)Enum.Parse( typeof( Element ), parts[1], true );
				while(count-- > 0)
					items.Add( el );
			}

			return items.ToArray();
		}

	}

}