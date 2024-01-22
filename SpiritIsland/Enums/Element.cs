namespace SpiritIsland;

public enum Element{ 

	// The order these appear here, is the order they are disaplayed in - by using their intrinsict int value

	None = 0,	// none / default / error

	Sun = 1,	    // yellow
	Moon = 2,	    // white
	Fire = 4,	    // orange
	Air = 8,	    // purple
	Water = 16,	    // blue
	Earth = 32,     // gray 
	Plant = 64,	    // green
	Animal = 128,	// red

	Any           = 256 + 1 + 2 + 4 + 8 + 16 + 32 + 64 + 128,	// used by Bringer
};

/// <summary>
/// Some elements are multi-element - they can act as 1 of their parts, delayed choice
/// </summary>
public static class MultiElements {

	const int MultiMarker = 256; // Marker indicating Element has Multiple 'OR' Elements in it.

	public static Element Build( params Element[] singles ) {
		int total = MultiMarker;
		foreach(Element single in singles)
			total |= (int)single;
		return (Element)total;
	}

	public static Element[] GetMultiElements(this CountDictionary<Element> elements) 
		=> elements.Keys.Where( MultiElements.IsMulti ).ToArray();

	public static IEnumerable<Element> SplitIntoSingles( this Element multiEl ) 
		=> ElementList.AllElements.Where( el => ((int)el & (int)multiEl) != 0 );

	public static bool IsMulti(this Element el) => MultiMarker < (int)el;

	public static bool HasSingle(this Element multi, Element single) => ((int)multi & (int)single) != 0;
}

public static class ElementList {

	public static readonly Element[] AllElements = [ Element.Sun, Element.Moon, Element.Fire, Element.Air, Element.Water, Element.Earth, Element.Plant, Element.Animal ];

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

public static class ElementStrings {

	/// <summary> sort elements into 'Standard' order </summary>
	public static string BuildElementString(this CountDictionary<Element> elements, bool showOneCount=true) {
		string ShowLabel( int value ) => (showOneCount || 1 < value) ? value + " " : "";
		return ElementList.AllElements
			.Where( el => 0 < elements[el] )
			.Select(el => ShowLabel( elements[el]) + el.ToString().ToLower() )
			.Join(" "); // comma or space
	}

	/// <summary>
	/// Parses a comma(,)-delimited string of elements
	/// </summary>
	/// <example>1 air,2 fire,animal</example>
	public static CountDictionary<Element> Parse( string elementFormat ) {
		var counts = new CountDictionary<Element>();
		if(!string.IsNullOrEmpty( elementFormat ))
			foreach(var singleElementType in elementFormat.Split( ',' )) {
				var (count, el) = ParseSingleElementCounts( singleElementType );
				counts[el] += count;
			}

		return counts;
	}

	static (int, Element) ParseSingleElementCounts( string single ) {
		string[] parts = single.Trim().Split( ' ' );
		return parts.Length == 1
			? (1, ParseEl( parts[0] ))
			: (int.Parse( parts[0] ), ParseEl( parts[1] ));
	}

	public static Element ParseEl( string text ) {
		return (Element)Enum.Parse( typeof( Element ), text, true );
	}

}