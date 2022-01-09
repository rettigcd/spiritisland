using System;
using System.Collections.Generic;

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
		Any,	// used by Bringer
	};

	public static class ElementList {

		public static readonly Element[] AllElements = new Element[] { Element.Sun, Element.Moon, Element.Fire, Element.Air, Element.Water, Element.Earth, Element.Plant, Element.Animal };

		public static ElementCounts Parse( string elementFormat ) {
			var items = new List<Element>();
			if(!string.IsNullOrEmpty(elementFormat))
				foreach(var singleElementType in elementFormat.Split( ',' )) {
					var (count,el) = GetElementCounts(singleElementType);
					while(count-- > 0)
						items.Add( el );
				}

			return new ElementCounts( items.ToArray() );
		}

		static (int,Element) GetElementCounts(string single ) {
			string[] parts = single.Trim().Split( ' ' );
			return parts.Length == 1 
				? (1, ParseEl(parts[0]) ) 
				: (int.Parse( parts[0] ), ParseEl(parts[1]));
		}

		public static Element ParseEl( string text ) {
			return (Element)Enum.Parse( typeof( Element ), text, true );
		}

		static public Img GetTokenImg(this Element element) => element switch {
			Element.Sun    => Img.Token_Sun,
			Element.Moon   => Img.Token_Moon,
			Element.Air    => Img.Token_Air,
			Element.Fire   => Img.Token_Fire,
			Element.Water  => Img.Token_Water,
			Element.Plant  => Img.Token_Plant,
			Element.Earth  => Img.Token_Earth,
			Element.Animal => Img.Token_Animal,
			Element.Any	   => Img.Token_Any,
			_              => throw new ArgumentOutOfRangeException(nameof(element)),
		};

		static public Img GetIconImg(this Element element) => element switch {
			Element.Sun    => Img.Icon_Sun,
			Element.Moon   => Img.Icon_Moon,
			Element.Air    => Img.Icon_Air,
			Element.Fire   => Img.Icon_Fire,
			Element.Water  => Img.Icon_Water,
			Element.Plant  => Img.Icon_Plant,
			Element.Earth  => Img.Icon_Earth,
			Element.Animal => Img.Icon_Animal,
			_              => throw new ArgumentOutOfRangeException(nameof(element)),
		};

	}

}