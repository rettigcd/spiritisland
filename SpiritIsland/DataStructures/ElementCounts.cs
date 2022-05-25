namespace SpiritIsland;

public class ElementCounts : CountDictionary<Element> {

	#region constructors
	public ElementCounts(){ }

	public ElementCounts(IEnumerable<Element> items):base(items){ }

	public ElementCounts(Dictionary<Element,int> inner):base(inner){ }

	#endregion

	public new ElementCounts Clone() {
		var clone = new ElementCounts();
		foreach(var invader in Keys)
			clone[invader] = this[invader];
		return clone;
	}

	/// <summary> Reorders elements into 'Standard' order </summary>
	public string BuildElementString(string delimiter = " " ) {
		return this
			.OrderBy(p=>(int)p.Key)
			.Select(p=>(p.Value > 1 ? p.Value+" ":"")+p.Key.ToString().ToLower())
			.Join( delimiter ); // comma or space
	}

	public static ElementCounts Parse( string elementFormat ) {
		var items = new List<Element>();
		if(!string.IsNullOrEmpty( elementFormat ))
			foreach(var singleElementType in elementFormat.Split( ',' )) {
				var (count, el) = GetElementCounts( singleElementType );
				while(count-- > 0)
					items.Add( el );
			}

		return new ElementCounts( items.ToArray() );
	}

	static (int, Element) GetElementCounts( string single ) {
		string[] parts = single.Trim().Split( ' ' );
		return parts.Length == 1
			? (1, ParseEl( parts[0] ))
			: (int.Parse( parts[0] ), ParseEl( parts[1] ));
	}

	public static Element ParseEl( string text ) {
		return (Element)Enum.Parse( typeof( Element ), text, true );
	}


}