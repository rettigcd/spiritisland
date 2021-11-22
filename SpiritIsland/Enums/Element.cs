using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public enum Element{ 

		// The order these appear here, is the order they are disaplayed in - by using their intrinsict int value

		None,	// none / default / error
		Sun,	// yellow
		Moon,	// white
		Fire,	// orange
		Air,	// purple
		Water,	// blue
		Earth,  // gray 
		Plant,	// green
		Animal,	// red
		Any		// used by Bringer
	};

	public class ElementCounts : CountDictionary<Element> { }

	public static class ElementList {

		public static readonly Element[] AllElements = new Element[] { Element.Sun, Element.Moon, Element.Air, Element.Fire, Element.Water, Element.Earth, Element.Plant, Element.Animal };

		public static CountDictionary<Element> Parse( string elementFormat ) {
			var items = new List<Element>();
			foreach(var singleElementType in elementFormat.Split( ',' )) {
				var (count,el) = GetElementCounts(singleElementType);
				while(count-- > 0)
					items.Add( el );
			}

			return new CountDictionary<Element>( items.ToArray() );
		}

		/// <summary> Reorders elements into 'Standard' order </summary>
		public static string BuildElementString(this CountDictionary<Element> elements, string delimiter = " " ) {
			return elements
				.OrderBy(p=>(int)p.Key)
				.Select(p=>p.Value+" "+p.Key.ToString().ToLower())
				.Join( delimiter ); // comma or space
		}

		static (int,Element) GetElementCounts(string single ) {
			string[] parts = single.Trim().Split( ' ' );
			return parts.Length == 1 
				? (1, ParseEl(parts[0]) ) 
				: (int.Parse( parts[0] ), ParseEl(parts[1]));
		}

		static Element ParseEl( string text ) {
			return (Element)Enum.Parse( typeof( Element ), text, true );
		}

	}

}